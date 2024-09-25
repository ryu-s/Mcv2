namespace Mcv.PluginV2.Messages;
public record NotifyDownloadProgress(IUpdateProgressData Progress) : INotifyMessageV2
{
    public string Raw { get; } = "";
}
public interface IUpdateProgressData { }
public record UpdateProgressMessage(string Progress) : IUpdateProgressData;
public class DownloadProgress(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage) : IUpdateProgressData
{
    public long? TotalFileSize { get; } = totalFileSize;
    public long TotalBytesDownloaded { get; } = totalBytesDownloaded;
    public double? ProgressPercentage { get; } = progressPercentage;
}