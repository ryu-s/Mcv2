namespace Mcv.PluginV2.Messages;

public record GetLegacyOptions : IGetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"legacyoptions\"}}";
}
public record ReplyLegacyOptions(string RawOptions) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"legacyoptions\",\"raw_options\":{RawOptions}}}";
}
