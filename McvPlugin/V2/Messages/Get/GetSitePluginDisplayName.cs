namespace Mcv.PluginV2.Messages;

public record GetSitePluginDisplayName : IGetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"siteplugindisplayname\"}}";
}
public record ReplySitePluginDisplayName(string DisplayName) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"siteplugindisplayname\",\"display_name\":\"{DisplayName}\"}}";
}
