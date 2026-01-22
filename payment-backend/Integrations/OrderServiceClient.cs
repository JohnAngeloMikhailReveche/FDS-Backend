using System.Net.Http.Json;
using System.Text.Json;
using PaymentService2.Models;

namespace PaymentService.Integrations;

public interface IOrderServiceClient
{
    bool IsConfigured { get; }
    Task<ExternalOrderDto> GetOrderAsync(string orderId, CancellationToken ct = default);
    Task<int> GetPendingCountAsync(string userId, CancellationToken ct = default);
}

public class OrderServiceClient : IOrderServiceClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<OrderServiceClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OrderServiceClient(HttpClient http, IConfiguration config, ILogger<OrderServiceClient> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public bool IsConfigured
    {
        get
        {
            // Integration is explicitly opt-in.
            // Default behavior (until a real Order Service exists) is to keep OrderService:Enabled=false
            // and use DB-backed /api/orders as the mock order source.
            var enabledRaw = _config["OrderService:Enabled"];
            var enabled = bool.TryParse(enabledRaw, out var parsedEnabled) && parsedEnabled;
            var baseUrl = _config["OrderService:BaseUrl"];
            return enabled && !string.IsNullOrWhiteSpace(baseUrl);
        }
    }

    private Uri BuildUri(string template, Dictionary<string, string> tokens)
    {
        var baseUrl = _config["OrderService:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("OrderService is not configured. Set OrderService:BaseUrl in appsettings.");
        }

        var path = template;
        foreach (var kv in tokens)
        {
            path = path.Replace("{" + kv.Key + "}", Uri.EscapeDataString(kv.Value));
        }

        return new Uri(new Uri(baseUrl.TrimEnd('/')), path);
    }

    public async Task<ExternalOrderDto> GetOrderAsync(string orderId, CancellationToken ct = default)
    {
        var path = _config["OrderService:GetOrderPath"] ?? "/api/orders/{orderId}";
        var uri = BuildUri(path, new Dictionary<string, string> { { "orderId", orderId } });

        _logger.LogInformation("Fetching order {OrderId} from OrderService: {Uri}", orderId, uri);

        HttpResponseMessage res;
        try
        {
            res = await _http.GetAsync(uri, ct);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new InvalidOperationException($"OrderService request timed out contacting: {uri}", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to reach OrderService at: {uri}. {ex.Message}", ex);
        }

        using (res)
        {
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"OrderService returned {(int)res.StatusCode}: {body}");
            }

            var dto = await res.Content.ReadFromJsonAsync<ExternalOrderDto>(JsonOptions, ct);
            return dto ?? throw new InvalidOperationException("OrderService returned empty response");
        }
    }

    public async Task<int> GetPendingCountAsync(string userId, CancellationToken ct = default)
    {
        var path = _config["OrderService:ListOrdersByUserPath"] ?? "/api/orders?userId={userId}";
        var uri = BuildUri(path, new Dictionary<string, string> { { "userId", userId } });

        _logger.LogInformation("Fetching orders for user {UserId} from OrderService: {Uri}", userId, uri);

        HttpResponseMessage res;
        try
        {
            res = await _http.GetAsync(uri, ct);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new InvalidOperationException($"OrderService request timed out contacting: {uri}", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to reach OrderService at: {uri}. {ex.Message}", ex);
        }

        using (res)
        {
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"OrderService returned {(int)res.StatusCode}: {body}");
            }

            // Expected shape: either an array of orders, or { data: [..] }
            var raw = await res.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(raw);

        JsonElement ordersEl;
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            ordersEl = doc.RootElement;
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
        {
            ordersEl = dataEl;
        }
        else
        {
            throw new InvalidOperationException("OrderService list endpoint returned an unexpected shape.");
        }

        var pending = 0;
        foreach (var o in ordersEl.EnumerateArray())
        {
            if (o.ValueKind != JsonValueKind.Object) continue;
            if (!o.TryGetProperty("status", out var statusEl)) continue;
            var status = statusEl.GetString() ?? string.Empty;
            if (status.Equals("pending", StringComparison.OrdinalIgnoreCase)) pending++;
        }

            return pending;
        }
    }
}

public class ExternalOrderDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public List<ExternalOrderItemDto> Items { get; set; } = new();
}

public class ExternalOrderItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
