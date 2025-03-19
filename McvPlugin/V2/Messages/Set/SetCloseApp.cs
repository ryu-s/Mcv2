namespace Mcv.PluginV2.Messages;

public record SetCloseApp : ISetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"close_app\"}}";
}
