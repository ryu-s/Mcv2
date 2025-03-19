namespace Mcv.PluginV2.Messages;

public record GetUserAgent : IGetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"useragent\"}}";
}
public record ReplyUserAgent(string UserAgent) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"useragent\",\"useragent\":\"{UserAgent}\"}}";
}
