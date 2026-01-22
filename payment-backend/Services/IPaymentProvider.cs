using System.Text.Json.Serialization;

namespace PaymentService2.Services;

/// <summary>
/// Interface for payment providers (PayMongo, Mock, etc.)
/// Allows switching between real and mock payment processing
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Creates a payment link for the specified amount
    /// </summary>
    Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(decimal amount, string description, string referenceId, string? redirectUrl = null);
    
    /// <summary>
    /// Gets the status of a payment intent
    /// </summary>
    Task<PaymentIntentResponse> GetPaymentIntentAsync(string paymentIntentId);
    
    /// <summary>
    /// Name of the payment provider (for logging)
    /// </summary>
    string ProviderName { get; }
}

// Response Models
public class CreatePaymentLinkResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public PaymentLink? Data { get; set; }

    [JsonPropertyName("checkout_id")]
    public string? CheckoutId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class PaymentLink
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PaymentIntentResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public PaymentIntent? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class PaymentIntent
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
