using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Mcv.Core;

/// <summary>
/// Coreのアップデート関連を抽出したクラス
/// </summary>
class Updater
{
    private readonly ICoreLogger _logger;

    public event EventHandler<string>? ProgressChanged;
    public async Task Update(string url, string zipFilePath, string _appDirPath)
    {
        //指定されたzipファイルをダウンロードする。
        RaiseProgressChanged("ファイルをダウンロードします");
        await DownloadFileAsync(url, zipFilePath);

        //uninstall_info.txtに記載されているファイル全てに.oldを付加            
        AppendOldToOldFiles(_appDirPath);

        RaiseProgressChanged("ダウンロードしたファイルを展開します");
        ExtractZipFile(zipFilePath, _appDirPath);
    }
    private async Task DownloadFileAsync(string url, string destPath)
    {
        var client = new HttpClientDownloadWithProgress(url, destPath);
        client.ProgressChanged += HttpClient_ProgressChanged;
        await client.StartDownload();
    }
    private static void ExtractZipFile(string zipFilePath, string appDirPath)
    {
        using var archive = ZipFile.OpenRead(zipFilePath);
        foreach (var entry in archive.Entries)
        {
            var entryPath = Path.Combine(appDirPath, entry.FullName);
            var entryDir = Path.GetDirectoryName(entryPath);
            if (entryDir is not null && !Directory.Exists(entryDir))
            {
                Directory.CreateDirectory(entryDir);
            }

            var entryFn = Path.GetFileName(entryPath);
            if (!string.IsNullOrEmpty(entryFn))
            {
                entry.ExtractToFile(entryPath, true);
            }
        }
    }
    private void RaiseProgressChanged(string s)
    {
        ProgressChanged?.Invoke(this, s);
    }
    private void HttpClient_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        RaiseProgressChanged($"{e.ProgressPercentage}, {e.TotalBytesDownloaded} / {e.TotalFileSize}");
    }
    private void AppendOldToOldFiles(string _appDirPath)
    {
        try
        {
            var list = new List<string>();
            using (var sr = new System.IO.StreamReader(System.IO.Path.Combine(_appDirPath, "uninstall_info.txt")))
            {
                while (!sr.EndOfStream)
                {
                    var filename = sr.ReadLine();
                    if (!string.IsNullOrEmpty(filename))
                        list.Add(filename);
                }
            }
            foreach (var filename in list)
            {
                if (filename.StartsWith("System.IO.Compression"))
                {
                    continue;
                }
                var srcPath = System.IO.Path.Combine(_appDirPath, filename);
                var dstPath = System.IO.Path.Combine(_appDirPath, filename + ".old");
                try
                {
                    if (System.IO.File.Exists(srcPath))
                    {
                        System.IO.File.Delete(dstPath);//If the file to be deleted does not exist, no exception is thrown.
                        System.IO.File.Move(srcPath, dstPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.AddLog(ex, $"src={srcPath}, dst={dstPath}");
                }
            }
        }
        catch (System.IO.FileNotFoundException ex)
        {
            _logger.AddLog(ex);
        }
        catch (Exception ex)
        {
            _logger.AddLog(ex);
        }
    }
    public Updater(ICoreLogger logger)
    {
        _logger = logger;
    }
}
