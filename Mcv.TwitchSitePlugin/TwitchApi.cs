using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwitchSitePlugin.V2.TwitchApi;
static class TwitchApi
{
    const string clientId = "kimne78kx3ncx6brgo4mv6wki5h1ko";
    public static async Task<Dictionary<string, Badge>> GetGlobalBadges(IDataServer server)
    {
        var payload = $"{{\"operationName\":\"GlobalBadges\",\"variables\":{{}},\"extensions\":{{\"persistedQuery\":{{\"version\":1,\"sha256Hash\":\"9db27e18d61ee393ccfdec8c7d90f14f9a11266298c2e5eb808550b77d7bcdf6\"}}}}}}";
        var url = "https://gql.twitch.tv/gql";
        var headers = new Dictionary<string, string>
        {
            { "Client-Id", clientId },
            { "Accept-Language","ja-JP" },
        };
        var data = await server.PostAsync(url, headers, payload, new CookieContainer());
        var badges = Badges.Parse(data);
        return badges;
    }
    public static async Task<List<RecentChatMessage>> GetChatHistory(IDataServer server, ChannelLogin login, string oauth)
    {
        var payload = $"{{\"operationName\":\"MessageBufferChatHistory\",\"variables\":{{\"channelLogin\":\"{login.Raw}\"}},\"extensions\":{{\"persistedQuery\":{{\"version\":1,\"sha256Hash\":\"7d83d04b5d437c18899b50f909ceec5d17fecfec9757e31ab19cfc1a2fbd3890\"}}}}}}";
        var url = "https://gql.twitch.tv/gql";
        var headers = new Dictionary<string, string>
        {
            { "Client-Id", clientId },
            {"Authorization", $"OAuth {oauth}"},
            {"Origin","https://www.twitch.tv" },
        };
        var json = await server.PostAsync(url, headers, payload, new CookieContainer());
        return RecentChatMesages.GetRecentChatMessages(json);
    }
    public static async Task<ChannelId?> GetIdFromLoginAsync(IDataServer server, string login)
    {
        var payload = $"{{\"operationName\":\"GetIDFromLogin\",\"variables\":{{\"login\":\"{login}\"}},\"extensions\":{{\"persistedQuery\":{{\"version\":1,\"sha256Hash\":\"94e82a7b1e3c21e186daa73ee2afc4b8f23bade1fbbff6fe8ac133f50a2f58ca\"}}}}}}";
        var url = "https://gql.twitch.tv/gql";
        var headers = new Dictionary<string, string>
        {
            { "Client-Id", clientId },
        };
        var json = await server.PostAsync(url, headers, payload, new CookieContainer());
        dynamic? d = JsonConvert.DeserializeObject(json);
        if (d is null)
        {
            return null;
        }
        var id = (string)d.data.user.id;
        return new ChannelId(id);
    }
    public static async Task<List<SubscriptionProduct>> GetSubscriptionProductsAsync(IDataServer server, ChannelId channelId)
    {
        var payload = $"{{\"operationName\":\"ChannelProductsWithCommunityGiftOffers\",\"variables\":{{\"channelID\":\"{channelId.Raw}\"}},\"extensions\":{{\"persistedQuery\":{{\"version\":1,\"sha256Hash\":\"403955b53340517f2a7f219b6bca2b559ad1995bfc98976ed5edc7b19203d774\"}}}}}}";
        var url = "https://gql.twitch.tv/gql";
        var headers = new Dictionary<string, string>
        {
            { "Client-Id", clientId },
        };
        var json = await server.PostAsync(url, headers, payload, new CookieContainer());
        dynamic? d = JsonConvert.DeserializeObject(json);
        if (d is null)
        {
            return new List<SubscriptionProduct>();
        }
        var list = new List<SubscriptionProduct>();
        foreach (var product in d.data.user.subscriptionProducts)
        {
            list.Add(SubscriptionProduct.Parse(product));
        }
        return list;
    }
    public static async Task<LastBroadcast?> GetLastBroadcastAsync(IDataServer server, ChannelLogin login)
    {
        var payload = $"{{\"operationName\":\"UseLiveBroadcast\",\"variables\":{{\"channelLogin\":\"{login.Raw}\"}},\"extensions\":{{\"persistedQuery\":{{\"version\":1,\"sha256Hash\":\"0b47cc6d8c182acd2e78b81c8ba5414a5a38057f2089b1bbcfa6046aae248bd2\"}}}}}}";
        var url = "https://gql.twitch.tv/gql";
        var headers = new Dictionary<string, string>
        {
            { "Client-Id", clientId },
        };
        var json = await server.PostAsync(url, headers, payload, new CookieContainer());
        dynamic? d = JsonConvert.DeserializeObject(json);
        if (d is null)
        {
            return null;
        }
        return LastBroadcast.Parse(d);
    }
    public static async Task<List<object>> GetCelebrationEmotesAsync(IDataServer server, ChannelId channelId)
    {
        var payload = $"{{\"operationName\":\"CelebrationEmotes\",\"variables\":{{\"channelID\":\"{channelId.Raw}\"}},\"extensions\":{{\"persistedQuery\":{{\"version\":1,\"sha256Hash\":\"2add4bd682371bfb75aa347ad39ae3ba8a168a1cfff03e0867b62582e3ab6786\"}}}}}}";
        var url = "https://gql.twitch.tv/gql";
        var headers = new Dictionary<string, string>
        {
            { "Client-Id", clientId },
        };
        var json = await server.PostAsync(url, headers, payload, new CookieContainer());
        dynamic? d = JsonConvert.DeserializeObject(json);
        if (d is null)
        {
            return new List<object>();
        }
        //return CelebrationEmotes. new List<object>();
        throw new NotImplementedException();
    }
    public static async Task GetViewCount(IDataServer server, ChannelLogin login)
    {
        var payload = $"{{\"operationName\":\"UseViewCount\",\"variables\":{{\"channelLogin\":\"{login.Raw}\"}},\"extensions\":{{\"persistedQuery\":{{\"version\":1,\"sha256Hash\":\"95e6bd7acfbb2f220c17e387805141b77b43b18e5b27b4f702713e9ddbe6b907\"}}}}}}";
        var url = "https://gql.twitch.tv/gql";
        var headers = new Dictionary<string, string>
        {
            { "Client-Id", clientId },
        };
        var json = await server.PostAsync(url, headers, payload, new CookieContainer());
        dynamic? d = JsonConvert.DeserializeObject(json);
        if (d is null)
        {
            return;// new List<object>();
        }
        //return CelebrationEmotes. new List<object>();
        throw new NotImplementedException();

    }
}
class LastBroadcast
{
    public string LastBroadcastId { get; private set; }
    public string LastBroadcastTitle { get; private set; }
    private LastBroadcast(string lastBroadcastId, string lastBroadcastTitle)
    {
        LastBroadcastId = lastBroadcastId;
        LastBroadcastTitle = lastBroadcastTitle;
    }
    public static LastBroadcast Parse(dynamic json)
    {
        var lastBroadcastId = (string)json.data.user.lastBroadcast.id;
        var lastBroadcastTitle = (string)json.data.user.lastBroadcast.title;
        return new LastBroadcast(lastBroadcastId, lastBroadcastTitle);
    }
}
class SubscriptionProduct
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Tier { get; private set; }
    private SubscriptionProduct(string id, string name, string tier)
    {
        Id = id;
        Name = name;
        Tier = tier;
    }
    public static SubscriptionProduct Parse(dynamic json)
    {
        var id = (string)json.id;
        var name = (string)json.name;
        var tier = (string)json.tier;
        if (json.gifting.community != null)
        {

        }
        return new SubscriptionProduct(id, name, tier);
    }
}
/// <summary>
/// チャンネルを一意に識別するための数字の羅列
/// </summary>
class ChannelId(string raw)
{
    public string Raw { get; } = raw;
}
/// <summary>
/// チャンネルを一意に識別するための文字列
/// </summary>
/// <param name="raw"></param>
class ChannelLogin(string raw)
{
    public string Raw { get; } = raw;
}

class Badge
{
    public string Id { get; private set; }
    public string Title { get; private set; }
    public string Image2X { get; private set; }

    public Badge(string id, string title, string image2x)
    {
        Id = id;
        Title = title;
        Image2X = image2x;
    }
    public static Badge Parse(dynamic json)
    {
        var id = (string)json.id;
        var title = (string)json.title;
        var image2x = (string)json.image2x;
        return new Badge(id, title, image2x);
    }
}
static class Badges
{
    public static Dictionary<string, Badge> Parse(string json)
    {
        dynamic? d = JsonConvert.DeserializeObject(json);
        if (d is null)
        {
            return new Dictionary<string, Badge>();
        }
        var dict = new Dictionary<string, Badge>();
        var badges = d.data.badges;
        foreach (var badge in d.data.badges)
        {
            var b = (Badge)Badge.Parse(badge);
            dict.Add(b.Id, b);
        }
        return dict;
    }
}
class MessageFragment
{
    public string Text { get; private set; }
    public Emote? Content { get; private set; }
    private MessageFragment(string text, Emote? content)
    {
        Text = text;
        Content = content;
    }
    public static MessageFragment Parse(dynamic json)
    {
        var text = (string)json.text;
        Emote? content = null;
        if (json.content != null)
        {
            content = Emote.Parse(json.content);
        }
        return new MessageFragment(text, content);
    }
}
class Emote
{
    public string EmoteId { get; private set; }
    public string SetId { get; private set; }
    public string Token { get; private set; }
    private Emote(string emoteId, string setId, string token)
    {
        EmoteId = emoteId;
        SetId = setId;
        Token = token;
    }
    public static Emote Parse(dynamic json)
    {
        var emoteId = (string)json.emoteID;
        var setId = (string)json.setID;
        var token = (string)json.token;
        return new Emote(emoteId, setId, token);
    }
}
class SenderBadge
{
    public string SetId { get; private set; }
    public string Version { get; private set; }
    public string Id { get; private set; }
    public SenderBadge(string setId, string version, string id)
    {
        SetId = setId;
        Version = version;
        Id = id;
    }
    public static SenderBadge Parse(dynamic json)
    {
        var setId = (string)json.setID;
        var version = (string)json.version;
        var id = (string)json.id;
        return new SenderBadge(setId, version, id);
    }

}
class RecentChatMessage
{
    public string Id { get; private set; }
    public string? DeletedAt { get; private set; }
    public string SentAt { get; private set; }
    public string ContentText { get; private set; }
    public List<MessageFragment> ContentFragments { get; private set; }
    public object? ContentParentMessage { get; private set; }
    public object? ContentThreadParentMessage { get; private set; }
    public string SenderId { get; private set; }
    public string SenderLogin { get; private set; }
    public string SenderDisplayName { get; private set; }
    public List<SenderBadge> SenderBadges { get; private set; }
    private RecentChatMessage(string id, string? deletedAt, string sentAt, string contentText, List<MessageFragment> contentFragments, object? contentParentMessage, object? contentThreadParentMessage, string senderId, string senderLogin, string senderDisplayName, List<SenderBadge> senderBadges)
    {
        Id = id;
        DeletedAt = deletedAt;
        SentAt = sentAt;
        ContentText = contentText;
        ContentFragments = contentFragments;
        ContentParentMessage = contentParentMessage;
        ContentThreadParentMessage = contentThreadParentMessage;
        SenderId = senderId;
        SenderLogin = senderLogin;
        SenderDisplayName = senderDisplayName;
        SenderBadges = senderBadges;
    }

    public static RecentChatMessage Parse(dynamic json)
    {
        var id = (string)json.id;
        var deletedAt = (string?)json.deletedAt;
        var sentAt = (string)json.sentAt;
        var contentText = (string)json.content.text;
        var fragments = new List<MessageFragment>();
        foreach (var fragment in json.content.fragments)
        {
            fragments.Add(MessageFragment.Parse(fragment));
        }
        var contentParentMessage = (object?)json.parentMessage;
        var contentThreadParentMessage = (object?)json.threadParentMessage;
        if (contentParentMessage != null)
        {

        }
        if (contentThreadParentMessage != null)
        {

        }
        var senderId = (string)json.sender.id;
        var senderLogin = (string)json.sender.login;
        var senderDisplayName = (string)json.sender.displayName;
        var senderBadges = new List<SenderBadge>();
        foreach (var badge in json.senderBadges)
        {
            senderBadges.Add(SenderBadge.Parse(badge));
        }
        return new RecentChatMessage(id, deletedAt, sentAt, contentText, fragments, contentParentMessage, contentThreadParentMessage, senderId, senderLogin, senderDisplayName, senderBadges);
    }


}
static class RecentChatMesages
{
    public static List<RecentChatMessage> GetRecentChatMessages(string json)
    {
        dynamic? d = JsonConvert.DeserializeObject(json);
        if (d is null)
        {
            return new List<RecentChatMessage>();
        }
        var messages = new List<RecentChatMessage>();
        foreach (var message in d.data.channel.recentChatMessages)
        {
            messages.Add(RecentChatMessage.Parse(message));
        }
        return messages;
    }
}