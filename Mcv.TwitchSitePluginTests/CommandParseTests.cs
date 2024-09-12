using System.Collections.Generic;
using TwitchSitePlugin;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TwitchSitePluginTests
{
    [TestFixture]
    public class CommandParseTests
    {
        [Test]
        public void TwitchCommand001ParseTest()
        {
            var s = ":tmi.twitch.tv 001 justinfan12345 :Welcome, GLHF!";
            var result = Tools.Parse(s);
            Assert.That(result.Prefix, Is.EqualTo("tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("001"));
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "Welcome, GLHF!" }, result.Params);
        }
        [Test]
        public void TwitchCommand002ParseTest()
        {
            var s = ":tmi.twitch.tv 002 justinfan12345 :Your host is tmi.twitch.tv";
            var result = Tools.Parse(s);
            Assert.That(result.Prefix, Is.EqualTo("tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("002"));
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "Your host is tmi.twitch.tv" }, result.Params);
        }
        [Test]
        public void TwitchCommand003ParseTest()
        {
            var s = ":tmi.twitch.tv 003 justinfan12345 :This server is rather new";
            var result = Tools.Parse(s);
            Assert.That(result.Prefix, Is.EqualTo("tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("003"));
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "This server is rather new" }, result.Params);
        }
        [Test]
        public void TwitchCommand004ParseTest()
        {
            var s = ":tmi.twitch.tv 004 justinfan12345 :-";
            var result = Tools.Parse(s);
            Assert.That(result.Prefix, Is.EqualTo("tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("004"));
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "-" }, result.Params);
        }
        [Test]
        public void TwitchCommand353ParseTest()
        {
            var s = ":justinfan12345.tmi.twitch.tv 353 justinfan12345 = #gugu2525 :justinfan12345";
            var result = Tools.Parse(s);
            Assert.That(result.Prefix, Is.EqualTo("justinfan12345.tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("353"));
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "=", "#gugu2525", "justinfan12345" }, result.Params);
        }
        [Test]
        public void TwitchCommand366ParseTest()
        {
            var s = ":justinfan12345.tmi.twitch.tv 366 justinfan12345 #gugu2525 :End of /NAMES list";
            var result = Tools.Parse(s);
            Assert.That(result.Prefix, Is.EqualTo("justinfan12345.tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("366"));
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "#gugu2525", "End of /NAMES list" }, result.Params);
        }
        [Test]
        public void TwitchCommand372ParseTest()
        {
            var s = ":tmi.twitch.tv 372 justinfan12345 :You are in a maze of twisty passages, all alike.";
            var result = Tools.Parse(s);
            Assert.That(result.Prefix, Is.EqualTo("tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("372"));
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "You are in a maze of twisty passages, all alike." }, result.Params);
        }
        [Test]
        public void TwitchCommand375ParseTest()
        {
            var s = ":tmi.twitch.tv 375 justinfan12345 :-";
            var result = Tools.Parse(s);
            Assert.That(result.Prefix, Is.EqualTo("tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("375"));
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", "-" }, result.Params);
        }
        [Test]
        public void TwitchCommand376ParseTest()
        {
            var s = ":tmi.twitch.tv 376 justinfan12345 :>";
            var result = Tools.Parse(s);
            Assert.That(result.Prefix, Is.EqualTo("tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("376"));
            CollectionAssert.AreEquivalent(new List<string> { "justinfan12345", ">" }, result.Params);
        }
        [Test]
        public void TwitchCommandJOINParseTest()
        {
            var s = ":justinfan12345!justinfan12345@justinfan12345.tmi.twitch.tv JOIN #gugu2525";
            var result = Tools.Parse(s);
            Assert.That(result.Prefix, Is.EqualTo("justinfan12345!justinfan12345@justinfan12345.tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("JOIN"));
            CollectionAssert.AreEquivalent(new List<string> { "#gugu2525" }, result.Params);
        }
        [Test]
        public void TwitchCommandPRIVMSGParseTest()
        {
            var s = "@badges=premium/1;color=;display-name=clllp;emotes=;id=3c18fda8-8008-460d-9442-ca4451b5fb84;mod=0;room-id=95066484;subscriber=0;tmi-sent-ts=1517933598965;turbo=0;user-id=156711248;user-type= :clllp!clllp@clllp.tmi.twitch.tv PRIVMSG #gugu2525 :意識過剰もほどほどに";
            var result = Tools.Parse(s);
            CollectionAssert.AreEquivalent(new Dictionary<string, string>
            {
                {"badges","premium/1" },
                {"color","" },
                {"display-name","clllp" },
                { "emotes", ""},
                { "id","3c18fda8-8008-460d-9442-ca4451b5fb84"},
                { "mod","0"},
                { "room-id","95066484"},
                { "subscriber","0"},
                { "tmi-sent-ts","1517933598965"},
                { "turbo","0"},
                { "user-id","156711248"},
                { "user-type",""},
            }, result.Tags);
            Assert.That(result.Prefix, Is.EqualTo("clllp!clllp@clllp.tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("PRIVMSG"));
            CollectionAssert.AreEquivalent(new List<string> { "#gugu2525", "意識過剰もほどほどに" }, result.Params);
        }
        [Test]
        public void TwitchCommandPRIVMSGParseTest2()
        {
            var s = ":jtv!jtv@jtv.tmi.twitch.tv PRIVMSG 3lis_game :GamesFan34260 is now hosting you.";
            var result = Tools.Parse(s);
            var commentData = Tools.ParsePrivMsg(result);
            Assert.That(result.Prefix, Is.EqualTo("jtv!jtv@jtv.tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("PRIVMSG"));
            CollectionAssert.AreEquivalent(new List<string> { "3lis_game", "GamesFan34260 is now hosting you." }, result.Params);
        }
        [Test]
        public void TwitchCommandROOMSTATEParseTest()
        {
            var s = "@broadcaster-lang=ja;emote-only=0;followers-only=-1;mercury=0;r9k=0;rituals=0;room-id=95066484;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #gugu2525";
            var result = Tools.Parse(s);

            var dict = new Dictionary<string, string>
            {
                {"broadcaster-lang","ja" },
                {"emote-only","0" },
                { "followers-only","-1" },
                { "mercury","0" },
                { "r9k","0" },
                { "rituals","0" },
                { "room-id","95066484" },
                {"slow","0" },
                { "subs-only","0"},
            };
            CollectionAssert.AreEquivalent(dict, result.Tags);
            Assert.That(result.Prefix, Is.EqualTo("tmi.twitch.tv"));
            Assert.That(result.Command, Is.EqualTo("ROOMSTATE"));
            CollectionAssert.AreEquivalent(new List<string> { "#gugu2525" }, result.Params);
        }
    }
}
