using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InternetSeparationAdapter
{
  public class Broadcaster
  {
    private readonly TelegramBotClient _telegramBot;
    private readonly IList<long> _telegramGroupIds;
    private readonly CancellationToken _cancellationToken;

    public Broadcaster(string apiToken, IList<long> groupIds,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      _telegramBot = new TelegramBotClient(apiToken);
      _telegramGroupIds = groupIds;
      _cancellationToken = cancellationToken;
    }

    public IEnumerable<Task<Message>> SendToTelegram(string message)
    {
      return _telegramGroupIds.Select(id => _telegramBot.SendTextMessageAsync(id, message,
        cancellationToken: _cancellationToken));
    }

    public IEnumerable<Task<Message>> SendPhotoToTelegram(Stream image, string name, string caption = null)
    {
      var file = new FileToSend(name, image);
      return _telegramGroupIds.Select(id => _telegramBot.SendPhotoAsync(id, file, caption: caption ?? name,
        cancellationToken: _cancellationToken));
    }

    public static string FormatMessage(MimeMessage message)
    {
      var from = string.Join("; ", message.From.Select(sender => sender.ToString()));
      return $"{from}\n{message.Subject}\n{message.TextBody}";
    }
  }
}
