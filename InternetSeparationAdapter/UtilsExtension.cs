using System;
using System.IO;
using MimeKit;

namespace InternetSeparationAdapter
{
  public static class UtilsExtension
  {
    // http://stackoverflow.com/a/33113820
    public static string Base64UrlEncode(this byte[] arg)
    {
      var s = Convert.ToBase64String(arg); // Regular base64 encoder
      s = s.Split('=')[0]; // Remove any trailing '='s
      s = s.Replace('+', '-'); // 62nd char of encoding
      s = s.Replace('/', '_'); // 63rd char of encoding
      return s;
    }

    public static byte[] Base64UrlDecode(this string arg)
    {
      var s = arg;
      s = s.Replace('-', '+'); // 62nd char of encoding
      s = s.Replace('_', '/'); // 63rd char of encoding
      switch (s.Length % 4) // Pad with trailing '='s
      {
        case 0:
          break; // No pad chars in this case
        case 2:
          s += "==";
          break; // Two pad chars
        case 3:
          s += "=";
          break; // One pad char
        default:
          throw new FormatException("Illegal base64url string!");
      }
      return Convert.FromBase64String(s); // Standard base64 decoder
    }

    public static MimeMessage ToMimeMessage(this Google.Apis.Gmail.v1.Data.Message message)
    {
      if (message.Raw == null)
        throw new ArgumentException("Gmail message is missing raw body. You need to fetch it with raw format");
      var raw = message.Raw.Base64UrlDecode();
      MimeMessage result;
      using (var stream = new MemoryStream(raw))
      {
        result = MimeMessage.Load(stream);
      }
      return result;
    }
  }
}
