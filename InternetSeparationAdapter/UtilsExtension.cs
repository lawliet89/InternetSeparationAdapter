using System;
using System.Text.RegularExpressions;

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
          throw new InvalidOperationException("This code should never be executed");
      }
      return Convert.FromBase64String(s); // Standard base64 decoder
    }

    public static string StripSuccessiveNewLines(this string input)
    {
      if (string.IsNullOrEmpty(input)) return input;
      const string pattern = @"(\r\n|\n|\n\r){2,}";
      var regex = new Regex(pattern);
      return regex.Replace(input, "\n\n");
    }
  }
}
