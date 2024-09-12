using Mcv.YouTubeLiveSitePlugin;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
namespace YouTubeLiveSitePluginTests
{
    [TestFixture]
    class VidResolverTests
    {
        [Test]
        public async Task ResolveVidFromWatchUrl()
        {
            var s = new VidResolver();
            var serverMock = new Mock<IYouTubeLiveServer>();
            var result1 = await s.GetVid(serverMock.Object, new WatchUrl("https://www.youtube.com/watch?v=Rs-WxTGgVus"));
            Assert.That(result1, Is.InstanceOf<IVidResult>());
            Assert.That(((VidResult)result1).Vid, Is.EqualTo("Rs-WxTGgVus"));

            var result2 = await s.GetVid(serverMock.Object, new WatchUrl("https://www.youtube.com/watch?v=Rs-WxTGgVus&feature=test"));
            Assert.That(result2, Is.InstanceOf<IVidResult>());
            Assert.That(((VidResult)result2).Vid, Is.EqualTo("Rs-WxTGgVus"));
        }
    }
}
