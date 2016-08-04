using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace InternetSeparationAdapter
{
  public class Gmail
  {
    private const string UnreadLabel = "UNREAD";

    private static ModifyMessageRequest MarkUnreadRequest =
      new ModifyMessageRequest {RemoveLabelIds = new[] {UnreadLabel}};

    private readonly CancellationToken _cancellationToken;
    private readonly GmailService _service;
    private readonly IConfigurableHttpClientInitializer _credentials;

    // TODO: Don't block on getting credentials -- make it an async method call for the user
    public Gmail(string secretFile, string credentialsPath, IEnumerable<string> scopes, string applicationName,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      _credentials = GetCredentials(secretFile, scopes, credentialsPath, cancellationToken).Result;
      if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException();
      _service = GetGmailService(_credentials, applicationName);
    }

    private static Task<UserCredential> GetCredentials(string secretsFile,
      IEnumerable<string> scopes,
      string credentialsPath = null,
      CancellationToken cancellation = default(CancellationToken))
    {
      using (var stream = new FileStream(secretsFile, FileMode.Open, FileAccess.Read))
      {
        var credPath = credentialsPath ?? Program.Arguments.DefaultCredentialsPath;
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

    private UsersResource.MessagesResource.ListRequest MakeUnreadMessageListRequest(string label)
    {
      var request = _service.Users.Messages.List("me");
      request.LabelIds = new[] {label, UnreadLabel};
      return request;
    }

    public async Task<IList<Google.Apis.Gmail.v1.Data.Message>> GetUnreadMessage(string label)
    {
      var request = MakeUnreadMessageListRequest(label);
      var execution = await request.ExecuteAsync(_cancellationToken);
      return _cancellationToken.IsCancellationRequested ? null : execution.Messages;
    }

    private UsersResource.MessagesResource.GetRequest MakeGetMessageRequest(string id)
    {
      return _service.Users.Messages.Get("me", id);
    }

    public IEnumerable<Task<Message>> FetchMessages(IEnumerable<Google.Apis.Gmail.v1.Data.Message> messages)
    {
      return messages.Select(async messageMeta =>
      {
        var messageRequest = MakeGetMessageRequest(messageMeta.Id);
        var message = await messageRequest.ExecuteAsync(_cancellationToken);
        return _cancellationToken.IsCancellationRequested ? null : new Message(message);
      });
    }

    public IEnumerable<Task<Google.Apis.Gmail.v1.Data.Message>> MarkRead(IEnumerable<Google.Apis.Gmail.v1.Data.Message> messages)
    {
      return messages.Select(message =>
      {
        var request = _service.Users.Messages.Modify(MarkUnreadRequest, "me", message.Id);
        return request.ExecuteAsync(_cancellationToken);
      });
    }
  }
}
