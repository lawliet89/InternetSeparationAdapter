using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using CommandLine;

namespace InternetSeparationAdapter
{
  class Program
  {
    // If modifying these scopes, delete your previously saved credentials
    // at ~/.credentials/gmail-dotnet-quickstart.json
    static string[] Scopes = {GmailService.Scope.GmailReadonly};
    static string ApplicationName = "Gmail API .NET Quickstart";

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

      if (arguments == null) return 1;

      UserCredential credential;

      Console.Write(arguments.CredentialsPath);

      using (var stream = new FileStream(arguments.SecretsFile, FileMode.Open, FileAccess.Read))
      {
        var credPath = arguments.CredentialsPath ?? Arguments.DefaultCredentialsPath;

        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
          GoogleClientSecrets.Load(stream).Secrets,
          Scopes,
          "user",
          CancellationToken.None,
          new FileDataStore(credPath, true)).Result;
        Console.WriteLine("Credential file saved to: " + credPath);
      }

      // Create Gmail API service.
      var service = new GmailService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = ApplicationName,
      });

      // Get unread Inbox Messages
      var inboxUnreadRequest = service.Users.Messages.List("me");
      inboxUnreadRequest.LabelIds = arguments.Label;
      inboxUnreadRequest.Q = "is:unread";

      var messages = inboxUnreadRequest.Execute().Messages;
      Console.WriteLine($"{messages.Count} unread");

      foreach (var messageMeta in messages)
      {
        Console.WriteLine($"{messageMeta.Id}");
        var id = messageMeta.Id;
        var messageRequest = service.Users.Messages.Get("me", id);
        var message = new Message(messageRequest.Execute());
        Console.WriteLine($"From: {message.From}");
        Console.WriteLine($"Subject: {message.Subject}");
        var body = message.Body;
        if (body.HasValue)
        {
          Console.WriteLine($"Body: {body.Value.Value}");
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
  }
}