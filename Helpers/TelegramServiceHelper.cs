using Humanizer;
using Telegram.Bot;


namespace NotificationService.Helpers;

public class TelegramServiceHelper
{
    private readonly TelegramBotClient _bot;

    public TelegramServiceHelper(string token)
    {
        _bot = new TelegramBotClient(token);
    }

    public async Task SendMessageAsync(long chatId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));

        try
        {
            await _bot.SendMessage(
                chatId: chatId,
                text: message
            );
        }
        catch (Telegram.Bot.Exceptions.ApiRequestException ex)
        {
            throw new InvalidOperationException(
                $"Telegram API error: {ex.ErrorCode} - {ex.Message}. Make sure the bot is added to the chat (ID: {chatId}) and has permission to send messages.",
                ex
            );
        }
    }
}

