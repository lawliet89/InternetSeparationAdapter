using System.Text;
using NUnit.Framework;

namespace InternetSeparationAdapter.Tests
{
  [TestFixture]
  public static class UtilsExtensionTest
  {
    [TestCase("212132321123123123213s", ExpectedResult = "MjEyMTMyMzIxMTIzMTIzMTIzMjEzcw")]
    [TestCase("abc123!?$*&()'-=@~???", ExpectedResult = "YWJjMTIzIT8kKiYoKSctPUB-Pz8_")]
    public static string Base64UrlIsEncodedProperly(string input)
    {
      return Encoding.UTF8.GetBytes(input).Base64UrlEncode();
    }

    [TestCase("MjEyMTMyMzIxMTIzMTIzMTIzMjEzcw", ExpectedResult = "212132321123123123213s")]
    [TestCase("YWJjMTIzIT8kKiYoKSctPUB-Pz8_", ExpectedResult = "abc123!?$*&()'-=@~???")]
    public static string Base64UrlIsDecodedProperly(string input)
    {
      return Encoding.UTF8.GetString(input.Base64UrlDecode());
    }

    [TestCase("a\nb\n", ExpectedResult = "a\nb\n")]
    [TestCase("a\n\n\n\nb\n\nc\nd", ExpectedResult = "a\n\nb\n\nc\nd")]
    [TestCase("a\r\n\n\r\n\nb\n\nc\nd", ExpectedResult = "a\n\nb\n\nc\nd")]
    public static string SuccessiveNewLinesAreStripped(string input)
    {
      return input.StripSuccessiveNewLines();
    }
  }
}
