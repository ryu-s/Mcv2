namespace Mcv.PluginV2.Messages;
public record NotifyDownloadProgress(string Progress) : ISetMessageToPluginV2;
