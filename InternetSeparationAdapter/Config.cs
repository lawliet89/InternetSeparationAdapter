using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;

namespace InternetSeparationAdapter
{
  public class Config
  {
    public string TelegramApiToken { get; set; }
    public IList<long> TelegramChatGroupIds { get; set; }
    public GoogleClientSecrets GoogleCredentials { get; set; }
    public string StoredGoogleCredentialsPath { get; set; }

    [DefaultValue("INBOX")]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string Label { get; set; }

    [DefaultValue(30)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public int PollInterval { get; set; }

    public static string DefaultCredentialsPath
    {
      get
      {
        var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        return Path.Combine(credPath, ".credentials/internet-separation-adapter.json");
      }
    }
  }
}