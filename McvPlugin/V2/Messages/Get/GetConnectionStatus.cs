namespace Mcv.PluginV2.Messages;

public record GetConnectionStatus(ConnectionId ConnId) : IGetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"connectionstatus\",\"conn_id\":\"{ConnId}\"}}";
}
public record ReplyConnectionStatus(IConnectionStatus ConnSt) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"connectionstatus\",\"connection_status\":{System.Text.Json.JsonSerializer.Serialize(ConnSt)}}}";
}
