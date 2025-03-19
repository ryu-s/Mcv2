namespace Mcv.PluginV2.Messages;

public record GetBrowserProfiles : IGetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"browserprofiles\"}}";
}
public record ReplyBrowserProfiles(IList<ProfileInfo> Profiles) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"browserprofiles\",\"profiles\":{System.Text.Json.JsonSerializer.Serialize(Profiles)}}}";
}
public record ProfileInfo(PluginId PluginId, string BrowserName, string? ProfileName, BrowserProfileId ProfileId);
