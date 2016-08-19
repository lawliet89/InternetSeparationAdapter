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
  }
}
