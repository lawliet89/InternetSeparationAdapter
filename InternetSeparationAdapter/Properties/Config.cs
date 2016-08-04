using System.ComponentModel;
using Newtonsoft.Json;

namespace InternetSeparationAdapter.Properties
{
  public class Config
  {
    [DefaultValue("telegram_api_token.txt")]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string TelegramApiPath { get; set; }

    [DefaultValue("telegram_group.txt")]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string TelegramChatGroupIdPath { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string SecretsPath { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string CredentialsPath { get; set; }

    [DefaultValue("INBOX")]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string Label { get; set; }
  }
}