namespace Mcv.PluginV2.Messages;

public record GetPluginSettingsDirPath(string FilePath) : IGetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"pluginsettingsdirpath\",\"filepath\":\"{FilePath}\"}}";
}
public record ReplyPluginSettingsDirPath(string PluginSettingsDirPath) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"pluginsettingsdirpath\",\"pluginsettingsdirpath\":\"{PluginSettingsDirPath}\"}}";
}
