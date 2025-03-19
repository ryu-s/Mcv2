using System.Net;
using System.Text.Json;

namespace Mcv.PluginV2.Messages;
public record GetCookies(BrowserProfileId BrowserProfileId, string Domain) : IGetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"cookies\",\"browser_profile_id\":\"{BrowserProfileId}\",\"domain\":\"{Domain}\"}}";
}
public record ReplyCookies(List<System.Net.Cookie> Cookies) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"cookies\",\"cookies\":{JsonSerializer.Serialize(Cookies)}}}";
}
