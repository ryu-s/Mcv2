namespace Mcv.PluginV2.Messages;

public record SetPluginHello(PluginId PluginId, string PluginName, List<string> PluginRole) : ISetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"plugin_hello\",\"plugin_id\":\"{PluginId}\",\"plugin_name\":\"{PluginName}\",\"plugin_role\":{System.Text.Json.JsonSerializer.Serialize(PluginRole)}}}";
}

public record NotifyPluginAdded(PluginId PluginId, string PluginName, List<string> PluginRole) : INotifyMessageV2
{
    public string Raw => $"{{\"type\":\"notify\",\"notify\":\"plugin_added\",\"plugin_id\":\"{PluginId}\",\"plugin_name\":\"{PluginName}\",\"plugin_role\":{System.Text.Json.JsonSerializer.Serialize(PluginRole)}}}";
}
