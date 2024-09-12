using Mcv.PluginV2;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhowatchSitePlugin;

namespace WhowatchSitePluginTests
{
    [TestFixture]
    class MessageParserTests
    {
        private Task<Dictionary<long, PlayItem>> CreatePlayItemsTestData()
        {
            var data = DataLoader.GetSampleData("PlayItems.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            return Api.GetPlayItemsAsync(server);
        }
        [Test]
        public async Task WhowatchItemParseTest()
        {
            var data = "[null,null,\"room:9184711\",\"shout\",{\"topic\":\"room:9184711\",\"event\":\"shout\",\"comment\":{\"user\":{\"user_profile\":{\"is_date_of_birth_today\":false},\"user_path\":\"w:satorou\",\"name\":\"🔹SAPPOROイケチャン🔹休止中\",\"is_admin\":false,\"id\":12764231,\"icon_url\":\"https://img.whowatch.tv/user_files/12764231/profile_icon/1545653225128.jpeg\",\"account_name\":\"ふ:satorou\"},\"tts\":{\"name\":\"SAPPOROイケチャン休止中\"},\"reply_to_user_id\":0,\"posted_at\":1547574410000,\"play_item_pattern_id\":16,\"pickup_time\":2000,\"not_escaped\":false,\"message\":\"メガホンをプレゼントしました。\",\"live_id\":9184711,\"item_count\":1,\"is_silent_comment\":false,\"is_reply_to_me\":false,\"id\":568502964,\"escaped_message\":\"メガホンをプレゼントしました。\",\"enabled\":true,\"comment_type\":\"BY_PLAYITEM\",\"anonymized\":false}}]";
            MessageParser.Resolver = new ItemNameResolver(await CreatePlayItemsTestData());
            var internalMessage = MessageParser.ParseRawString2InternalMessage(data);
            var message = MessageParser.ParseShoutMessage(internalMessage);

            var shout = message as IWhowatchItem;
            Assert.That(shout, Is.Not.Null);
            Assert.That(shout?.AccountName, Is.EqualTo("ふ:satorou"));
            Assert.That(shout?.Comment, Is.EqualTo("メガホンをプレゼントしました。"));
            Assert.That(shout?.Id, Is.EqualTo(568502964));
            Assert.That(shout?.PostedAt, Is.EqualTo(1547574410000));
            Assert.That(shout?.UserId, Is.EqualTo(12764231));
            Assert.That(shout?.UserName, Is.EqualTo("🔹SAPPOROイケチャン🔹休止中"));
            Assert.That(shout?.UserPath, Is.EqualTo("w:satorou"));
            Assert.That(shout?.WhowatchMessageType, Is.EqualTo(WhowatchMessageType.Item));
            Assert.That(shout?.ItemCount, Is.EqualTo(1));
            Assert.That(shout?.ItemName, Is.EqualTo("メガホン"));
        }
        [Test]
        public async Task WhowatchByOwnerParseTest()
        {
            var data = "[null,null,\"room:11089976\",\"shout\",{\"topic\":\"room_pub:11089976\",\"event\":\"shout\",\"comment\":{\"user\":{\"user_profile\":{},\"user_path\":\"t:kv510k\",\"name\":\"Ryu\",\"is_admin\":false,\"id\":1072838,\"icon_url\":\"\",\"account_name\":\"@kv510k\"},\"tts\":{},\"reply_to_user_id\":0,\"posted_at\":1560538378000,\"not_escaped\":false,\"ng_word_included\":false,\"message\":\"てｓｔ\",\"live_id\":11089976,\"is_silent_comment\":false,\"is_reply_to_me\":false,\"id\":714170546,\"escaped_message\":\"てｓｔ\",\"enabled\":true,\"comment_type\":\"BY_OWNER\",\"anonymized\":false}}]";
            MessageParser.Resolver = new ItemNameResolver(await CreatePlayItemsTestData());
            var internalMessage = MessageParser.ParseRawString2InternalMessage(data);
            var message = MessageParser.ParseShoutMessage(internalMessage);

            var shout = message as IWhowatchComment;
            Assert.That(shout, Is.Not.Null);
            Assert.That(shout?.AccountName, Is.EqualTo("@kv510k"));
            Assert.That(shout?.Comment, Is.EqualTo("てｓｔ"));
            Assert.That(shout?.Id, Is.EqualTo("714170546"));
            Assert.That(shout?.PostTime, Is.EqualTo("03:52:58"));
            Assert.That(shout?.UserId, Is.EqualTo("1072838"));
            Assert.That(shout?.UserName, Is.EqualTo("Ryu"));
            Assert.That(shout?.UserPath, Is.EqualTo("t:kv510k"));
        }
        [Test]
        public void Test()
        {
            var data = "[null,null,\"room:9184711\",\"shout\",{\"a\"}]";
            var message = MessageParser.ParseRawString2InternalMessage(data);
            Assert.That(message.JoinRef, Is.Null);
            Assert.That(message.Ref, Is.Null);
            Assert.That(message.Topic, Is.EqualTo("room:9184711"));
            Assert.That(message.Event, Is.EqualTo("shout"));
            Assert.That(message.Payload, Is.EqualTo("{\"a\"}"));
        }
        [Test]
        public void Test1()
        {
            var data = "[\"1\",null,\"room:9184711\",\"shout\",{\"a\"}]";
            var message = MessageParser.ParseRawString2InternalMessage(data);
            Assert.That(message.JoinRef, Is.EqualTo(1));
            Assert.That(message.Ref, Is.Null);
            Assert.That(message.Topic, Is.EqualTo("room:9184711"));
            Assert.That(message.Event, Is.EqualTo("shout"));
            Assert.That(message.Payload, Is.EqualTo("{\"a\"}"));
        }
        [Test]
        public void Test2()
        {
            var data = "[null,\"1\",\"room:9184711\",\"shout\",{\"a\"}]";
            var message = MessageParser.ParseRawString2InternalMessage(data);
            Assert.That(message.Ref, Is.EqualTo(1));
            Assert.That(message.JoinRef, Is.Null);
            Assert.That(message.Topic, Is.EqualTo("room:9184711"));
            Assert.That(message.Event, Is.EqualTo("shout"));
            Assert.That(message.Payload, Is.EqualTo("{\"a\"}"));
        }
        [Test]
        public void Test3()
        {
            var data = "[\"1\",\"1\",\"room:9184711\",\"shout\",{\"a\"}]";
            var message = MessageParser.ParseRawString2InternalMessage(data);
            Assert.That(message.JoinRef, Is.EqualTo(1));
            Assert.That(message.Ref, Is.EqualTo(1));
            Assert.That(message.Topic, Is.EqualTo("room:9184711"));
            Assert.That(message.Event, Is.EqualTo("shout"));
            Assert.That(message.Payload, Is.EqualTo("{\"a\"}"));
        }
        [Test]
        public void NGCommentParseTest()
        {
            var data = "[null,null,\"room:10030668\",\"shout\",{\"topic\":\"room_pub:10030668\",\"event\":\"shout\",\"comment\":{\"user\":{\"user_profile\":{},\"user_path\":\"w:ryu_s\",\"name\":\"Ryu\",\"is_admin\":false,\"id\":1614280,\"icon_url\":\"\",\"account_name\":\"ふ:ryu_s\"},\"tts\":{},\"reply_to_user_id\":0,\"posted_at\":1553429971000,\"original_message\":\"あいうえお\",\"not_escaped\":false,\"ng_word_included\":true,\"message\":\"この投稿は視聴者には表示されません。\",\"live_id\":10030668,\"is_silent_comment\":true,\"is_reply_to_me\":false,\"id\":633344989,\"escaped_original_message\":\"<span class=\\\"ngword\\\">あいうえお</span>\",\"escaped_message\":\"この投稿は視聴者には表示されません。\",\"enabled\":true,\"comment_type\":\"BY_FOLLOWER\",\"anonymized\":false}}]";
            var internalMessage = MessageParser.ParseRawString2InternalMessage(data);
            Assert.That(internalMessage.InternalMessageType, Is.EqualTo(WhowatchInternalMessageType.Shout));
            var message = MessageParser.ParseShoutMessage(internalMessage);
            var ngComment = message as IWhowatchNgComment;
            Assert.That(ngComment, Is.Not.Null);
            Assert.That(ngComment?.OriginalMessage, Is.EqualTo("あいうえお"));
        }

    }
}
