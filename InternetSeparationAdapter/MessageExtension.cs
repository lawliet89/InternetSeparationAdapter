using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MimeKit;

namespace InternetSeparationAdapter
{
  public static class MessageExtension
  {
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

    public static IEnumerable<MimeEntity> InlineParts(this IEnumerable<MimeEntity> parts)
    {
      return parts.Where(part => part.ContentDisposition?.Disposition == ContentDisposition.Inline);
    }

    public static IEnumerable<MimeEntity> ImageParts(this IEnumerable<MimeEntity> parts)
    {
      return parts.Where(part => part.ContentType.MediaType == "image");
    }
  }
}
