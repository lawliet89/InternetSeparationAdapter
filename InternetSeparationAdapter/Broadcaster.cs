using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    private readonly int _maxParts;

    public Broadcaster(string apiToken, IList<long> groupIds, int maxParts = 5,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      _telegramBot = new TelegramBotClient(apiToken);
      _telegramGroupIds = groupIds;
      _cancellationToken = cancellationToken;
      _maxParts = maxParts;
    }

    public async Task<IEnumerable<Message[]>> SendMailToTelegram(MimeMessage message)
    {
      var messages = new List<Message[]>();
      using (var blocks = FormatMessage(message).GetEnumerator())
      {
        var counter = 0;

        while (counter < _maxParts && blocks.MoveNext())
        {
          var block = blocks.Current;
          messages.Add(await Task.WhenAll(SendToTelegram(block)).ConfigureAwait(false));
          await Task.Delay(TimeSpan.FromSeconds(2), _cancellationToken);
          ++counter;
        }

        if (blocks.MoveNext())
        {
          var bytes = Encoding.UTF8.GetBytes(message.TextBody);
          using (var file = new MemoryStream(bytes))
          {
            messages.Add(await Task.WhenAll(SendFileToTelegram(file, "message.txt",
              "Message too long. See attachment.")).ConfigureAwait(false));
          }
        }
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

    public IEnumerable<Task<Message>> SendFileToTelegram(Stream fileStream, string name, string caption = null)
    {
      var file = new FileToSend(name, fileStream);
      return _telegramGroupIds.Select(id => _telegramBot.SendDocumentAsync(id, file, caption: caption ?? name,
        cancellationToken: _cancellationToken));
    }

    public static IEnumerable<string> FormatMessage(MimeMessage message)
    {
      var from = string.Join("; ", message.From.Select(sender => sender.ToString()));
      var fullMessage = $"{from}\n{message.Subject}\n{message.TextBody}";
      return SplitMessage(fullMessage);
    }

    private static IEnumerable<string> SplitMessage(string fullMessage, int maxLength = 4000)
    {
      var index = 0;
      while (index < fullMessage.Length)
      {
        yield return fullMessage.Substring(index, Math.Min(maxLength, fullMessage.Length - index));
        index += maxLength;
      }
    }
  }
}
