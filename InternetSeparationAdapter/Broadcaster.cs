using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;

namespace InternetSeparationAdapter
{
  public class Broadcaster
  {
    private readonly TelegramBotClient _telegramBot;
    private readonly long _telegramChatId;

    public Broadcaster(string apiTokenPath, string chatGroupIdPath)
    {
      string apiToken;
      using (var reader = new StreamReader(apiTokenPath))
      {
        apiToken = reader.ReadLine() ?? "";
      }

      string chatId;
      using (var reader = new StreamReader(chatGroupIdPath))
      {
        chatId = reader.ReadLine() ?? "";
      }

      _telegramBot = new TelegramBotClient(apiToken);
      _telegramChatId = Convert.ToInt64(chatId);
    }

    public async Task SendToTelegram(string message)
    {
      await _telegramBot.SendTextMessageAsync(_telegramChatId, message);
    }
  }
}