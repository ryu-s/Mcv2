namespace Mcv.PluginV2.Messages;

public record SetCreateCommentProvider(ConnectionId ConnId) : ISetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"createcommentprovider\",\"conn_id\":\"{ConnId}\"}}";
}
public record SetDestroyCommentProvider(ConnectionId ConnId) : ISetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"destroycommentprovider\",\"conn_id\":\"{ConnId}\"}}";
}
