using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using OpenrecSitePlugin;
using Newtonsoft.Json;
namespace OpenrecSitePluginTests
{
    [TestFixture]
    class ToolsTests
    {
        [Test]
        public async Task GetLiveIdTest()
        {
            //https://www.openrec.tv/live/FkJTQSjkQps
            //https://www.openrec.tv/user/Matomo_SV
            //https://www.openrec.tv/live/Matomo_SV

            var data = DataLoader.GetSampleData(@"External\Movies\TextFile1.txt");
            var serverMock = new Mock<IDataSource>();
            serverMock.Setup(s => s.GetAsync("https://public.openrec.tv/external/api/v5/movies?channel_id=FkJTQSjkQps")).ReturnsAsync("[" + "]");
            serverMock.Setup(s => s.GetAsync("https://public.openrec.tv/external/api/v5/movies?channel_id=Matomo_SV")).ReturnsAsync("[" + data + "]");
            Assert.That(await Tools.GetLiveId(serverMock.Object, "https://www.openrec.tv/live/FkJTQSjkQps"), Is.EqualTo("FkJTQSjkQps"));
            Assert.That(await Tools.GetLiveId(serverMock.Object, "https://www.openrec.tv/user/Matomo_SV"), Is.EqualTo("FkJTQSjkQps"));
            Assert.That(await Tools.GetLiveId(serverMock.Object, "https://www.openrec.tv/live/Matomo_SV"), Is.EqualTo("FkJTQSjkQps"));

        }
        [Test]
        public void IsValidUrlTest()
        {
            Assert.That(Tools.IsValidUrl("https://www.openrec.tv/live/mizLXCyaDLk"), Is.True);
            Assert.That(Tools.IsValidUrl("https://www.openrec.tv/movie/mizLXCyaDLk"), Is.True);
            Assert.That(Tools.IsValidUrl("https://www.openrec.tv/a/mizLXCyaDLk"), Is.False);

        }
        [Test]
        public void ExtractLiveIdTest()
        {
            Assert.That(Tools.ExtractLiveId("https://www.openrec.tv/live/mizLXCyaDLk"), Is.EqualTo("mizLXCyaDLk"));
            Assert.That(Tools.ExtractLiveId("https://www.openrec.tv/movie/mizLXCyaDLk"), Is.EqualTo("mizLXCyaDLk"));
            Assert.That(Tools.ExtractLiveId("https://www.openrec.tv/a/mizLXCyaDLk"), Is.EqualTo(""));
        }
        [Test]
        public void WebsocketMessageType0ParseTest()
        {
            var data = DataLoader.GetSampleData("Websocket\\MessageType0.txt");
            var obj = JsonConvert.DeserializeObject<OpenrecSitePlugin.Low.Item>(data);
            var comment = Tools.Parse(obj);
            Assert.That(comment.Message, Is.EqualTo("運だけ〜"));
            Assert.That(comment.Id, Is.EqualTo("182743145"));
            Assert.That(comment.IsPremium, Is.True);
        }
        [Test]
        public void ChatsParseTest()
        {
            var data = DataLoader.GetSampleData("Chats_stamp.txt");
            var obj = JsonConvert.DeserializeObject<OpenrecSitePlugin.Low.Chats.RootObject[]>(data);
            var comment = Tools.Parse(obj[10]);
            Assert.That(comment.Message, Is.EqualTo(""));
            Assert.That(comment.Id, Is.EqualTo("182716576"));
            Assert.That(comment.IsPremium, Is.True);
            Assert.That(comment.StampUrl, Is.EqualTo("https://dqd0jw5gvbchn.cloudfront.net/stamp/15/128/16a1341ae3788fa55c84cb05be3663825f5df7e2.png"));
        }
    }
}
