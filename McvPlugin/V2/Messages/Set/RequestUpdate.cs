namespace Mcv.PluginV2.Messages;

public record RequestUpdate(string UpdateTo, string Url) : ISetMessageToCoreV2
{
    public string Raw => $"{{\"type\":\"set\",\"set\":\"update\",\"update_to\":\"{UpdateTo}\",\"url\":\"{Url}\"}}";
}
