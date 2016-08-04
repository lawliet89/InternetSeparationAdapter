using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
  }
}