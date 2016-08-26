using System;
using System.Linq;
using NUnit.Framework;

namespace InternetSeparationAdapter.Tests
{
  [TestFixture]
  public static class BroadcasterTests
  {
    [Test]
    public static void SplitMessageSplitsTextCorrectly()
    {
      const int partLength = 2000;
      var fixture = Fixtures.MobyDick.Text;
      var actual = Broadcaster.SplitMessage(fixture, partLength).ToList();
      Assert.That(actual, Is.EqualTo(Fixtures.MobyDick.Parts));
      Assert.That(actual.Select(part => part.Length).All(length => length <= partLength));
    }
  }
}
