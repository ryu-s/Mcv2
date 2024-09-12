using NUnit.Framework;
using TwitchSitePlugin;
using Moq;
using Mcv.PluginV2;

namespace TwitchSitePluginTests
{
    [TestFixture]
    class TwitchSiteContextTests
    {
        [Test]
        public void TwitchSite_IsValdUrlTest()
        {
            var loggerMock = new Mock<ILogger>();
            var serverMock = new Mock<IDataServer>();
            var webSocket = new Mock<IMessageProvider>();
            var site = new TwitchSiteContext(serverMock.Object, loggerMock.Object);
            Assert.That(site.IsValidInput("https://www.twitch.tv/abc"), Is.True);
            Assert.That(site.IsValidInput("https://www.twitch.tv/abc?"), Is.True);
            Assert.That(site.IsValidInput("https://www.twitch.tv/abc/"), Is.True);
            Assert.That(site.IsValidInput("https://www.twitch.tv/"), Is.False);
            Assert.That(site.IsValidInput("https://www.twitch.tv/?"), Is.False);
        }
    }
}
