using System.Net.Http;
using System.Net;

namespace NotificationService.Helpers;

public class SemaphoreSMSService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public SemaphoreSMSService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> SendSMSAsync(string number, string message)
    {
        if (number.StartsWith("09"))
        number = "63" + number.Substring(1);
        
        var url = "https://api.semaphore.co/api/v4/messages";
        var API_KEY = _config["Semaphore:API_KEY"];
        
        if (string.IsNullOrWhiteSpace(API_KEY))
            throw new InvalidOperationException("Semaphore API key is not configured.");

        var values = new Dictionary<string, string>
        {
            { "apikey", API_KEY },
            { "number", number },
            { "message", message },
            // { "sendername", "SEMAPHORE" },
        };

        var content = new FormUrlEncodedContent(values);

        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"SMS API returned error: {response.StatusCode} - {responseBody}");
        }
            
        Console.WriteLine("Semaphore Response:" + responseBody);
        
        return responseBody;
    }
}