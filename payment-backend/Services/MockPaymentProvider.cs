namespace PaymentService2.Services;

/// <summary>
/// Mock payment provider for testing without real PayMongo API calls
/// </summary>
public class MockPaymentProvider : IPaymentProvider
{
    private readonly ILogger<MockPaymentProvider> _logger;
    private readonly Dictionary<string, PaymentLink> _mockLinks = new();
    private readonly Dictionary<string, PaymentIntent> _mockIntents = new();

    public string ProviderName => "Mock";

    public MockPaymentProvider(ILogger<MockPaymentProvider> logger)
    {
        _logger = logger;
    }

    public Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(decimal amount, string description, string referenceId, string? redirectUrl = null)
    {
        var mockLink = new PaymentLink
        {
            Id = $"mock_link_{Guid.NewGuid():N}"[..16],
            // Use relative URL so it works on any frontend port
            Url = $"/mock-checkout/{referenceId}",
            Amount = amount,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        _mockLinks[mockLink.Id] = mockLink;

        // Also create a mock payment intent for this
        var mockIntent = new PaymentIntent
        {
            Id = $"mock_pi_{Guid.NewGuid():N}"[..16],
            Status = "awaiting_payment_method",
            Amount = amount
        };
        _mockIntents[referenceId] = mockIntent;

        _logger.LogInformation($"[MOCK] Created payment link: {mockLink.Id}");
        _logger.LogInformation($"[MOCK] Amount: ₱{amount:N2}");
        _logger.LogInformation($"[MOCK] Description: {description}");
        _logger.LogInformation($"[MOCK] Checkout URL: {mockLink.Url}");

        return Task.FromResult(new CreatePaymentLinkResponse
        {
            Success = true,
            Data = mockLink,
            CheckoutId = mockLink.Id
        });
    }

    public Task<PaymentIntentResponse> GetPaymentIntentAsync(string paymentIntentId)
    {
        _logger.LogInformation($"[MOCK] Getting payment intent: {paymentIntentId}");

        // Return a mock successful payment intent
        return Task.FromResult(new PaymentIntentResponse
        {
            Success = true,
            Data = new PaymentIntent
            {
                Id = paymentIntentId,
                Status = "succeeded",
                Amount = 0
            }
        });
    }
}
