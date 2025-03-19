using System.Net;

namespace Mcv.PluginV2.Messages;

public record SetConnectSite(ConnectionId ConnId, string Input, List<Cookie> Cookies) : ISetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"connectsite\",\"conn_id\":\"{ConnId}\",\"input\":\"{Input}\",\"cookies\":{System.Text.Json.JsonSerializer.Serialize(Cookies)}}}";
}
public record NotifySiteConnected(ConnectionId ConnId) : INotifyMessageV2
{
    public string Raw => $"{{\"type\":\"notify\",\"notify\":\"site_connected\",\"conn_id\":\"{ConnId}\"}}";
}
