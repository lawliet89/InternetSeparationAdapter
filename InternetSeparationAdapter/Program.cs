using Google.Apis.Gmail.v1;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
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
      var arguments = Parser.Default.ParseArguments<Arguments>(args)
        .MapResult(parsed => parsed,
          errors =>
          {
            foreach (var error in errors)
            {
              Console.WriteLine(error);
            }
            return null;
          }
        );

      return arguments == null ? 1 : AsyncContext.Run(() => AsyncMain(arguments));
    }

    private static async Task<int> AsyncMain(Arguments arguments)
    {
      var exitToken = new CancellationTokenSource();
      Console.CancelKeyPress += (sender, cancelArgs) => { exitToken.Cancel(); };

      var service = new Gmail(arguments.SecretsFile, arguments.CredentialsPath, Scopes,
        ApplicationName, exitToken.Token);
      var bot = new Broadcaster(arguments.TelegramApiPath, arguments.TelegramChatGroupPath);

      var periodicTimespan = TimeSpan.FromSeconds(arguments.PollInterval);

      while (!exitToken.IsCancellationRequested)
      {
        try
        {
          Console.WriteLine($"[{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}] Polling");
          await FetchUnread(service, bot, arguments.Label, exitToken.Token);
          await Task.Delay(periodicTimespan, exitToken.Token).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
        }
      }

      return 0;
    }

    private static async Task FetchUnread(Gmail service, Broadcaster bot,
      string label, CancellationToken cancellationToken)
    {
      var messages = await service.GetUnreadMessage(label).ConfigureAwait(false);

      var messageBodies = service.FetchMessages(messages);
      foreach (var messageLazy in messageBodies)
      {
        var message = await messageLazy.ConfigureAwait(false);
        if (cancellationToken.IsCancellationRequested) return;
        Console.WriteLine($"ID: {message.Id}");
        Console.WriteLine($"From: {message.From}");
        Console.WriteLine($"Subject: {message.Subject}");
        try
        {
          var body = message.Body;
          if (body.HasValue)
          {
            Console.WriteLine($"Body: {body.Value.Value}");
          }
          await bot.SendToTelegram(message.FormattedMessage);
        }
        catch (NotImplementedException)
        {
          Console.WriteLine("Body: mutlipart/related");
        }
      }
      await Task.WhenAll(service.MarkRead(messages)).ConfigureAwait(false);
    }

    public class Arguments
    {
      [Option('s', "secret", Required = true, HelpText = "Path to the Gmail API Secrets JSON file")]
      public string SecretsFile { get; set; }

      [Option('t', "telegram-api-path", Required = true, HelpText = "Path to the Telegram API token file")]
      public string TelegramApiPath { get; set; }

      [Option('g', "telegram-chat-group-path", Required = true, HelpText = "Path to the Telegram chat group file")]
      public string TelegramChatGroupPath { get; set; }

      [Option('c', "credentials-path", HelpText = "Path to directory to store OAuth Credentials")]
      public string CredentialsPath { get; set; }

      [Option('l', "label", HelpText = "The default label to look for emails. Defaults to INBOX",
         Default = DefaultLabel)]
      public string Label { get; set; }

      [Option('i', "interval", HelpText = "The interval to poll Gmail", Default = 30)]
      public int PollInterval { get; set; }

      public static string DefaultCredentialsPath
      {
        get
        {
          var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
          return Path.Combine(credPath, ".credentials/internet-separation-adapter.json");
        }
      }

      public const string DefaultLabel = "INBOX";
    }
  }
}
