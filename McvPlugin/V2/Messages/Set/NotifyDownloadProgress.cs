namespace Mcv.PluginV2.Messages;

public record NotifyDownloadProgress(IUpdateProgressData Progress) : INotifyMessageV2
{
    public string Raw => $"{{\"type\":\"notify\",\"notify\":\"download_progress\",\"download_progress\":{Progress.Raw}}}";
}

public interface IUpdateProgressData
{
    public string Raw { get; }
}
public record UpdateProgressMessage(string Progress) : IUpdateProgressData
{
    public string Raw => $"{{\"update_progress\":{{\"progress\":\"{Progress}\"}}}}";
}
public class DownloadProgress(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage) : IUpdateProgressData
{
    public long? TotalFileSize { get; } = totalFileSize;
    public long TotalBytesDownloaded { get; } = totalBytesDownloaded;
    public double? ProgressPercentage { get; } = progressPercentage;
    public string Raw
    {
        get
        {
            return $"{{\"download_progress\":{{\"total_file_size\":{TotalFileSize},\"total_bytes_downloaded\":{TotalBytesDownloaded},\"progress_percentage\":{ProgressPercentage}}}}}";
        }
    }
}