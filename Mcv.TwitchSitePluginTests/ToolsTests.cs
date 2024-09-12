using System;
using TwitchSitePlugin;
using System.Net;
using NUnit.Framework;

namespace TwitchSitePluginTests
{
    [TestFixture]
    public class ToolsTests
    {
        [Test]
        public void ResultTest()
        {
            var t = Tools.Parse("@badges=subscriber/24,bits/1000;color=#00380B;display-name=こにゃった;emotes=;id=a49d99cd-b198-4598-be61-83c51fed6676;mod=0;room-id=39587048;sent-ts=1518059637527;subscriber=1;tmi-sent-ts=1518059639596;turbo=0;user-id=25317975;user-type= :konyatta!konyatta@konyatta.tmi.twitch.tv PRIVMSG #mimicchi :小便まみれの信雄ハウス。");
            return;
        }
        [Test]
        public void ParseTest()
        {
            var result = Tools.Parse("@badges=subscriber/24,bits/1000;color=#00380B;display-name=こにゃった;emotes=;id=a49d99cd-b198-4598-be61-83c51fed6676;mod=0;room-id=39587048;sent-ts=1518059637527;subscriber=1;tmi-sent-ts=1518059639596;turbo=0;user-id=25317975;user-type= :konyatta!konyatta@konyatta.tmi.twitch.tv PRIVMSG #mimicchi :小便まみれの信雄ハウス。");
            //TODO:
        }
        [Test]
        public void GetChannelNameTest()
        {
            Assert.That(Tools.GetChannelName("ryu123"), Is.EqualTo("ryu123"));
            Assert.That(Tools.GetChannelName("https://www.twitch.tv/ryu123"), Is.EqualTo("ryu123"));
            Assert.That(Tools.GetChannelName("https://www.twitch.tv/ryu123?abc"), Is.EqualTo("ryu123"));
            Assert.Throws<ArgumentException>(() => Tools.GetChannelName("test.net/ryu123"));
        }
        [Test]
        public void ExtractCookiesTest()
        {
            var cookie = new Cookie("a", "b") { Domain = "int-main.net", Path = "/" };
            var cc = new CookieContainer();
            cc.Add(cookie);
            var list = Tools.ExtractCookies(cc);
            Assert.That(list[0].Name, Is.EqualTo("a"));
            Assert.That(list[0].Value, Is.EqualTo("b"));
            Assert.That(list[0].Domain, Is.EqualTo("int-main.net"));
            Assert.That(list[0].Path, Is.EqualTo("/"));
        }
        [Test]
        public void GetRandomGuestUsernameTest()
        {
            var s = Tools.GetRandomGuestUsername();
            Assert.That(System.Text.RegularExpressions.Regex.IsMatch(s, "justinfan\\d+"), Is.True);
        }
        [Test]
        public void RemoveActionFormatTestsz()//(string input, string expected)
        {
            //[TestCase]だと何故か認識されない　https://github.com/nunit/nunit3-vs-adapter/issues/484
            Assert.That(Tools.RemoveActionFormat("\u0001ACTION abc\u0001"), Is.EqualTo("abc"));
            Assert.That(Tools.RemoveActionFormat("xyz"), Is.EqualTo("xyz"));
        }
    }
}
