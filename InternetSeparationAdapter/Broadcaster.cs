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
      var fullMessage = $"{from}\n{message.Subject}\n{message.TextBody.StripSuccessiveNewLines()}";
      return SplitMessage(fullMessage);
    }

    public static IEnumerable<string> SplitMessage(string fullMessage, int maxLength = 4000, double threshold = 0.1)
    {
      var delimiters = new[]
      {
        new[] { "\n\n", "\n" },
        new[] { ". ", ";" },
        new[] { " " }
      };

      var startIndex = 0;
      var fullLength = fullMessage.Length;
      var thresholdLength = Convert.ToInt32(maxLength * (1 - threshold));

      while (startIndex < fullLength)
      {
        var endIndex = Math.Min(startIndex + maxLength, fullLength); // non-inclusive end index
        if (endIndex != fullLength)
        {
          foreach (var delimiterSet in delimiters)
          {
            var foundIndex = FindEndIndex(fullMessage, delimiterSet, startIndex, endIndex, thresholdLength);
            if (foundIndex == -1) continue;
            endIndex = foundIndex;
            break;
          }
        }
        yield return fullMessage.Substring(startIndex, endIndex - startIndex).Trim();
        startIndex = endIndex;
      }
    }

    private static int FindEndIndex(string fullMessage, IList<string> delimiterSet, int startIndex,
      int endIndex, int thresholdLength)
    {
      if (delimiterSet.Count == 0)
        throw new ArgumentException("A delimiter set cannot be empty", nameof(delimiterSet));

      var delimterLength = delimiterSet.Select(delim => delim.Length).Max();
      var substring = DelimiterTestSubstring(fullMessage, endIndex, delimterLength);
      while (!delimiterSet.Select(delim => substring.Contains(delim)).Any(contains => contains))
      {
        if (endIndex - startIndex < thresholdLength) return -1;
        endIndex -= delimterLength;
        substring = DelimiterTestSubstring(fullMessage, endIndex, delimterLength);
      }
      return endIndex;
    }

    private static string DelimiterTestSubstring(string str, int index, int length)
    {
      return str.Substring(index - length, length * 2);
    }
  }
}
