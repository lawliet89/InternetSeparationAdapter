using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InternetSeparationAdapter
{
  class Program
  {
    // If modifying these scopes, delete your previously saved credentials
    // at ~/.credentials/gmail-dotnet-quickstart.json
    static string[] Scopes = {GmailService.Scope.GmailReadonly};
    static string ApplicationName = "Gmail API .NET Quickstart";

    static void Main(string[] args)
    {
      UserCredential credential;

      using (var stream =
        new FileStream("client_id.json", FileMode.Open, FileAccess.Read))
      {
        string credPath = System.Environment.GetFolderPath(
          System.Environment.SpecialFolder.Personal);
        credPath = Path.Combine(credPath, ".credentials/gmail-dotnet-quickstart.json");

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

      // Define parameters of request.
      UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

      // List labels.
      var labels = request.Execute().Labels;
      Console.WriteLine("Labels:");
      if (labels != null && labels.Count > 0)
      {
        foreach (var labelItem in labels)
        {
          Console.WriteLine("{0}", labelItem.Name);
        }
      }
      else
      {
        Console.WriteLine("No labels found.");
      }

      // Get unread Inbox Messages
      var inboxUnreadRequest = service.Users.Messages.List("me");
      inboxUnreadRequest.LabelIds = "INBOX";
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
    }
  }
}