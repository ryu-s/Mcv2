namespace Mcv.PluginV2.Messages;

public record RequestSavePluginOptions(string Filename, string PluginOptionsRaw) : ISetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"savepluginoptions\",\"filename\":\"{Filename}\",\"plugin_options_raw\":{PluginOptionsRaw}}}";
}
