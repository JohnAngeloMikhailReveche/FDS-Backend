using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text;

namespace NotificationService.Integration.Email
{
    public class GmailService
    {
        private static string[] Scopes = { Google.Apis.Gmail.v1.GmailService.Scope.GmailSend };
        private static string ApplicationName = "NotificationService";

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            UserCredential credential;

            // This assumes credentials.json is in the project root at runtime
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }

            var service = new Google.Apis.Gmail.v1.GmailService(new BaseClientService.Initializer()
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

        private string CreateEmail(string to, string subject, string bodyPlainText)
        {
            return $"To: {to}\r\n" +
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