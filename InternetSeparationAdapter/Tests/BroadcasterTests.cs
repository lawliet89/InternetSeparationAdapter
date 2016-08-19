using NUnit.Framework;
using System.Collections.Generic;

namespace InternetSeparationAdapter.Tests
{
  [TestFixture]
  public static class BroadcasterTests
  {
    public static string TestInput =
      @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam iaculis gravida risus nec ornare. Praesent et auctor felis. Mauris eget imperdiet metus, sed tempor leo. Pellentesque cursus elit diam, sed tincidunt massa mattis vitae. Interdum et malesuada fames ac ante ipsum primis in faucibus. Phasellus blandit mauris ac ligula laoreet, eleifend luctus mauris dictum. Aliquam quis volutpat arcu, interdum rutrum neque. Nullam interdum sapien orci, eget varius erat ullamcorper sed. Duis lacinia sapien dolor, vel rhoncus nibh porta id. Aenean tristique accumsan dapibus. Pellentesque eget tortor volutpat, convallis sem quis, tincidunt eros. Pellentesque nisl dui, gravida quis lacus ut, mattis suscipit quam. Praesent tempor odio sit amet euismod facilisis. Nullam nec bibendum nunc. Mauris vel semper dui, in auctor nulla. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae;

Pellentesque at purus a magna dictum mattis. In hac habitasse platea dictumst. Mauris dictum libero scelerisque pharetra porta. Phasellus enim nibh, accumsan ut orci non, aliquam eleifend justo. Fusce laoreet at lorem in scelerisque. Vestibulum vestibulum cursus nibh sed placerat. Donec velit enim, condimentum sed libero id, auctor vestibulum ipsum. Nullam et placerat nibh.

Quisque et eleifend lectus. Quisque cursus ipsum ac orci auctor auctor. Vivamus a tortor vitae tellus porta scelerisque a et odio. Nulla pulvinar, massa vitae efficitur luctus, urna nunc aliquam orci, sit amet rutrum dui sapien a dolor. Cras eget elit nec libero sodales sodales quis eu ex. Mauris turpis ligula, dignissim sed orci vel, mattis venenatis eros. Integer augue odio, gravida et lectus id, commodo mollis quam. Ut congue pretium tempor. Maecenas in interdum erat, nec blandit urna. Cras tempus viverra quam non dignissim.

In id consectetur felis, ut ornare magna. Aliquam non pulvinar quam. Integer risus orci, malesuada se";

    [Test]
    public static void SplitMessageSplitsTextCorrectly()
    {
      var expected = new List<string>()
      {
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam iaculis gravida risus nec ornare. Praesent et auctor felis. Mauris eget imperdiet metus, sed tempor leo. Pellentesque cursus elit diam, sed",
        " tincidunt massa mattis vitae. Interdum et malesuada fames ac ante ipsum primis in faucibus. Phasellus blandit mauris ac ligula laoreet, eleifend luctus mauris dictum. Aliquam quis volutpat arcu, inte",
        "rdum rutrum neque. Nullam interdum sapien orci, eget varius erat ullamcorper sed. Duis lacinia sapien dolor, vel rhoncus nibh porta id. Aenean tristique accumsan dapibus. Pellentesque eget tortor volu",
        "tpat, convallis sem quis, tincidunt eros. Pellentesque nisl dui, gravida quis lacus ut, mattis suscipit quam. Praesent tempor odio sit amet euismod facilisis. Nullam nec bibendum nunc. Mauris vel semp",
        @"er dui, in auctor nulla. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae;

Pellentesque at purus a magna dictum mattis. In hac habitasse platea dictumst. Mauris ",
        "dictum libero scelerisque pharetra porta. Phasellus enim nibh, accumsan ut orci non, aliquam eleifend justo. Fusce laoreet at lorem in scelerisque. Vestibulum vestibulum cursus nibh sed placerat. Done",
        @"c velit enim, condimentum sed libero id, auctor vestibulum ipsum. Nullam et placerat nibh.

Quisque et eleifend lectus. Quisque cursus ipsum ac orci auctor auctor. Vivamus a tortor vitae tellus porta ",
        "scelerisque a et odio. Nulla pulvinar, massa vitae efficitur luctus, urna nunc aliquam orci, sit amet rutrum dui sapien a dolor. Cras eget elit nec libero sodales sodales quis eu ex. Mauris turpis lig",
        "ula, dignissim sed orci vel, mattis venenatis eros. Integer augue odio, gravida et lectus id, commodo mollis quam. Ut congue pretium tempor. Maecenas in interdum erat, nec blandit urna. Cras tempus vi",
        @"verra quam non dignissim.

In id consectetur felis, ut ornare magna. Aliquam non pulvinar quam. Integer risus orci, malesuada se"
      };
      var actual = Broadcaster.SplitMessage(TestInput, 200);
      Assert.That(actual, Is.EqualTo(expected));
    }
  }
}
