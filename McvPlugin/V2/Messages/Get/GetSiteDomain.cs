namespace Mcv.PluginV2.Messages;

public record GetSiteDomain(ConnectionId ConnectionId) : IGetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"sitedomain\",\"connection_id\":\"{ConnectionId}\"}}";
}
public record ReplySiteDomain(string Domain) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"sitedomain\",\"domain\":\"{Domain}\"}}";
}
