using Google.Apis.Gmail.v1;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nito.AsyncEx;

namespace InternetSeparationAdapter
{
  internal class Program
  {
    private static readonly string[] Scopes =
    {
      GmailService.Scope.GmailReadonly,
      GmailService.Scope.GmailModify
    };

    private const string ApplicationName = "Internet Separation Adapter";

    public static int Main(string[] args)
    {
      if (args.Length < 1)
      {
        Console.Write("Usage: InternetSeparationAdapter.exe path/to/config,json");
        return 1;
      }

      var configPath = args[0];
      string json;
      using (var configStream = GetConfigStream(configPath))
      {
        using (var reader = new StreamReader(configStream))
        {
          json = reader.ReadToEnd();
        }
      }

      var config = InitializeConfig(json);
      return config == null ? 1 : AsyncContext.Run(() => AsyncMain(config));
    }

    private static Stream GetConfigStream(string configPath)
    {
      return configPath == "-" ? Console.OpenStandardInput() :
        new FileStream(configPath, FileMode.Open, FileAccess.Read);
    }

    private static async Task<int> AsyncMain(Config config)
    {
      var exitToken = new CancellationTokenSource();
      Console.CancelKeyPress += (sender, cancelArgs) => { exitToken.Cancel(); };

      var service = new Gmail(
        config.GoogleCredentials.Secrets,
        config.StoredGoogleCredentialsPath,
        Scopes,
        ApplicationName, exitToken.Token);

      var bot = new Broadcaster(
        config.TelegramApiToken,
        config.TelegramChatGroupIds,
        cancellationToken: exitToken.Token
      );

      var periodicTimespan = TimeSpan.FromSeconds(config.PollInterval);

      while (!exitToken.IsCancellationRequested)
      {
        try
        {
          Console.WriteLine($"[{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}] Polling");
          await FetchUnread(service, bot, config.Label, exitToken.Token, config.MinimumImageSize)
            .ConfigureAwait(false);
          await Task.Delay(periodicTimespan, exitToken.Token).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
        }
      }

      return 0;
    }

    private static async Task FetchUnread(Gmail service, Broadcaster bot,
      string label, CancellationToken cancellationToken, int minimumImageSize)
    {
      var messages = await service.GetUnreadMessage(label).ConfigureAwait(false);

      var rawMessages = service.FetchMessages(messages, UsersResource.MessagesResource.GetRequest.FormatEnum.Raw);
      foreach (var rawLazy in rawMessages)
      {
        var raw = await rawLazy.ConfigureAwait(false);
        if (cancellationToken.IsCancellationRequested) return;
        try
        {
          Console.WriteLine($"ID: {raw.Id}");
          var message = raw.ToMimeMessage();
          Console.WriteLine(message.TextBody);
          await Task.WhenAll(bot.SendMailToTelegram(message)).ConfigureAwait(false);

          var images = message.BodyParts.InlineParts().ImageParts()
            .Where(image => image.ContentDisposition?.Size > minimumImageSize);
          foreach (var imagePart in images)
          {
            Console.WriteLine($"Content ID: {imagePart.ContentId} Content Location: {imagePart.ContentLocation}");
            using (var imageStream = imagePart.ContentObject.Open())
            {
              await Task.WhenAll(bot.SendPhotoToTelegram(imageStream, imagePart.FileName,
                $"{imagePart.ContentId}\n{imagePart.FileName}"));
            }
          }

          var attachments = message.BodyParts.AttachmentParts();
          var imageAttachments = attachments.ImageParts();
          foreach (var imageAttachment in imageAttachments)
          {
            Console.WriteLine(
              $"Content ID: {imageAttachment.ContentId} Content Location: {imageAttachment.ContentLocation}"
            );
            using (var imageStream = imageAttachment.ContentObject.Open())
            {
              await Task.WhenAll(bot.SendPhotoToTelegram(imageStream, imageAttachment.FileName,
                $"{imageAttachment.ContentId}\n{imageAttachment.FileName}"));
            }
          }

          var fileAttachments = attachments.Except(imageAttachments);
          foreach (var fileAttachment in fileAttachments)
          {
            Console.WriteLine(
              $"Content ID: {fileAttachment.ContentId} Content Location: {fileAttachment.ContentLocation}"
            );
            using (var fileStream = fileAttachment.ContentObject.Open())
            {
              await Task.WhenAll(bot.SendFileToTelegram(fileStream, fileAttachment.FileName,
                $"{fileAttachment.ContentId}\n{fileAttachment.FileName}"));
            }
          }

        }
        catch (FormatException e)
        {
          Console.WriteLine(e);
        }

      }
      await Task.WhenAll(service.MarkRead(messages)).ConfigureAwait(false);
    }

    private static Config InitializeConfig(string json)
    {
      return JsonConvert.DeserializeObject<Config>(json);
    }
  }
}
