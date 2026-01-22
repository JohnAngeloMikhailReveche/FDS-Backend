using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentService2.Services;

/// <summary>
/// PayMongo API client for payment links and webhook management
/// </summary>
public interface IPayMongoService
{
    Task<PaymentLinkResult> CreatePaymentLinkAsync(decimal amount, string description, string referenceId, string? redirectUrl = null, string? cancelUrl = null);
    Task<WebhookResult> RegisterOrUpdateWebhookAsync(string webhookUrl);
    Task<bool> VerifyWebhookSignature(string payload, string signature);
    Task<PayMongoLinkData?> GetCheckoutSessionAsync(string sessionId);
}

public class PayMongoService : IPayMongoService
{
    private readonly HttpClient _httpClient;
    private readonly string _secretKey;
    private readonly string _apiUrl;
    private readonly ILogger<PayMongoService> _logger;
    private string? _webhookSecretKey;

    public PayMongoService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<PayMongoService> logger)
    {
        _secretKey = Environment.GetEnvironmentVariable("PAYMONGO_SECRET_KEY")
            ?? configuration["PayMongo:SecretKey"]
            ?? throw new InvalidOperationException("PayMongo secret key not configured");

        _apiUrl = Environment.GetEnvironmentVariable("PAYMONGO_BASE_URL")
            ?? configuration["PayMongo:BaseUrl"]
            ?? "https://api.paymongo.com/v1";

        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;

        // Set up Basic Auth
        var authBytes = Encoding.ASCII.GetBytes($"{_secretKey}:");
        var authHeader = Convert.ToBase64String(authBytes);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);
    }

    /// <summary>
    /// Retrieves a Checkout Session by ID
    /// </summary>
    public async Task<PayMongoLinkData?> GetCheckoutSessionAsync(string sessionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/checkout_sessions/{sessionId}");
            var body = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get checkout session: {Response}", body);
                return null;
            }

            var result = JsonSerializer.Deserialize<PayMongoLinkResponse>(body, JsonOptions);
            return result?.Data;
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error getting checkout session");
             return null;
        }
    }

    /// <summary>
    /// Creates a PayMongo Checkout Session (better than Links for redirects)
    /// </summary>
    public async Task<PaymentLinkResult> CreatePaymentLinkAsync(decimal amount, string description, string referenceId, string? redirectUrl = null, string? cancelUrl = null)
    {
        try
        {
            var amountInCentavos = (long)(amount * 100);

            // Checkout Session Payload
            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        line_items = new[]
                        {
                            new 
                            { 
                                currency = "PHP",
                                amount = amountInCentavos,
                                description = description,
                                name = "Top-up Credit",
                                quantity = 1
                            }
                        },
                        payment_method_types = new[] { "gcash", "paymaya", "card", "grab_pay" },
                        reference_number = referenceId,
                        success_url = redirectUrl,
                        cancel_url = cancelUrl ?? redirectUrl, // specific cancel URL or fallback to success URL
                        description = description
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            _logger.LogInformation("Creating PayMongo Checkout Session: {Payload}", json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiUrl}/checkout_sessions", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayMongo error: {Response}", responseBody);
                return new PaymentLinkResult { Success = false, Message = responseBody };
            }

            var result = JsonSerializer.Deserialize<PayMongoLinkResponse>(responseBody, JsonOptions);

            return new PaymentLinkResult
            {
                Success = true,
                CheckoutUrl = result?.Data?.Attributes?.CheckoutUrl ?? "",
                LinkId = result?.Data?.Id ?? "",
                CheckoutId = result?.Data?.Id ?? "",
                Amount = amount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session");
            return new PaymentLinkResult { Success = false, Message = ex.Message };
        }
    }

    /// <summary>
    /// Registers or updates a webhook with PayMongo
    /// </summary>
    public async Task<WebhookResult> RegisterOrUpdateWebhookAsync(string webhookUrl)
    {
        try
        {
            _logger.LogInformation("Registering webhook: {Url}", webhookUrl);

            // First, list existing webhooks
            var listResponse = await _httpClient.GetAsync($"{_apiUrl}/webhooks");
            var listBody = await listResponse.Content.ReadAsStringAsync();
            var webhookId = "";

            if (listResponse.IsSuccessStatusCode)
            {
                var existingWebhooks = JsonSerializer.Deserialize<PayMongoWebhookListResponse>(listBody, JsonOptions);

                // Check if we already have a webhook for this URL pattern
                var existing = existingWebhooks?.Data?.FirstOrDefault(w => 
                    w.Attributes?.Url == webhookUrl || w.Attributes?.Url?.Contains("api/payments/webhook") == true);

                if (existing != null)
                {
                    webhookId = existing.Id;
                    _logger.LogInformation("Found existing webhook: {Id}", webhookId);

                    // If it exists, we try to ENABLE it with the new events
                    // This acts as an "update"
                    var payload = new
                    {
                        data = new
                        {
                            attributes = new
                            {
                                events = new[] { "link.payment.paid", "payment.paid", "payment.failed", "checkout_session.payment.paid" }
                            }
                        }
                    };

                    var json = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    // Try to enable/update
                    var enableResponse = await _httpClient.PostAsync($"{_apiUrl}/webhooks/{webhookId}/enable", content);
                    var enableBody = await enableResponse.Content.ReadAsStringAsync();

                    if (enableResponse.IsSuccessStatusCode)
                    {
                         var result = JsonSerializer.Deserialize<PayMongoWebhookResponse>(enableBody, JsonOptions);
                         _webhookSecretKey = result?.Data?.Attributes?.SecretKey;
                         return new WebhookResult { Success = true, WebhookId = webhookId, SecretKey = _webhookSecretKey ?? "" };
                    }
                    
                    _logger.LogWarning("Failed to enable/update existing webhook, will try to disable and recreate. Error: {Error}", enableBody);
                    
                    // If enable failed, try to disable then recreate (last resort)
                    await _httpClient.PostAsync($"{_apiUrl}/webhooks/{webhookId}/disable", null);
                }
            }

            // Create new webhook (if not found or disabled)
            var createPayload = new
            {
                data = new
                {
                    attributes = new
                    {
                        url = webhookUrl,
                        events = new[] { "link.payment.paid", "payment.paid", "payment.failed", "checkout_session.payment.paid" }
                    }
                }
            };

            var createJson = JsonSerializer.Serialize(createPayload);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiUrl}/webhooks", createContent);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Gracefully handle "resource_exists"
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && responseBody.Contains("resource_exists"))
                {
                    _logger.LogWarning("Webhook already exists (PayMongo returned resource_exists). Returning success.");
                    return new WebhookResult { Success = true, Message = "Webhook already exists" };
                }

                _logger.LogError("Failed to register webhook: {Response}", responseBody);
                return new WebhookResult { Success = false, Message = responseBody };
            }

            var createResult = JsonSerializer.Deserialize<PayMongoWebhookResponse>(responseBody, JsonOptions);
            _webhookSecretKey = createResult?.Data?.Attributes?.SecretKey;

            _logger.LogInformation("Webhook registered successfully: {Id}", createResult?.Data?.Id);

            return new WebhookResult
            {
                Success = true,
                WebhookId = createResult?.Data?.Id ?? "",
                SecretKey = _webhookSecretKey ?? ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering webhook");
            return new WebhookResult { Success = false, Message = ex.Message };
        }
    }

    /// <summary>
    /// Verifies webhook signature from PayMongo
    /// </summary>
    public Task<bool> VerifyWebhookSignature(string payload, string signature)
    {
        // PayMongo signature format: t=timestamp,li=live_signature,te=test_signature
        if (string.IsNullOrEmpty(_webhookSecretKey))
        {
            _logger.LogWarning("Webhook secret key not set, skipping verification");
            return Task.FromResult(true); // Skip verification in dev
        }

        try
        {
            var parts = signature.Split(',');
            var timestamp = parts.FirstOrDefault(p => p.StartsWith("t="))?.Substring(2);
            var testSig = parts.FirstOrDefault(p => p.StartsWith("te="))?.Substring(3);

            if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(testSig))
                return Task.FromResult(false);

            var signedPayload = $"{timestamp}.{payload}";
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
            var computed = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            return Task.FromResult(computed == testSig);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private static JsonSerializerOptions JsonOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
}

// Result DTOs
public class PaymentLinkResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string LinkId { get; set; } = "";
    public string CheckoutId { get; set; } = "";
    public string CheckoutUrl { get; set; } = "";
    public decimal Amount { get; set; }
}

public class WebhookResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string WebhookId { get; set; } = "";
    public string SecretKey { get; set; } = "";
}

// PayMongo API Response Models
public class PayMongoLinkResponse
{
    [JsonPropertyName("data")]
    public PayMongoLinkData? Data { get; set; }
}

public class PayMongoLinkData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("attributes")]
    public PayMongoLinkAttributes? Attributes { get; set; }
}

public class PayMongoLinkAttributes
{
    [JsonPropertyName("checkout_url")]
    public string CheckoutUrl { get; set; } = "";

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("reference_number")]
    public string ReferenceNumber { get; set; } = "";

    [JsonPropertyName("payments")]
    public List<PayMongoPaymentData>? Payments { get; set; }
}

public class PayMongoPaymentData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("attributes")]
    public PayMongoPaymentAttributes? Attributes { get; set; }
}

public class PayMongoPaymentAttributes
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("amount")]
    public long Amount { get; set; }
}

public class PayMongoWebhookListResponse
{
    [JsonPropertyName("data")]
    public List<PayMongoWebhookData>? Data { get; set; }
}

public class PayMongoWebhookResponse
{
    [JsonPropertyName("data")]
    public PayMongoWebhookData? Data { get; set; }
}

public class PayMongoWebhookData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("attributes")]
    public PayMongoWebhookAttributes? Attributes { get; set; }
}

public class PayMongoWebhookAttributes
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("secret_key")]
    public string SecretKey { get; set; } = "";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("events")]
    public List<string>? Events { get; set; }
}
