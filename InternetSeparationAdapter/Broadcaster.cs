using System.IO;
using Telegram.Bot;

namespace InternetSeparationAdapter
{
  public class Broadcaster
  {
    private readonly TelegramBotClient _bot;
    private readonly long _telegramChatId; // TODO: move this to a config file

    public Broadcaster(long chatId)
    {
      string apiToken;
      using (var reader = new StreamReader("telegram_api_token.txt"))
      {
        apiToken = reader.ReadLine() ?? "";
      }

      _bot = new TelegramBotClient(apiToken);
      _telegramChatId = chatId;
    }

    public void SendToTelegram(string message)
    {
      _bot.SendTextMessageAsync(_telegramChatId, message).Wait();
    }
  }
}