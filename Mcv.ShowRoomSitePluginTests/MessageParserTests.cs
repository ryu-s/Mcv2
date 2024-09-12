using NUnit.Framework;
using ShowRoomSitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowRoomSitePluginTests
{
    [TestFixture]
    public class MessageParserTests
    {
        [Test]
        public void PingParseTest()
        {
            var data = "PING\tshowroom";
            var internalMessage = MessageParser.Parse(data) as Ping;
            Assert.That(internalMessage, Is.Not.Null);
        }
        [Test]
        public void PongParseTest()
        {
            var data = "PONG\tshowroom";
            var internalMessage = MessageParser.Parse(data) as Pong;
            Assert.That(internalMessage, Is.Not.Null);
        }
        [Test]
        public void Type1ParseTest()
        {
            var data = "MSG\t6cda70:87HHYS8k\t{\"av\":1014474,\"d\":0,\"ac\":\"しまやん♥\",\"cm\":\"マイクが小さい\",\"u\":2370410,\"created_at\":1561880210,\"at\":0,\"t\":\"1\"}";
            var internalMessage = MessageParser.Parse(data) as T1;
            Assert.That(internalMessage, Is.Not.Null);
            Assert.That(internalMessage.UserName, Is.EqualTo("しまやん♥"));
            Assert.That(internalMessage.Comment, Is.EqualTo("マイクが小さい"));
            Assert.That(internalMessage.CreatedAt, Is.EqualTo(1561880210));
            Assert.That(internalMessage.MessageType, Is.EqualTo(InternalMessageType.t1));
            Assert.That(internalMessage.UserId, Is.EqualTo(2370410));
        }
        [Test]
        public void Type2ParseTest()
        {
            var data = "MSG\t6cda70:87HHYS8k\t{\"n\":10,\"av\":1001422,\"d\":8,\"ac\":\"マカオのりゅう\",\"created_at\":1561880211,\"u\":842213,\"h\":0,\"g\":1001,\"gt\":2,\"at\":0,\"t\":\"2\"}";
            var internalMessage = MessageParser.Parse(data) as T2;
            Assert.That(internalMessage, Is.Not.Null);
            Assert.That(internalMessage?.Ac, Is.EqualTo("マカオのりゅう"));
            Assert.That(internalMessage?.At, Is.EqualTo(0));
            Assert.That(internalMessage?.Av, Is.EqualTo(1001422));

            Assert.That(internalMessage?.CreatedAt, Is.EqualTo(1561880211));
            Assert.That(internalMessage?.D, Is.EqualTo(8));
            Assert.That(internalMessage?.G, Is.EqualTo(1001));
            Assert.That(internalMessage?.Gt, Is.EqualTo(2));
            Assert.That(internalMessage?.H, Is.EqualTo(0));
            Assert.That(internalMessage?.N, Is.EqualTo(10));
            Assert.That(internalMessage?.MessageType, Is.EqualTo(InternalMessageType.t2));
            Assert.That(internalMessage?.U, Is.EqualTo(842213));
        }
    }
}
