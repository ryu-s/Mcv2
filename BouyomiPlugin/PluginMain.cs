using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace BouyomiPlugin;

[Export(typeof(IPlugin))]
public class PluginMain : IPlugin
{
    public IPluginHost Host { get; set; } = default!;
    public PluginId Id { get; } = new PluginId(new Guid("B03801D1-D380-45BA-8C46-67BB3251BE3B"));
    public string Name { get; } = "棒読みちゃん連携";
    public List<string> Roles { get; } = [];

    ITalker? _talker;
    private readonly Options _options = new();
    Process? _bouyomiChanProcess;
    public async Task<IReplyMessageToPluginV2> RequestMessageAsync(IGetMessageToPluginV2 message)
    {
        switch (message)
        {
        }
        await Task.CompletedTask;
        throw new Exception("bug");
    }

    public async Task SetMessageAsync(ISetMessageToPluginV2 message)
    {
        switch (message)
        {
            case SetLoading _:
                {
                    _options.Set(await LoadOptions());
                    _talker = new TcpTalker("127.0.0.1", 50001);
                    await Host.SetMessageAsync(new SetPluginHello(Id, Name, Roles));
                }
                break;
            case SetLoaded _:
                {
                    try
                    {
                        if (_options.IsExecBouyomiChanAtBoot && !IsExecutingProcess("BouyomiChan"))
                        {
                            StartBouyomiChan();
                        }
                    }
                    catch (Exception) { }
                }
                break;
            case SetClosing _:
                {
                    await Host.SetMessageAsync(new RequestSavePluginOptions(Name, _options.Serialize()));
                    if (_bouyomiChanProcess != null && _options.IsKillBouyomiChan)
                    {
                        try
                        {
                            _bouyomiChanProcess.Kill();
                        }
                        catch (Exception) { }
                    }
                }
                break;
            case RequestShowSettingsPanelToPlugin _:
                ShowSettingView();
                break;
        }
    }
    private readonly List<ISiteMessageConverter> _siteMessageConverters = new List<ISiteMessageConverter>
    {
        new TwitchMessageConverter(),
        new NicoMessageConverter(),
        new TwicasMessageConverter(),
        new LineLiveMessageConverter(),
        new WhowatchMessageConverter(),
        new MirrativMessageConverter(),
        new ShowRoomMessageConverter(),
        new BigoMessageConverter(),
        new MixchMessageConverter(),
        new OpenrecMessageConverter(),
        new YouTubeLiveMessageConverter()
    };
    private void OnNotifyMessageReceived(NotifyMessageReceived msgReceived)
    {
        if (_talker is null || !_options.IsEnabled)
        {
            return;
        }
        string? name = null;
        string? comment = null;
        foreach (var site in _siteMessageConverters)
        {
            bool success;
            (success, name, comment) = site.Convert(msgReceived.Message, _options);
            if (success)
            {
                break;
            }
        }
        //nameがnullでは無い場合かつUser.Nicknameがある場合はNicknameを採用
        //2024/09/09 なんでnameがnullだとダメなんだっけ？
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(msgReceived.Nickname))
        {
            name = msgReceived.Nickname;
        }

        var dataToRead = "";//棒読みちゃんに読んでもらう文字列
        if (_options.IsReadHandleName && !string.IsNullOrEmpty(name))
        {
            dataToRead += name;

            if (_options.IsAppendNickTitle)
            {
                dataToRead += _options.NickTitle;
            }
        }
        if (_options.IsReadComment && !string.IsNullOrEmpty(comment))
        {
            if (!string.IsNullOrEmpty(dataToRead))//空欄で無い場合、つまり名前も読み上げる場合は名前とコメントの間にスペースを入れる。こうすると名前とコメントの間で一呼吸入れてくれる
            {
                dataToRead += " ";
            }
            dataToRead += comment;
        }
        if (string.IsNullOrEmpty(dataToRead))
        {
            return;
        }
        try
        {
            _talker.TalkText(dataToRead);
        }
        catch (TalkException)
        {
            //多分棒読みちゃんが起動していない。
            if (_bouyomiChanProcess == null && System.IO.File.Exists(_options.BouyomiChanPath))
            {
                _bouyomiChanProcess = Process.Start(_options.BouyomiChanPath);
                _bouyomiChanProcess.EnableRaisingEvents = true;
                _bouyomiChanProcess.Exited += (s, e) =>
                {
                    try
                    {
                        _bouyomiChanProcess?.Close();//2018/03/25ここで_bouyomiChanProcessがnullになる場合があった
                    }
                    catch { }
                    _bouyomiChanProcess = null;
                };
            }
            //起動するまでの間にコメントが投稿されたらここに来てしまうが諦める。
        }
    }
    public async Task SetMessageAsync(INotifyMessageV2 message)
    {
        switch (message)
        {
            case NotifyMessageReceived msgReceived:
                OnNotifyMessageReceived(msgReceived);
                break;
        }
        await Task.CompletedTask;
    }
    ConfigView? _settingsView;
    public void ShowSettingView()
    {
        if (_settingsView == null)
        {
            _settingsView = new ConfigView
            {
                DataContext = new ConfigViewModel(_options)
            };
        }
        //_settingsView.Topmost = Host.IsTopmost;
        //_settingsView.Left = Host.MainViewLeft;
        //_settingsView.Top = Host.MainViewTop;

        _settingsView.Show();
    }
    private async Task<Options> LoadOptions()
    {
        var loadedOptions = await Host.RequestMessageAsync(new RequestLoadPluginOptions(Name)) as ReplyPluginOptions;

        var options = new Options();
        options.Deserialize(loadedOptions?.RawOptions);
        return options;
    }
    /// <summary>
    /// 指定したプロセス名を持つプロセスが起動中か
    /// </summary>
    /// <param name="processName">プロセス名</param>
    /// <returns></returns>
    private static bool IsExecutingProcess(string processName)
    {
        return Process.GetProcessesByName(processName).Length > 0;
    }
    private void StartBouyomiChan()
    {
        if (_bouyomiChanProcess == null && System.IO.File.Exists(_options.BouyomiChanPath))
        {
            _bouyomiChanProcess = Process.Start(_options.BouyomiChanPath);
            _bouyomiChanProcess.EnableRaisingEvents = true;
            _bouyomiChanProcess.Exited += BouyomiChanProcess_Exited;
        }
    }

    private void BouyomiChanProcess_Exited(object? sender, EventArgs e)
    {
        try
        {
            _bouyomiChanProcess?.Close();//2018/03/25ここで_bouyomiChanProcessがnullになる場合があった
        }
        catch { }
        _bouyomiChanProcess = null;
    }
}
