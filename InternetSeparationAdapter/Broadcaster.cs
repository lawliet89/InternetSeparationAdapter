using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;
using Telegram.Bot;

namespace InternetSeparationAdapter
{
  public class Broadcaster
  {
    private readonly TelegramBotClient _telegramBot;
    private readonly IList<long> _telegramGroupIds;

    public Broadcaster(string apiToken, IList<long> groupIds)
    {
      _telegramBot = new TelegramBotClient(apiToken);
      _telegramGroupIds = groupIds;
    }

    public IEnumerable<Task> SendToTelegram(string message)
    {
      return _telegramGroupIds.Select(id => _telegramBot.SendTextMessageAsync(id, message));
    }

    public static string FormatMessage(MimeMessage message)
    {
      var from = string.Join("; ", message.From.Select(sender => sender.ToString()));
      return $"{from}\n{message.Subject}\n{message.TextBody}";
    }
  }
}
