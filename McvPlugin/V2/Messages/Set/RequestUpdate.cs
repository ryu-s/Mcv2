namespace Mcv.PluginV2.Messages;

public record RequestUpdate(string UpdateTo, string Url) : ISetMessageToCoreV2;
