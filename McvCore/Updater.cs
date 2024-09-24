using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Mcv.Core;

/// <summary>
/// Coreのアップデート関連を抽出したクラス
/// </summary>
class Updater
{
    private readonly ICoreLogger _logger;

    public event EventHandler<string>? ProgressChanged;
    public async Task<bool> Update(string url, string zipFilePath, string _appDirPath)
    {
        //"System.IO.Compression"から始まるzip関連のdllだけ残して他は全て削除してからzipファイルを展開しようとすると"System.IO.Compression, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"が存在しないと言われてうまくいかなかった。

        const string tempDir = "latest";
        var tempDirPath = Path.Combine(_appDirPath, tempDir);

        //指定されたzipファイルをダウンロードする。
        RaiseProgressChanged("ファイルをダウンロードします");
        try
        {
            await DownloadFileAsync(url, zipFilePath);
        }
        catch (Exception ex)
        {
            _logger.AddLog(ex);
            return false;
        }

        RaiseProgressChanged("ダウンロードしたファイルを展開します");
        try
        {
            ExtractZipFile(zipFilePath, tempDirPath);
        }
        catch (Exception ex)
        {
            _logger.AddLog(ex);
            RaiseProgressChanged("ダウンロードしたファイルの展開に失敗しました");
            return false;
        }

        //uninstall_info.txtに記載されているファイル全てに.oldを付加
        try
        {
            AppendOldToOldFiles(_appDirPath);
        }
        catch (Exception ex)
        {
            _logger.AddLog(ex);
            RaiseProgressChanged("アップデートの準備中に不具合が発生しました");
            return false;
        }

        try
        {
            foreach (var srcPath in Directory.GetFiles(tempDirPath, "*", SearchOption.AllDirectories))
            {
                var srcFilename = Path.GetFileName(srcPath);
                var relativePath = Path.GetRelativePath(tempDirPath, srcPath);
                var dstFilePath = Path.Combine(_appDirPath, relativePath);
                var dstFileDirPath = Path.GetDirectoryName(dstFilePath)!;
                //ディレクトリが存在しない場合は作成する
                Directory.CreateDirectory(dstFileDirPath);
                File.Copy(srcPath, dstFilePath, true);
            }
        }
        catch (Exception ex)
        {
            _logger.AddLog(ex);
            RaiseProgressChanged("アップデートの準備中に不具合が発生しました");
            return false;
        }

        //本当はここで.oldファイルを削除したいが、AccessDeniedが発生してしまうため諦めて起動時にやる。

        RaiseProgressChanged("アップデートの準備が整いました");
        return true;
    }
    private async Task DownloadFileAsync(string url, string destPath)
    {
        var client = new HttpClientDownloadWithProgress(url, destPath);
        client.ProgressChanged += HttpClient_ProgressChanged;
        try
        {
            await client.StartDownload();
        }
        finally
        {
            client.ProgressChanged -= HttpClient_ProgressChanged;
        }
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
        const string uninstallInfoFileName = "uninstall_info.txt";
        if (!File.Exists(Path.Combine(_appDirPath, uninstallInfoFileName)))
        {
            _logger.AddLog($"{uninstallInfoFileName}が存在しない", LogType.Debug);
            return;
        }
        var list = new List<string>();
        using (var sr = new System.IO.StreamReader(System.IO.Path.Combine(_appDirPath, uninstallInfoFileName)))
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
    public Updater(ICoreLogger logger)
    {
        _logger = logger;
    }
}
