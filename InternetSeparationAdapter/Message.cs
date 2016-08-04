using System;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Google.Apis.Gmail.v1.Data;

namespace InternetSeparationAdapter
{
  public class Message
  {
    public struct BodyWithType
    {
      public string Value;
      public ContentType ContentType;
    }

    private Google.Apis.Gmail.v1.Data.Message RawMessage;

    public Message(Google.Apis.Gmail.v1.Data.Message rawMessage)
    {
      RawMessage = rawMessage;
    }

    public MessagePart Payload => RawMessage.Payload;

    public string Subject
    {
      get { return Payload.Headers.SingleOrDefault(header => header.Name == "Subject")?.Value; }
    }

    public BodyWithType? Body
    {
      get
      {
        if (MultiPartAlternative)
        {
          var part = PlainBodyPart ?? HtmlBodyPart;
          return part == null ? (BodyWithType?) null : DecodePart(part);
        }
        if (MutliPartRelated)
        {
          throw new NotImplementedException();
        }
        return DecodePart(Payload);
      }
    }

    public string From
    {
      get { return Payload.Headers.SingleOrDefault(header => header.Name == "From")?.Value; }
    }

    public string To
    {
      get { return Payload.Headers.SingleOrDefault(header => header.Name == "To")?.Value; }
    }

    public bool MultiPartAlternative => Payload.MimeType == "multipart/alternative";

    public bool MutliPartRelated => Payload.MimeType == "multipart/related";


    public MessagePart PlainBodyPart
    {
      get { return Payload.Parts.SingleOrDefault(part => part.MimeType == "text/plain"); }
    }

    public MessagePart HtmlBodyPart
    {
      get { return Payload.Parts.SingleOrDefault(part => part.MimeType == "text/html"); }
    }

    private BodyWithType DecodePart(MessagePart part)
    {
      var contentType = new ContentType(part.Headers.SingleOrDefault(header => header.Name == "Content-Type")?.Value ??
                                        "text/plain; charset=UTF-8");
      var base64Body = part.Body.Data.Trim();
      var bytes = Base64UrlDecode(base64Body);
      return new BodyWithType
      {
        Value = Encoding.GetEncoding(contentType.CharSet).GetString(bytes),
        ContentType = contentType
      };
    }

    // http://stackoverflow.com/a/33113820
    static string Base64UrlEncode(byte[] arg)
    {
      var s = Convert.ToBase64String(arg); // Regular base64 encoder
      s = s.Split('=')[0]; // Remove any trailing '='s
      s = s.Replace('+', '-'); // 62nd char of encoding
      s = s.Replace('/', '_'); // 63rd char of encoding
      return s;
    }

    static byte[] Base64UrlDecode(string arg)
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
          throw new FormatException( "Illegal base64url string!");
      }
      return Convert.FromBase64String(s); // Standard base64 decoder
    }
  }
}
