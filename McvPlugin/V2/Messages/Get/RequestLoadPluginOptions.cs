namespace Mcv.PluginV2.Messages;
public record RequestLoadPluginOptions(string PluginName) : IGetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"loadpluginoptions\",\"plugin_name\":\"{PluginName}\"}}";
}
public record ReplyPluginOptions(string? RawOptions) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"pluginoptions\",\"raw_options\":{(RawOptions == null ? "null" : $"{RawOptions}")}}}";
}
