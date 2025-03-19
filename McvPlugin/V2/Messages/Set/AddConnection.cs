namespace Mcv.PluginV2.Messages;

public class RequestAddConnection : ISetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"add_connection\"}}";
}

public record NotifyConnectionAdded(IConnectionStatus ConnSt) : INotifyMessageV2
{
    public string Raw => $"{{\"type\":\"notify\",\"notify\":\"connection_added\",\"connection_status\":{System.Text.Json.JsonSerializer.Serialize(ConnSt)}}}";
}