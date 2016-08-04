using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Google.Apis.Http;
using Nito.AsyncEx;

namespace InternetSeparationAdapter
{
  internal class Program
  {
    private static readonly string[] Scopes = {GmailService.Scope.GmailReadonly};
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
      Console.CancelKeyPress += (sender, cancelArgs) =>
      {
        exitToken.Cancel();
      };

      var credential = await GetCredentials(arguments.SecretsFile, Scopes, arguments.CredentialsPath, exitToken.Token);
      var service = GetGmailService(credential, ApplicationName);

      if (exitToken.IsCancellationRequested) return 1;

      var messages = await GetUnreadMessage(service, arguments.Label, exitToken.Token);

      foreach (var messageLazy in messages)
      {
        var message = await messageLazy;
        if (exitToken.IsCancellationRequested) return 1;
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
        }
        catch (NotImplementedException)
        {
          Console.WriteLine("Body: mutlipart/related");
        }
      }

      return 0;
    }

    public class Arguments
    {
      [Option('s', "secret", Required = true, HelpText = "Path to the Gmail API Secrets JSON file")]
      public string SecretsFile { get; set; }

      [Option('c', "credentials-path", HelpText = "Path to directory to store OAuth Credentials")]
      public string CredentialsPath { get; set; }

      [Option('l', "label", HelpText = "The default label to look for emails. Defaults to INBOX",
         Default = DefaultLabel)]
      public string Label { get; set; }

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

    private static Task<UserCredential > GetCredentials(string secretsFile,
      IEnumerable<string> scopes,
      string credentialsPath = null,
      CancellationToken cancellation = default(CancellationToken))
    {
      using (var stream = new FileStream(secretsFile, FileMode.Open, FileAccess.Read))
      {
        var credPath = credentialsPath ?? Arguments.DefaultCredentialsPath;
        Console.WriteLine("Credential file will be saved to: " + credPath);
        return GoogleWebAuthorizationBroker.AuthorizeAsync(
          GoogleClientSecrets.Load(stream).Secrets,
          scopes,
          "user",
          cancellation,
          new FileDataStore(credPath, true));
      }
    }

    private static GmailService GetGmailService(IConfigurableHttpClientInitializer credentials, string applicationName)
    {
      return new GmailService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credentials,
        ApplicationName = applicationName,
      });
    }

    private static UsersResource.MessagesResource.ListRequest MakeUnreadMessageListRequest(GmailService service,
      string label)
    {
      var request = service.Users.Messages.List("me");
      request.LabelIds = label;
      request.Q = "is:unread";
      return request;
    }

    private static UsersResource.MessagesResource.GetRequest MakeGetMessageRequest(GmailService service, string id)
    {
      return service.Users.Messages.Get("me", id);
    }

    private static async Task<IEnumerable<Task<Message>>> GetUnreadMessage(GmailService service, string label,
      CancellationToken cancellation = default(CancellationToken))
    {
      var request = MakeUnreadMessageListRequest(service, label);
      var execution = await request.ExecuteAsync(cancellation);
      if (cancellation.IsCancellationRequested) return null;
      var messages = execution.Messages;

      return messages.Select(async messageMeta =>
      {
        var messageRequest = MakeGetMessageRequest(service, messageMeta.Id);
        var message = await messageRequest.ExecuteAsync(cancellation);
        return cancellation.IsCancellationRequested ? null : new Message(message);
      });
    }
  }
}
