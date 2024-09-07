using Akka.Actor;
using Mcv.Core.CoreActorMessages;
using Mcv.Core.PluginActorMessages;
using Mcv.Core.V1;
using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Mcv.Core;

enum LogType
{
    Error,
    Debug,
}
class LogData
{
    public string Message { get; set; }
    public DateTime DateTime { get; set; } = DateTime.Now;
    public LogData(string message)
    {
        Message = message;
    }
}
class Logger
{
    List<LogData> _logs = new();
    public void Log(string message)
    {
        _logs.Add(new LogData(message));
    }
    public void LogException(Exception ex, string message = "", string? detail = null)
    {
        Log($"Exception: {message} {ex.Message} {ex.StackTrace} {detail}");
    }
    public void WriteFile(string filename)
    {
        using (var sw = new StreamWriter(filename, true))
        {
            foreach (var log in _logs)
            {
                sw.WriteLine($"{log.DateTime} {log.Message}");
                sw.WriteLine("=======================");
            }
        }
        _logs.Clear();
    }
}
class McvCoreActor : ReceiveActor
{
    private readonly ConnectionManager _connManager;
    private readonly IActorRef _pluginManager;
    private readonly V1.IUserStoreManager _userStoreManager;
    private static readonly string OptionsPath = Path.Combine("settings", "options.txt");
    private static readonly string MainViewPluginOptionsPath = Path.Combine("settings", "MainViewPlugin.txt");
    private static readonly ILogger _logger = new LoggerTest();
    private IMcvCoreOptions _coreOptions = default!;
    private static readonly Logger _coreLogger = new();
    private void SetMessageToPluginManager(ISetMessageToPluginV2 message)
    {
        _pluginManager.Tell(new SetSetToAllPlugin(message));
    }
    private void SetMessageToPluginManager(PluginId pluginId, ISetMessageToPluginV2 message)
    {
        _pluginManager.Tell(new SetSetToAPlugin(pluginId, message));
    }
    private void SetMessageToPluginManager(INotifyMessageV2 message)
    {
        _pluginManager.Tell(new SetNotifyToAllPlugin(message));
    }
    private void SetMessageToPluginManager(PluginId pluginId, INotifyMessageV2 message)
    {
        _pluginManager.Tell(new SetNotifyToAPlugin(pluginId, message));
    }
    private Task<IReplyMessageToPluginV2> GetMessageToPluginManagerAsync(PluginId pluginId, IGetMessageToPluginV2 message)
    {
        return _pluginManager.Ask<IReplyMessageToPluginV2>(new GetMessage(pluginId, message));
    }
    private Task<List<IPluginInfo>> GetPluginListAsync()
    {
        return _pluginManager.Ask<List<IPluginInfo>>(new GetPluginList(), CancellationToken.None);
    }
    private void RemovePlugin(PluginId pluginId)
    {
        _pluginManager.Tell(new RemovePlugin(pluginId));
    }
    private void SetPluginRole(PluginId pluginId, List<string> pluginRole)
    {
        _pluginManager.Tell(new SetPluginRole(pluginId, pluginRole));
    }
    private void AddPlugins(List<IPlugin> plugins, PluginHost pluginHost)
    {
        _pluginManager.Tell(new AddPlugins(plugins, pluginHost));
    }
    internal void RequestCloseApp()
    {
        SetMessageToPluginManager(new SetClosing());

        _userStoreManager.Save();

        _coreLogger.WriteFile("log.txt");

        //ここで直接Context.System.Terminate()したいけど、できないから自分にメッセージを送る
        _self.Tell(new SystemShutDown());
    }
    private readonly IActorRef _self;
    Thread _splashThread;
    SplashWindow _splashWindow;
    SplashWindowViewModel _splashVm;
    public McvCoreActor()
    {
        //System.Data.SQLite.Coreのバージョンが1.0.118のの時にSingleFileでPublishするとNullReferenceExceptionが発生する。
        //下記コードを追加することで回避できるらしい。
        //https://www.sqlite.org/forum/forumpost/66a0d2716a
        System.Environment.SetEnvironmentVariable("SQLite_NoConfigure", "1");

        _self = Self;
        _appDirPath = AppContext.BaseDirectory;
        var io = new IOTest();
        _pluginManager = Context.ActorOf(PluginManagerActor.Props());

        _connManager = new ConnectionManager();
        _connManager.ConnectionAdded += ConnManager_ConnectionAdded;
        _connManager.ConnectionRemoved += ConnManager_ConnectionRemoved;
        _connManager.ConnectionStatusChanged += ConnManager_ConnectionStatusChanged;

        _userStoreManager = new V1.UserStoreManager();
        _userStoreManager.UserAdded += UserStoreManager_UserAdded;

        _splashVm = new SplashWindowViewModel();


        _splashThread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher)
            );

            var newWindow = new SplashWindow();
            newWindow.Closed += (o, args) =>
            {
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            };
            newWindow.DataContext = _splashVm;
            newWindow.Show();
            Dispatcher.Run();
        });

        _splashThread.SetApartmentState(ApartmentState.STA);
        _splashThread.IsBackground = true;

        _splashThread.Start();


        Receive<Initialize>(_ =>
        {
            Initialize();
        });
        Receive<SystemShutDown>(_ =>
        {
            Context.System.Terminate();
        });
    }

    private void ConnManager_ConnectionRemoved(object? sender, ConnectionRemovedEventArgs e)
    {
        SetMessageToPluginManager(new NotifyConnectionRemoved(e.ConnId));
    }

    private void ConnManager_ConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        SetMessageToPluginManager(new NotifyConnectionStatusChanged(e.ConnStDiff));
    }

    internal async Task SetMessageAsync(ISetMessageToCoreV2 m)
    {
        Debug.WriteLine($"McvCoreActor::SetMessageAsync(): {m}");
        switch (m)
        {
            case SetPluginHello pluginHello:
                {
                    var success = true;
                    try
                    {
                        //Roleを登録した時点でPluginManagerの内部ではそのプラグインが登録され、PluginListで取得できるようになる。
                        var pluginList = await GetPluginListAsync();
                        SetPluginRole(pluginHello.PluginId, pluginHello.PluginRole);
                        if (PluginTypeChecker.IsSitePlugin(pluginHello.PluginRole))
                        {
                            var store = new V1.SQLiteUserStore(GetSettingsFilePath("users_" + pluginHello.PluginName + ".db"), _logger);
                            store.Load();
                            _userStoreManager.SetUserStore(pluginHello.PluginId, store);
                        }
                        else if (PluginTypeChecker.IsBrowserPlugin(pluginHello.PluginRole))
                        {

                        }
                        //新たに追加されたプラグインに対して次の情報を通知する
                        //・読み込み済みのプラグイン
                        //・Connection

                        SetMessageToPluginManager(pluginHello.PluginId, new NotifyPluginInfoList(pluginList.Where(p => p.Id != pluginHello.PluginId).ToList()));

                        SetMessageToPluginManager(pluginHello.PluginId, new NotifyConnectionStatusList(_connManager.GetConnectionStatusList()));
                    }
                    catch (Exception ex)
                    {
                        _coreLogger.LogException(ex);
                        success = false;
                    }
                    if (!success)
                    {
                        //rollback
                        _coreLogger.Log($"PluginAdded failed: {pluginHello.PluginName}");
                        RemovePlugin(pluginHello.PluginId);
                    }
                    if (PluginTypeChecker.IsMainViewPlugin(pluginHello.PluginRole))
                    {
                        //SplashWindowを閉じる
                        SplashWindowViewModel.RequestClose();
                        _coreLogger.Log($"SplashWindow closed");
                    }
                    SetMessageToPluginManager(pluginHello.PluginId, new SetLoaded());
                    _coreLogger.Log($"PluginAdded: {pluginHello.PluginName}");
                    SetMessageToPluginManager(new NotifyPluginAdded(pluginHello.PluginId, pluginHello.PluginName, pluginHello.PluginRole));
                }
                break;
            case SetMetadata metadata:
                {
                    SetMessageToPluginManager(new NotifyMetadataUpdated(metadata.ConnId, metadata.SiteId, metadata.Metadata));
                }
                break;
            case SetMessage setMessage:
                {
                    var userId = setMessage.UserId;
                    if (userId is not null)
                    {
                        var user = _userStoreManager.GetUser(setMessage.SiteId, userId);
                        if (setMessage.Username is not null)
                        {
                            user.Name = setMessage.Username;
                        }
                        if (setMessage.NewNickname is not null)
                        {
                            user.Nickname = setMessage.NewNickname;
                        }

                        //初コメかどうか。McvCoreでやること？
                        _userCommentCountManager.AddCommentCount(setMessage.ConnId, userId);
                        var isFirstComment = _userCommentCountManager.IsFirstComment(setMessage.ConnId, userId);

                        //TODO:NameとNicknameもプラグインに渡したい
                        IEnumerable<IMessagePart>? username = user.Name;// Common.MessagePartFactory.CreateMessageItems("");
                        var nickname = user.Nickname;
                        var isNgUser = user.IsNgUser;
                        SetMessageToPluginManager(new NotifyMessageReceived(setMessage.ConnId, setMessage.SiteId, setMessage.Message, userId, username, nickname, isNgUser, setMessage.IsInitialComment));
                    }
                    else
                    {
                        //InfoMessageとかがUserId==nullになるからこれが必要。
                        //他にも配信サイトのメッセージでもUserIdが無いものもある。
                        SetMessageToPluginManager(new NotifyMessageReceived(setMessage.ConnId, setMessage.SiteId, setMessage.Message, null, null, null, false, setMessage.IsInitialComment));
                    }
                }
                break;
            case RequestChangeConnectionStatus connStDiffMsg:
                ChangeConnectionStatus(connStDiffMsg.ConnStDiff);
                break;
            case RequestAddConnection _:
                _coreLogger.Log("RequestAddConnection");
                await AddConnection();
                break;
            case RequestShowSettingsPanel reqShowSettingsPanel:
                ShowPluginSettingsPanel(reqShowSettingsPanel.PluginId);
                break;
            case RequestRemoveConnection removeConn:
                RemoveConnection(removeConn.ConnId);
                break;
            case RequestSavePluginOptions savePluginOptions:
                SavePluginOptions(savePluginOptions.Filename, savePluginOptions.PluginOptionsRaw);
                break;
            case SetCloseApp _:
                RequestCloseApp();
                break;
            case SetDirectMessage directMsg:
                SetMessageToPluginManager(directMsg.Target, directMsg.Message);
                break;
            case RequestUpdate updateToLatest:
                {
                    await DownloadFileAsync(updateToLatest.Url, _zipFilePath);

                    //list.txtに記載されているファイル全てに.oldを付加            
                    try
                    {
                        var list = new List<string>();
                        using (var sr = new System.IO.StreamReader(System.IO.Path.Combine(_appDirPath, "list.txt")))
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
                                _logger.LogException(ex, "", $"src={srcPath}, dst={dstPath}");
                            }
                        }
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        _logger.LogException(ex);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    try
                    {
                        ExtractZipFile(_zipFilePath, _appDirPath);
                    }
                    catch (Exception ex)
                    {
                        _coreLogger.LogException(ex);
                    }

                    try
                    {
                        var exeFile = Process.GetCurrentProcess().MainModule!.FileName!;
                        var pid = Process.GetCurrentProcess().Id;
                        System.Diagnostics.Process.Start(exeFile, "/updated " + pid);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                        return;
                    }
                    try
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                }
                break;
        }
    }
    private async Task DownloadFileAsync(string url, string destPath)
    {
        //var client = new HttpClient();
        var client = new HttpClientDownloadWithProgress(url, destPath);
        client.ProgressChanged += HttpClient_ProgressChanged;
        await client.StartDownload();
        //var res = await client.GetAsync(url);
        //using var sr = await res.Content.ReadAsStreamAsync();
        //using var fs = new FileStream(destPath, FileMode.Create);
        //await sr.CopyToAsync(fs);
    }
    private void HttpClient_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        var message = new NotifyDownloadProgress($"{e.ProgressPercentage}, {e.TotalBytesDownloaded} / {e.TotalFileSize}");
        SetMessageToPluginManager(message);
    }
    private static void ExtractZipFile(string zipFilePath, string appDirPath)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogException(ex);
        }
    }

    private readonly string _zipFilePath = "latest.zip";
    private readonly string _appDirPath = "";
    internal async Task SetMessageAsync(INotifyMessageV2 message)
    {
        switch (message)
        {
            case NotifySiteConnected connected:
                SetMessageToPluginManager(new NotifyConnectionStatusChanged(new ConnectionStatusDiff(connected.ConnId) { CanConnect = false, CanDisconnect = true }));
                break;
            case NotifySiteDisconnected disconnected:
                SetMessageToPluginManager(new NotifyConnectionStatusChanged(new ConnectionStatusDiff(disconnected.ConnId) { CanConnect = true, CanDisconnect = false }));
                break;
            default:
                break;
        }
        await Task.CompletedTask;
    }
    internal async Task<IReplyMessageToPluginV2> RequestMessageAsync(IGetMessageToCoreV2 message)
    {
        switch (message)
        {
            case RequestLoadPluginOptions reqPluginOptions:
                {
                    var rawOptions = LoadPluginOptionsRaw(reqPluginOptions.PluginName);
                    return new ReplyPluginOptions(rawOptions);
                }
            case GetLegacyOptions _:
                {
                    var rawOptions = LoadLegacyOptionsRaw() ?? "";
                    return new ReplyLegacyOptions(rawOptions);
                }
            case GetConnectionStatus reqConnSt:
                {
                    var connSt = GetConnectionStatus(reqConnSt.ConnId);
                    return new ReplyConnectionStatus(connSt);
                }
            case GetAppName _:
                {
                    var appName = GetAppName();
                    return new ReplyAppName(appName);
                }
            case GetAppVersion _:
                {
                    var appVersion = GetAppVersion();
                    return new ReplyAppVersion(appVersion);
                }
            case GetAppSolutionConfiguration _:
                {
                    var appSolutionConfiguration = GetAppSolutionConfiguration();
                    return new ReplyAppSolutionConfiguration(appSolutionConfiguration);
                }
            case GetUserAgent _:
                {
                    var userAgent = GetUserAgent();
                    return new ReplyUserAgent(userAgent);
                }
            case GetDirectMessage directMsg:
                {
                    return await GetMessageToPluginManagerAsync(directMsg.Target, directMsg.Message);
                }
            case GetPluginSettingsDirPath pluginSettingsDirPath:
                {
                    return new ReplyPluginSettingsDirPath(GetSettingsFilePath(pluginSettingsDirPath.FilePath));
                }
            case GetIfUpdateExists _:
                {
                    string? version = null;
                    string? url = null;
                    try
                    {
                        (version, url) = await GetLatestVersionInfo(GetAppDirName(), GetUserAgent());
                    }
                    catch (Exception ex)
                    {
                        _coreLogger.LogException(ex);
                    }
                    if (version is not null && url is not null)
                    {
                        return new ReplyIfUpdateExists(IsNewer(GetAppVersion(), version), url, GetAppVersion(), version);
                    }
                    else
                    {
                        return new ReplyIfUpdateExistsError();
                    }
                }
        }
        throw new Exception("bug");
    }

    private static bool IsNewer(string current, string target)
    {
        return ToVersion(current) < ToVersion(target);
    }
    private static Version ToVersion(string versionStr)
    {
        //正規表現を使ってバージョン文字列をパースする
        var match = Regex.Match(versionStr, "^(\\d+)\\.(\\d+)\\.(\\d+)");
        if (match.Success)
        {
            return new Version(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));
        }
        throw new NotImplementedException();
    }

    public static async Task<(string version, string url)> GetLatestVersionInfo(string name, string userAgent)
    {
        name = name.ToLower();
        //APIが確定するまでアダプタを置いている。ここから本当のAPIを取得する。
        var permUrl = @"https://ryu-s.github.io/" + name + "_latest";

        using var client = new System.Net.Http.HttpClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
        var api = await client.GetStringAsync(permUrl);
        var jsonStr = await client.GetStringAsync(api);
        dynamic? d = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonStr);
        if (d is null)
        {
            throw new Exception($"json parse error: {jsonStr}");
        }
        if (!d.ContainsKey("version") || !d.ContainsKey("url"))
        {
            throw new Exception($"json parse error: {jsonStr}");
        }
        var version = (string)d.version;
        var url = (string)d.url;
        return (version, url);
    }
    internal void SavePluginOptions(string pluginName, string pluginOptionsRaw)
    {
        try
        {
            var io = new V1.IOTest();
            io.WriteFile(Path.Combine("settings", pluginName + ".txt"), pluginOptionsRaw);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
        }
    }
    internal void ShowPluginSettingsPanel(PluginId pluginId)
    {
        SetMessageToPluginManager(pluginId, new RequestShowSettingsPanelToPlugin());
    }

    private void ConnManager_ConnectionAdded(object? sender, ConnectionAddedEventArgs e)
    {
        SetMessageToPluginManager(new NotifyConnectionAdded(e.ConnSt));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    private static bool CheckIfCanReadWrite()
    {
        var filename = "test.txt";
        using (var sw = new StreamWriter(filename))
        {
            sw.Write("ok");
        }
        using (var sr = new StreamReader(filename))
        {
            var _ = sr.ReadToEnd();
        }
        File.Delete(filename);
        return true;
    }
    private string GetSettingsFilePath(string filename)
    {
        return Path.Combine(_coreOptions.SettingsDirPath, filename);
    }
    internal bool Initialize()
    {
        _splashVm.AddLog("初期化開始");
        try
        {
            _splashVm.AddLog("ファイルの読み書き権限確認");
            var testResult = CheckIfCanReadWrite();
        }
        catch (Exception)
        {
            MessageBox.Show("ファイルの読み書き権限無し", "マルチコメビュ起動エラー");
            return false;
        }
        if (File.Exists(OptionsPath) && !File.Exists(MainViewPluginOptionsPath))
        {
            File.Copy(OptionsPath, MainViewPluginOptionsPath);
        }
        //旧バージョンから移行する処理
        if (File.Exists(Path.Combine(OptionsPath, "users_YouTubeLive.db")))
        {
            File.Move(Path.Combine(OptionsPath, "users_YouTubeLive.db"), Path.Combine(OptionsPath, "users_YouTubeLiveSitePlugin.db"));
        }
        if (File.Exists(Path.Combine(OptionsPath, "users_Twitch.db")))
        {
            File.Move(Path.Combine(OptionsPath, "users_Twitch.db"), Path.Combine(OptionsPath, "users_TwitchSitePlugin.db"));
        }

        _splashVm.AddLog("設定の読み込み");
        _coreOptions = LoadOptions(OptionsPath, _logger);

        _coreOptions.PluginDir = "plugins";

        _splashVm.AddLog("プラグインの読み込み");
        var pluginHost = new PluginHost(this);
        var plugins = PluginLoader.LoadPlugins(_coreOptions.PluginDir);
        AddPlugins(plugins, pluginHost);

        //var options = LoadOptions(GetOptionsPath(), logger);
        //_sitePluginOptions = LoadSitePluginOptions(GetSitePluginOptionsPath(), _logger);
        //_pluginManager.LoadSitePlugins(_sitePluginOptions, _logger, GetUserAgent());
        //foreach (var site in _pluginManager.GetSitePlugins())
        //{
        //    //TODO:DisplayNameでファイル名を付けておきながらSiteTypeで識別している。
        //    var userStore = new V1.SQLiteUserStore(coreOptions.SettingsDirPath + "\\" + "users_" + site.Name + ".db", _logger);
        //    _userStoreManager.SetUserStore(site.SiteId, userStore);
        //}


        //_browserManager.LoadBrowserProfiles();

        return true;
    }
    private void UserStoreManager_UserAdded(object? sender, McvUser e)
    {
    }

    private static IMcvCoreOptions LoadOptions(string optionsPath, ILogger logger)
    {
        var options = new McvCoreOptions();
        try
        {
            var io = new V1.IOTest();
            var s = io.ReadFile(optionsPath);
            options.Deserialize(s);
        }
        catch (Exception ex)
        {
            logger.LogException(ex);
        }
        return options;
    }
    internal async Task AddConnection()
    {
        var defaultSite = await GetDefaultSite();
        if (defaultSite is null)
        {
            _coreLogger.Log("siteが無い");
            var k = await _pluginManager.Ask<List<IPluginInfo>>(new GetPluginList());
            if (k is not null)
            {
                var plugins = k.Where(p => PluginTypeChecker.IsSitePlugin(p.Roles)).ToList().Select(k => k.Name);
                var s = string.Join(",", plugins);
                _coreLogger.Log(s);
            }

            return;
        }
        var connId = _connManager.AddConnection(defaultSite);
        _userCommentCountManager.AddConnection(connId);
    }

    private Task<PluginId> GetDefaultSite()
    {
        return _pluginManager.Ask<PluginId>(new GetDefaultSite());
    }

    private readonly UserCommentCountManager _userCommentCountManager = new();

    internal void RemoveConnection(ConnectionId connId)
    {
        _connManager.RemoveConnection(connId);
    }
    internal static string GetAppVersion()
    {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var ver = asm.GetName().Version!;
        var s = $"{ver.Major}.{ver.Minor}.{ver.Build}";
        return s;
    }
    internal static string GetAppName()
    {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var title = asm.GetName().Name!;
        return title;
    }
    internal static string GetAppSolutionConfiguration()
    {
        string s = "";
#if BETA
        s = "ベータ版";
#elif ALPHA
        s = "アルファ版";
#elif DEBUG
        s = "DEBUG";
#else
        s = "安定版";
#endif
        return s;
    }
    internal static string GetAppDirName()
    {
        var Name = GetAppName();
#if BETA
        return Name + "_Beta";
#elif ALPHA
        return Name + "_Alpha";
#else
        return Name;
#endif

    }
    private static string GetUserAgent()
    {
        return $"{GetAppName()}/{GetAppVersion()} contact-> twitter.com/kv510k";
    }
    private string GetSitePluginOptionsPath()
    {
        return Path.Combine(_appDirPath, "settings", "options.txt");
    }

    internal void ChangeConnectionStatus(IConnectionStatusDiff connStDiff)
    {
        var connId = connStDiff.Id;
        if (connStDiff.SelectedSite is not null)
        {
            var before = _connManager.GetConnectionStatus(connId).SelectedSite;
            var after = connStDiff.SelectedSite;
            SetMessageToPluginManager(before, new SetDestroyCommentProvider(connId));
            SetMessageToPluginManager(after, new SetCreateCommentProvider(connId));
        }
        _connManager.ChangeConnectionStatus(connStDiff);
    }
    internal static string? LoadLegacyOptionsRaw()
    {
        var optionsPath = Path.Combine("settings", "options.txt");
        var io = new V1.IOTest();
        var s = io.ReadFile(optionsPath);
        return s;
    }
    internal static string? LoadPluginOptionsRaw(string pluginName)
    {
        var optionsPath = Path.Combine("settings", pluginName + ".txt");
        var io = new V1.IOTest();
        var s = io.ReadFile(optionsPath);
        return s;
    }

    internal IConnectionStatus GetConnectionStatus(ConnectionId connId)
    {
        return _connManager.GetConnectionStatus(connId);
    }

    internal async Task SetMessageAsync(PluginId target, ISetMessageToPluginV2 message)
    {
        SetMessageToPluginManager(target, message);
        await Task.CompletedTask;
    }
    public static Props Props()
    {
        return Akka.Actor.Props.Create(() => new McvCoreActor()).WithDispatcher("akka.actor.synchronized-dispatcher");
    }
}
