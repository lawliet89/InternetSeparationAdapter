using NUnit.Framework;
using System.Collections.Generic;

namespace InternetSeparationAdapter.Tests
{
  [TestFixture]
  public static class BroadcasterTests
  {
    public static string TestInput =
      @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam iaculis gravida risus nec ornare. Praesent et auctor felis. Mauris eget imperdiet metus, sed tempor leo. Pellentesque cursus elit diam, sed tincidunt massa mattis vitae. Interdum et malesuada fames ac ante ipsum primis in faucibus. Phasellus blandit mauris ac ligula laoreet, eleifend luctus mauris dictum. Aliquam quis volutpat arcu, interdum rutrum neque. Nullam interdum sapien orci, eget varius erat ullamcorper sed. Duis lacinia sapien dolor, vel rhoncus nibh porta id. Aenean tristique accumsan dapibus. Pellentesque eget tortor volutpat, convallis sem quis, tincidunt eros. Pellentesque nisl dui, gravida quis lacus ut, mattis suscipit quam. Praesent tempor odio sit amet euismod facilisis. Nullam nec bibendum nunc. Mauris vel semper dui, in auctor nulla. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae;";

    [Test]
    public static void SplitMessageSplitsTextCorrectly()
    {
      var expected = new List<string>()
      {
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam iaculis gravida risus nec ornare. Praesent et auctor felis. Mauris eget imperdiet metus, sed tempor leo. Pellentesque cursus elit diam, sed",
        " tincidunt massa mattis vitae. Interdum et malesuada fames ac ante ipsum primis in faucibus. Phasellus blandit mauris ac ligula laoreet, eleifend luctus mauris dictum. Aliquam quis volutpat arcu, inte",
        "rdum rutrum neque. Nullam interdum sapien orci, eget varius erat ullamcorper sed. Duis lacinia sapien dolor, vel rhoncus nibh porta id. Aenean tristique accumsan dapibus. Pellentesque eget tortor volu",
        "tpat, convallis sem quis, tincidunt eros. Pellentesque nisl dui, gravida quis lacus ut, mattis suscipit quam. Praesent tempor odio sit amet euismod facilisis. Nullam nec bibendum nunc. Mauris vel semp",
        @"er dui, in auctor nulla. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae;"
      };
      var actual = Broadcaster.SplitMessage(TestInput, 200);
      Assert.That(actual, Is.EqualTo(expected));
    }
  }
}
