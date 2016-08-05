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

    public async Task<IEnumerable<Message[]>> SendMailToTelegram(MimeMessage message)
    {
      var messages = new List<Message[]>();
      foreach (var block in FormatMessage(message))
      {
        messages.Add(await Task.WhenAll(SendToTelegram(block)).ConfigureAwait(false));
        await Task.Delay(TimeSpan.FromSeconds(2), _cancellationToken);
      }
      return messages;
    }

    public IEnumerable<Task<Message>> SendToTelegram(string message)
    {
      return _telegramGroupIds.Select(id =>
        _telegramBot.SendTextMessageAsync(
          id,
          message,
          disableWebPagePreview: true,
          cancellationToken: _cancellationToken)
      );
    }

    public IEnumerable<Task<Message>> SendPhotoToTelegram(Stream image, string name, string caption = null)
    {
      var file = new FileToSend(name, image);
      return _telegramGroupIds.Select(id => _telegramBot.SendPhotoAsync(id, file, caption: caption ?? name,
        cancellationToken: _cancellationToken));
    }

    public static List<string> FormatMessage(MimeMessage message)
    {
      var from = string.Join("; ", message.From.Select(sender => sender.ToString()));
      var fullMessage = $"{from}\n{message.Subject}\n{message.TextBody}";
      return SplitMessage(fullMessage);
    }

    private static List<string> SplitMessage(string fullMessage, int maxLength = 4000)
    {
      var output = new List<string>();
      var remainingString = fullMessage;

      while (remainingString.Length > 4000)
      {
        var element = remainingString.Substring(0, 4000);
        output.Add(element);
        remainingString = remainingString.Substring(4000);
      }
      output.Add(remainingString);
      return output;
    }
  }
}
