using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text;


// Made by Kyle Christian Casipit
namespace NotificationService.Helpers
{
    public class GmailEmailService
    {
        private readonly IConfiguration _config;
        private static string[] Scopes = 
        { 
            GmailService.Scope.GmailSend 
        };

        private static string ApplicationName = "NotificationService";

        public GmailEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Email address cannot be empty.");
            
            toEmail = toEmail.Trim();
            
            var atIndex = toEmail.IndexOf("@");
            if (atIndex <= 0 || atIndex == toEmail.Length - 1)
                throw new ArgumentException($"Invalid email address format: {toEmail}. Must be in format: username@domain.com");
            
            if (!toEmail.Contains("."))
                throw new ArgumentException($"Invalid email address: {toEmail}. Domain must contain a dot.");

            UserCredential credential;

            var clientId = _config["Google:client_id"];
            var clientSecret = _config["Google:client_secret"];

            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId!,
                    ClientSecret = clientSecret!
                },
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("token.json", true)
            );

            var service = new Google.Apis.Gmail.v1.GmailService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

            var message = new Message
            {
                Raw = Base64UrlEncode(CreateEmail(toEmail, subject, body))
            };

            await service.Users.Messages.Send(message, "me").ExecuteAsync();
        }

        private string CreateEmail(string user, string subject, string bodyPlainText)
        {
            return $"From: me\r\n" +
                   $"To: {user}\r\n" +
                   $"Subject: {subject}\r\n" +
                   "Content-Type: text/plain; charset=utf-8\r\n\r\n" +
                   $"{bodyPlainText}";
        }

        private string Base64UrlEncode(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }
    }
}