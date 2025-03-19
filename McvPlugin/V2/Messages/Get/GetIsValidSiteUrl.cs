namespace Mcv.PluginV2.Messages;
public record GetIsValidSiteUrl(string Input) : IGetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"isvalidsiteurl\",\"url\":\"{Input}\"}}";
}
public record ReplyIsValidSiteUrl(bool IsValid) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"isvalidsiteurl\",\"is_valid\":{IsValid.ToString().ToLower()}}}";
}
