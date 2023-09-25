namespace Mcv.PluginV2.Messages;
public record GetIfUpdateExists : IGetMessageToCoreV2;
public record ReplyIfUpdateExists(bool UpdateExists, string Url, string Current, string Latest) : IReplyMessageToPluginV2
{
    public string Raw => $"";
}
