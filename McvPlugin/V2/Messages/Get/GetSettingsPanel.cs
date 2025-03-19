namespace Mcv.PluginV2.Messages;

public record GetSettingsPanel : IGetMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"get\",\"get\":\"settingspanel\"}}";
}
public record AnswerSettingsPanel(IOptionsTabPage Panel) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"settingspanel\",\"panel\":{System.Text.Json.JsonSerializer.Serialize(Panel)}}}";
}
