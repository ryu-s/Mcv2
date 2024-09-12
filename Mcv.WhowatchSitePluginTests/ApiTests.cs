using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WhowatchSitePlugin;

namespace WhowatchSitePluginTests
{
    [TestFixture]
    class ApiTests
    {
        [Test]
        public async Task GetLiveDataTest()
        {
            var live_id = 123;
            var lastUpdatedAt = 0;
            var data = DataLoader.GetSampleData("LiveData.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var cc = new CookieContainer();
            var liveData = await Api.GetLiveDataAsync(serverMock.Object, live_id, lastUpdatedAt, cc);
            Assert.That(liveData.Live.Title, Is.EqualTo("👼🏻こっちん様🎀のライブ"));
            Assert.That(liveData.UpdatedAt, Is.EqualTo(1532194251671));
            Assert.That(liveData.Comments[0].Id, Is.EqualTo(413277878));
            Assert.That(liveData.Comments[0].User.Id, Is.EqualTo(1003));
            Assert.That(liveData.Comments[0].User.Name, Is.EqualTo("匿名係長ただの花火師"));
        }
        [Test]
        public async Task GetProfileTest()
        {
            var userPath = "w:abc";
            var data = DataLoader.GetSampleData("Profile.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync("https://api.whowatch.tv/users/" + userPath + "/profile", It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var cc = new CookieContainer();
            var profile = await Api.GetProfileAsync(serverMock.Object, userPath, cc);
            Assert.That(profile.Name, Is.EqualTo("👼🏻こっちん様🎀"));
            Assert.That(profile.AccountName, Is.EqualTo("ふ:koto0316"));
            Assert.That(profile.Live.Id, Is.EqualTo(7005919));
            Assert.That(profile.Live.Title, Is.EqualTo("👼🏻こっちん様🎀のライブ"));
            Assert.That(profile.Live.StartedAt, Is.EqualTo(1532189547000));
        }
        [Test]
        public async Task MeTest()
        {
            var ret = "{\"id\":1072838,\"user_type\":\"VALID\",\"user_code\":\"1778649641148661\",\"account_register_status\":\"TWITTER\",\"whowatch_point\":0,\"icon_url\":\"\",\"account_name\":\"@kv510k\",\"user_path\":\"t:kv510k\",\"name\":\"Ryu\",\"is_email_registered\":false,\"is_twitter_connected\":true,\"is_facebook_connected\":false,\"is_related_account_auto_blocked\":false}";
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync("https://api.whowatch.tv/users/me", It.IsAny<CookieContainer>())).Returns(Task.FromResult(ret));
            var cc = new CookieContainer();
            var me = await Api.GetMeAsync(serverMock.Object, cc);
            Assert.That(me.AccountName, Is.EqualTo("@kv510k"));
            Assert.That(me.UserPath, Is.EqualTo("t:kv510k"));
            Assert.That(me.Name, Is.EqualTo("Ryu"));
        }
        [Test]
        public async Task GetPlayItems()
        {
            var data = DataLoader.GetSampleData("PlayItems.txt");
            var serverMock = new Mock<IDataServer>();
            var server = serverMock.Object;
            serverMock.Setup(s => s.GetAsync("https://api.whowatch.tv/playitems")).Returns(Task.FromResult(data));
            var dict = await Api.GetPlayItemsAsync(server);
            var item = dict[182];
            Assert.That(item.Name, Is.EqualTo("大花火"));
        }
    }
}
