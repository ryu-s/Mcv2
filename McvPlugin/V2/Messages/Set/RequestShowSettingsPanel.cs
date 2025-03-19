namespace Mcv.PluginV2.Messages;

public record RequestShowSettingsPanel(PluginId PluginId) : ISetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"showsettingspanel\",\"plugin_id\":\"{PluginId}\"}}";
}
public class RequestShowSettingsPanelToPlugin : ISetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"showsettingspanel\"}}";
}
