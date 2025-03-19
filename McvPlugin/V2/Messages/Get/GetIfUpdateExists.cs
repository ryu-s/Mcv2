namespace Mcv.PluginV2.Messages;

public record GetIfUpdateExists : IGetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"ifupdateexists\"}}";
}

public record ReplyIfUpdateExists(bool UpdateExists, string Url, string Current, string Latest) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"ifupdateexists\",\"update_exists\":{UpdateExists.ToString().ToLower()},\"url\":\"{Url}\",\"current\":\"{Current}\",\"latest\":\"{Latest}\"}}";
}

public record ReplyIfUpdateExistsError() : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"ifupdateexists_error\"}}";
}
