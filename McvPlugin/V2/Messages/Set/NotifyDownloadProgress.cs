namespace Mcv.PluginV2.Messages;
public record NotifyDownloadProgress(string Progress) : INotifyMessageV2
{
    public string Raw { get; } = "";
}
