namespace Mcv.PluginV2.Messages;

public record SetDisconnectSite(ConnectionId ConnId) : ISetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"disconnectsite\",\"conn_id\":\"{ConnId}\"}}";
}
public record NotifySiteDisconnected(ConnectionId ConnId) : INotifyMessageV2
{
    public string Raw => $"{{\"type\":\"notify\",\"notify\":\"site_disconnected\",\"conn_id\":\"{ConnId}\"}}";
}
public record SetLoading : ISetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"loading\"}}";
}
public record SetLoaded : ISetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"loaded\"}}";
}
public record SetClosing : ISetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"closing\"}}";
}
