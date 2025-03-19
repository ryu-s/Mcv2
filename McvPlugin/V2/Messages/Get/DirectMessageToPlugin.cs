namespace Mcv.PluginV2.Messages;

/// <summary>
/// プラグインから別のプラグインに直接setメッセージを送る
/// </summary>
/// <param name="Target">対象のプラグイン</param>
/// <param name="Message">メッセージ</param>
public record SetDirectMessage(PluginId Target, ISetMessageToPluginV2 Message) : ISetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"direct_message\",\"target\":\"{Target}\",\"message\":{System.Text.Json.JsonSerializer.Serialize(Message)}}}";
}
/// <summary>
/// プラグインから別のプラグインに直接Getメッセージを送る
/// </summary>
/// <param name="Target">対象のプラグイン</param>
/// <param name="Message">メッセージ</param>
public record GetDirectMessage(PluginId Target, IGetMessageToPluginV2 Message) : IGetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"direct_message\",\"target\":\"{Target}\",\"message\":{System.Text.Json.JsonSerializer.Serialize(Message)}}}";
}
public record ReplyDirectMessage(IReplyMessageToPluginV2 Message) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"direct_message\",\"message\":{System.Text.Json.JsonSerializer.Serialize(Message)}}}";
}
public record ReplyPluginNotfound() : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"plugin_notfound\"}}";
}
