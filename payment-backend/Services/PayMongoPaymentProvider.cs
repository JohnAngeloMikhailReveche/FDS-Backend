using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentService2.Services;

/// <summary>
/// PayMongo payment provider - Real payment processing via PayMongo API
/// </summary>
public class PayMongoPaymentProvider : IPaymentProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _secretKey;
    private readonly string _apiUrl;
    private readonly ILogger<PayMongoPaymentProvider> _logger;

    public string ProviderName => "PayMongo";

    public PayMongoPaymentProvider(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<PayMongoPaymentProvider> logger)
    {
        // Check environment variable first, then fall back to appsettings
        _secretKey = Environment.GetEnvironmentVariable("PAYMONGO_SECRET_KEY") 
            ?? configuration["PayMongo:SecretKey"] 
            ?? throw new InvalidOperationException("PayMongo secret key not configured. Set PAYMONGO_SECRET_KEY env var or PayMongo:SecretKey in appsettings.");
        
        _apiUrl = Environment.GetEnvironmentVariable("PAYMONGO_BASE_URL") 
            ?? configuration["PayMongo:BaseUrl"] 
            ?? "https://api.paymongo.com/v1";
        
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        
        // Set up Basic Auth with secret key
        var authBytes = Encoding.ASCII.GetBytes($"{_secretKey}:");
        var authHeader = Convert.ToBase64String(authBytes);
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);
    }

    public async Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(decimal amount, string description, string referenceId, string? redirectUrl = null)
    {
        try
        {
            var amountInCentavos = (long)(amount * 100);
            
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
                                name = "Payment",
                                quantity = 1
                            }
                        },
                        payment_method_types = new[] { "gcash", "paymaya", "card", "grab_pay" },
                        reference_number = referenceId,
                        success_url = redirectUrl,
                        cancel_url = redirectUrl,
                        description = description
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            _logger.LogInformation($"Creating PayMongo Checkout Session: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiUrl}/checkout_sessions", content);
            
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"PayMongo error ({response.StatusCode}): {responseBody}");
                throw new InvalidOperationException($"Failed to create checkout session: {responseBody}");
            }

            var options = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            var result = JsonSerializer.Deserialize<PayMongoLinkResponse>(responseBody, options);

            _logger.LogInformation($"Successfully created PayMongo checkout session: {result?.Data?.Id}");

            return new CreatePaymentLinkResponse
            {
                Success = true,
                CheckoutId = result?.Data?.Id, // This is the Session ID
                Data = new PaymentLink
                {
                    Id = result?.Data?.Id ?? string.Empty,
                    Url = result?.Data?.Attributes?.CheckoutUrl ?? string.Empty,
                    Amount = amount,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating PayMongo checkout session: {ex.Message}");
            return new CreatePaymentLinkResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<PaymentIntentResponse> GetPaymentIntentAsync(string paymentIntentId)
    {
        // Re-implement if needed, but OrderService relies on checkout_sessions mainly.
        // For now, keep as is or simple wrapper.
        // NOTE: Checkout Sessions don't always expose simple Payment Intents immediately in the same way,
        // but we keep this method for interface compliance.
        // This method might need to be adjusted if we really use it for Order Verification via Intent ID.
        // However, VerifyOrder uses GetCheckoutSessionAsync from IPayMongoService, not this.
        return await Task.FromResult(new PaymentIntentResponse { Success = false, Message = "Not implemented for checkout sessions flow yet" });
    }
}

// PayMongo API Response Models - Shared with PayMongoService.cs
// Classes: PayMongoLinkResponse, PayMongoLinkData, PayMongoLinkAttributes, PayMongoPaymentIntentResponse, etc.
// are now defined in PayMongoService.cs only.
