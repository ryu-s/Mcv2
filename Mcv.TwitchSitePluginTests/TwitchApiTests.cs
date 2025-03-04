using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwitchSitePlugin;
using TwitchSitePlugin.V2.TwitchApi;

namespace Mcv.TwitchSitePluginTests;
[TestFixture]
internal class TwitchApiTests
{
    [Test]
    public async Task GetGlobalBadgesTests()
    {
        var server = new TwitchServer();
        var badges = await TwitchApi.GetGlobalBadges(server);

    }
    [Test]
    public async Task GetChatHistoryTests2()
    {
        var server = new TwitchServer();
        var chats = await TwitchApi.GetChatHistory(server, new ChannelLogin("penko_channel"), "");
    }
    [Test]
    public async Task GetChatHistoryTests()
    {
        var serverMock = new Mock<IDataServer>();
        var data = "{\"data\":{\"channel\":{\"id\":\"276371469\",\"recentChatMessages\":[{\"id\":\"a8368f30-7a32-4c78-881c-865437fde2aa\",\"deletedAt\":null,\"sentAt\":\"2025-02-16T21:31:15.304673106Z\",\"content\":{\"text\":\"あいう penkoDainoji\",\"fragments\":[{\"text\":\"あいう\",\"content\":null,\"__typename\":\"MessageFragment\"},{\"text\":\"penkoDainoji\",\"content\":{\"emoteID\":\"emotesv2_696b32e8befc454d9679de92b45dd020\",\"setID\":\"305456859\",\"token\":\"penkoDainoji\",\"__typename\":\"Emote\"},\"__typename\":\"MessageFragment\"}],\"__typename\":\"MessageContent\"},\"parentMessage\":null,\"threadParentMessage\":null,\"sender\":{\"id\":\"81723940\",\"login\":\"raatoman\",\"displayName\":\"Raatoman\",\"__typename\":\"User\"},\"senderBadges\":[{\"setID\":\"founder\",\"version\":\"0\",\"id\":\"Zm91bmRlcjswOw==\",\"__typename\":\"Badge\"}],\"senderChatColor\":null,\"sourceChannel\":null,\"sourceSenderBadges\":null,\"__typename\":\"Message\"},{\"id\":\"14b8c52a-a270-48eb-8145-cc3a806c56b2\",\"deletedAt\":null,\"sentAt\":\"2025-02-16T21:31:19.158254214Z\",\"content\":{\"text\":\"かきく\",\"fragments\":[{\"text\":\"かきく\",\"content\":null,\"__typename\":\"MessageFragment\"}],\"__typename\":\"MessageContent\"},\"parentMessage\":null,\"threadParentMessage\":null,\"sender\":{\"id\":\"19264788\",\"login\":\"nightbot\",\"displayName\":\"Nightbot\",\"__typename\":\"User\"},\"senderBadges\":[{\"setID\":\"moderator\",\"version\":\"1\",\"id\":\"bW9kZXJhdG9yOzE7\",\"__typename\":\"Badge\"},{\"setID\":\"partner\",\"version\":\"1\",\"id\":\"cGFydG5lcjsxOw==\",\"__typename\":\"Badge\"}],\"senderChatColor\":\"#7C7CE1\",\"sourceChannel\":null,\"sourceSenderBadges\":null,\"__typename\":\"Message\"},],\"__typename\":\"Channel\"}},\"extensions\":{\"durationMilliseconds\":50,\"operationName\":\"MessageBufferChatHistory\",\"requestID\":\"01JM8DSY7DGHJK2JYSBQ7CFJGV\"}}";
        serverMock.Setup(s => s.PostAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
        var server = serverMock.Object;

        var chats = await TwitchApi.GetChatHistory(server, new ChannelLogin("penko_channel"), "");
        Assert.That(chats[0].ContentFragments[0].Text, Is.EqualTo("あいう"));
        Assert.That(chats[0].ContentFragments[1].Text, Is.EqualTo("penkoDainoji"));
    }
    [Test]
    public async Task GetIdFromLoginTests()
    {
        var serverMock = new Mock<IDataServer>();
        var data = "{\"data\":{\"user\":{\"id\":\"512535130\",\"__typename\":\"User\"}},\"extensions\":{\"durationMilliseconds\":21,\"operationName\":\"GetIDFromLogin\",\"requestID\":\"01JMA5G9DTJVQFSGBGBQVNF2ZE\"}}";
        serverMock.Setup(s => s.PostAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
        var server = serverMock.Object;

        var id = await TwitchApi.GetIdFromLoginAsync(server, "darkzelk");
        Assert.That(id, Is.EqualTo("512535130"));
    }
    [Test]
    public async Task GetSubscriptionProductsTests()
    {
        var serverMock = new Mock<IDataServer>();
        var data = "{\"data\":{\"user\":{\"id\":\"512535130\",\"subscriptionProducts\":[{\"id\":\"403178356\",\"name\":\"darkzelk\",\"tier\":\"1000\",\"gifting\":{\"community\":null,\"__typename\":\"SubscriptionGifting\"},\"__typename\":\"SubscriptionProduct\"},{\"id\":\"403178357\",\"name\":\"darkzelk_2000\",\"tier\":\"2000\",\"gifting\":{\"community\":null,\"__typename\":\"SubscriptionGifting\"},\"__typename\":\"SubscriptionProduct\"},{\"id\":\"403178358\",\"name\":\"darkzelk_3000\",\"tier\":\"3000\",\"gifting\":{\"community\":null,\"__typename\":\"SubscriptionGifting\"},\"__typename\":\"SubscriptionProduct\"}],\"__typename\":\"User\"}},\"extensions\":{\"durationMilliseconds\":65,\"operationName\":\"ChannelProductsWithCommunityGiftOffers\",\"requestID\":\"01JMAAB9RSKR10FANF2M62GF9W\"}}";
        serverMock.Setup(s => s.PostAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
        var server = serverMock.Object;

        var ns = await TwitchApi.GetSubscriptionProductsAsync(server, new ChannelId("512535130"));
        Assert.That(ns[0].Id, Is.EqualTo("403178356"));
        Assert.That(ns[1].Id, Is.EqualTo("403178357"));
        Assert.That(ns[2].Id, Is.EqualTo("403178358"));
    }
    [Test]
    public async Task GetLastBroadcastTests()
    {
        var serverMock = new Mock<IDataServer>();
        var data = "{\"data\":{\"user\":{\"id\":\"512535130\",\"lastBroadcast\":{\"id\":\"123456789\",\"title\":\"title1\",\"game\":{\"id\":\"32982\",\"slug\":\"grand-theft-auto-v\",\"name\":\"Grand Theft Auto V\",\"displayName\":\"Grand Theft Auto V\",\"__typename\":\"Game\"},\"__typename\":\"Broadcast\"},\"__typename\":\"User\"}},\"extensions\":{\"durationMilliseconds\":56,\"operationName\":\"UseLiveBroadcast\",\"requestID\":\"01JMAKFRXWR2ZV7ZN3WY8P36EJ\"}}";
        serverMock.Setup(s => s.PostAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
        var server = serverMock.Object;
        var broadcast = await TwitchApi.GetLastBroadcastAsync(server, new ChannelLogin("darkzelk"));
        if (broadcast == null)
        {
            Assert.Fail();
            return;
        }
        Assert.That(broadcast.LastBroadcastId, Is.EqualTo("123456789"));
        Assert.That(broadcast.LastBroadcastTitle, Is.EqualTo("title1"));
    }
    [Test]
    public async Task GetCelebrationEmotesTests()
    {
        //var serverMock = new Mock<IDataServer>();
        //var data = "{\"data\":{\"user\":{\"id\":\"512535130\",\"channel\":{\"id\":\"512535130\",\"login\":\"darkzelk\",\"displayName\":\"DarkZelk\",\"__typename\":\"Channel\"},\"__typename\":\"User\"}},\"extensions\":{\"durationMilliseconds\":21,\"operationName\":\"GetChannelInfo\",\"requestID\":\"01JMA5G9DTJVQFSGBGBQVNF2ZE\"}}";
        //serverMock.Setup(s => s.PostAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
        //var server = serverMock.Object;
        var server = new TwitchServer();

        var channel = await TwitchApi.GetCelebrationEmotesAsync(server, new ChannelId("512535130"));
        //Assert.That(channel.Id, Is.EqualTo("512535130"));
        //Assert.That(channel.Login, Is.EqualTo("darkzelk"));
        //Assert.That(channel.DisplayName, Is.EqualTo("DarkZelk"));
    }
    [Test]
    public async Task GetViewCountTests()
    {
        var serverMock = new Mock<IDataServer>();
        var data = "{\"data\":{\"user\":{\"id\":\"865792830\",\"stream\":{\"id\":\"316370001276\",\"viewersCount\":580,\"collaborationViewersCount\":null,\"__typename\":\"Stream\"},\"__typename\":\"User\"}},\"extensions\":{\"durationMilliseconds\":39,\"operationName\":\"UseViewCount\",\"requestID\":\"01JMBR672N5ZDG00K76MFZB3BQ\"}}";
        serverMock.Setup(s => s.PostAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
        var server = serverMock.Object;

        await TwitchApi.GetViewCount(server, new ChannelLogin("kentsumeshi_heyoo"));
    }
}
