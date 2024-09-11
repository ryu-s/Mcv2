using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace CommentGeneratorPlugin;

public interface IUser
{
    string Nickname { get; }
}
public interface IMessageMetadata
{
    bool IsNgUser { get; }
    bool IsInitialComment { get; }
    IUser User { get; }
}
[Export(typeof(IPlugin))]
public class PluginMain : IPlugin
{
    public IPluginHost Host { get; set; } = default!;
    public PluginId Id { get; } = new PluginId(new Guid("7D00F365-630A-4BE2-88F7-11AE8FC8016A"));
    public string Name { get; } = "コメジェネ連携";
    public List<string> Roles { get; } = [];
    private readonly Options _options = new();
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
                    await Host.SetMessageAsync(new SetPluginHello(Id, Name, Roles));
                }
                break;
            case SetLoaded _:
                {
                    OnLoaded();
                }
                break;
            case SetClosing _:
                {
                    await OnClosingAsync();
                }
                break;
            case RequestShowSettingsPanelToPlugin _:
                ShowSettingView();
                break;
        }
    }
    public async Task SetMessageAsync(INotifyMessageV2 message)
    {
        switch (message)
        {
            case NotifyMessageReceived msgReceived:
                break;
        }
        await Task.CompletedTask;
    }
    private async Task<Options> LoadOptions()
    {
        var loadedOptions = await Host.RequestMessageAsync(new RequestLoadPluginOptions(Name)) as ReplyPluginOptions;

        var options = new Options();
        options.Deserialize(loadedOptions?.RawOptions);
        return options;
    }

    private System.Timers.Timer? _writeTimer;
    private System.Timers.Timer? _deleteTimer;
    private SynchronizedCollection<Data> _commentCollection = new SynchronizedCollection<Data>();

    protected virtual string CommentXmlPath { get; private set; }
    record class Data(string? Comment, string? Nickname, string SiteName);
    public void OnMessageReceived(ISiteMessage message, IMessageMetadata messageMetadata)
    {
        //if (!(message is IMessageComment comment)) return;
        if (!_options.IsEnabled || messageMetadata.IsNgUser || messageMetadata.IsInitialComment)
            return;

        //各サイトのサービス名
        //YouTubeLive:youtubelive
        //ニコ生:nicolive
        //Twitch:twitch
        //Twicas:twicas
        //ふわっち:whowatch
        //OPENREC:openrec
        //Mirrativ:mirrativ
        //LINELIVE:linelive
        //Periscope:periscope
        //Mixer:mixer


        if (message is MirrativSitePlugin.IMirrativJoinRoom && !_options.IsMirrativeJoin)
        {
            return;
        }
        //string name;
        //if (HasNickname(messageMetadata.User))
        //{
        //    name = messageMetadata.User.Nickname;
        //}
        //else
        //{
        //    name = comment.NameItems.ToText();
        //}
        var siteName = Tools.GetSiteName(message);
        var (name, comment) = Tools.GetData(message);
        if (HasNickname(messageMetadata.User))
        {
            name = messageMetadata.User.Nickname;
        }
        //var data = new Data
        //{
        //    Comment = comment.CommentItems.ToText(),
        //    Nickname = name,
        //    SiteName = siteName,
        //};
        var data = new Data(comment, name, siteName);
        _commentCollection.Add(data);
    }

    private static bool HasNickname(IUser user)
    {
        return user != null && !string.IsNullOrEmpty(user.Nickname);
    }

    public virtual void OnLoaded()
    {
        _writeTimer = new System.Timers.Timer
        {
            Interval = 500
        };
        _writeTimer.Elapsed += WriteTimer_Elapsed;
        _writeTimer.Start();

        _deleteTimer = new System.Timers.Timer
        {
            Interval = 5 * 60 * 1000
        };
        _deleteTimer.Elapsed += DeleteTimer_Elapsed;
        _deleteTimer.Start();
    }

    private readonly object _xmlWriteLockObj = new object();
    /// <summary>
    /// XMLファイルに書き込む
    /// </summary>
    /// <param name="xmlRootElement"></param>
    /// <param name="path"></param>
    private void WriteXml(XElement xmlRootElement, string path)
    {
        lock (_xmlWriteLockObj)
        {
            xmlRootElement.Save(path);
        }
    }
    private void DeleteTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (!_options.IsEnabled)
            return;

        //comment.xmlの要素を定期的に削除する
        XElement xml;
        try
        {
            if (!File.Exists(CommentXmlPath))
                return;
            xml = XElement.Load(CommentXmlPath);
            var arr = xml.Elements().ToArray();
            var count = arr.Length;
            if (count > 1000)
            {
                //1000件以上だったら、最後の100件以外を全て削除
                xml.RemoveAll();
                for (int i = count - 100; i < count; i++)
                {
                    xml.Add(arr[i]);
                }
                WriteXml(xml, CommentXmlPath);
            }
        }
        catch (IOException ex)
        {
            //being used in another process
            Debug.WriteLine(ex.Message);
            return;
        }
    }

    protected virtual string GetHcgPath(string hcgSettingsFilePath)
    {
        string settingXml;
        using (var sr = new StreamReader(hcgSettingsFilePath))
        {
            settingXml = sr.ReadToEnd();
        }
        var xmlParser = DynamicXmlParser.Parse(settingXml);
        if (!xmlParser.HasElement("HcgPath"))
            return "";
        return xmlParser.HcgPath;
    }
    private void WriteTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        //定期的にcomment.xmlに書き込む。

        Write();
    }
    protected virtual bool IsHcgSettingFileExists()
    {
        return File.Exists(_options.HcgSettingFilePath);
    }
    /// <summary>
    /// _commentCollectionの内容をファイルに書き出す
    /// </summary>
    public void Write()
    {
        if (!_options.IsEnabled || _commentCollection.Count == 0)
            return;

        //TODO:各ファイルが存在しなかった時のエラー表示
        if (string.IsNullOrEmpty(CommentXmlPath) && IsHcgSettingFileExists())
        {
            var hcgPath = GetHcgPath(_options.HcgSettingFilePath);
            CommentXmlPath = hcgPath + "\\comment.xml";
            //TODO:パスがxmlファイルで無かった場合の対応。ディレクトリの可能性も。
        }
        if (!File.Exists(CommentXmlPath))
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("log");

            doc.AppendChild(root);
            doc.Save(CommentXmlPath);
        }
        XElement xml;
        try
        {
            xml = XElement.Load(CommentXmlPath);
        }
        catch (IOException ex)
        {
            //being used in another process
            Debug.WriteLine(ex.Message);
            return;
        }
        catch (XmlException)
        {
            //Root element is missing.
            xml = new XElement("log");
        }
        lock (_lockObj)
        {
            var arr = _commentCollection.ToArray();

            foreach (var data in arr)
            {
                var item = new XElement("comment", data.Comment);
                item.SetAttributeValue("no", "0");
                item.SetAttributeValue("time", ToUnixTime(GetCurrentDateTime()));
                item.SetAttributeValue("owner", 0);
                item.SetAttributeValue("service", data.SiteName);
                //2019/08/25 コメジェネの仕様で、handleタグが無いと"0コメ"に置換されてしまう。だから空欄でも良いからhandleタグは必須。
                var handle = string.IsNullOrEmpty(data.Nickname) ? "" : data.Nickname;
                item.SetAttributeValue("handle", handle);
                xml.Add(item);
            }
            try
            {
                WriteXml(xml, CommentXmlPath);
            }
            catch (IOException ex)
            {
                //コメントの流れが早すぎるとused in another processが来てしまう。
                //この場合、コメントが書き込まれずに消されてしまう。
                Debug.WriteLine(ex.Message);
                return;
            }
            _commentCollection.Clear();
        }
    }
    private static readonly object _lockObj = new object();
    protected virtual DateTime GetCurrentDateTime()
    {
        return DateTime.Now;
    }

    public static long ToUnixTime(DateTime dateTime)
    {
        // 時刻をUTCに変換
        dateTime = dateTime.ToUniversalTime();

        // unix epochからの経過秒数を求める
        return (long)dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
    public async Task OnClosingAsync()
    {
        _settingsView?.ForceClose();
        _writeTimer?.Stop();
        _deleteTimer?.Stop();
        await Host.SetMessageAsync(new RequestSavePluginOptions(Name, _options.Serialize()));
    }
    SettingsView? _settingsView;
    public void ShowSettingView()
    {
        //var left = Host.MainViewLeft;
        //var top = Host.MainViewTop;
        if (_settingsView == null)
        {
            _settingsView = new SettingsView
            {
                DataContext = new ConfigViewModel(_options)
            };
        }
        //_settingsView.Topmost = Host.IsTopmost;
        //_settingsView.Left = left;
        //_settingsView.Top = top;
        _settingsView.Show();
    }

    //        public string GetSettingsFilePath()
    //        {
    //            var dir = Host.SettingsDirPath;
    //            return Path.Combine(dir, $"{Name}.xml");
    //        }
    //        public CommentGeneratorPlugin()
    //        {

    //        }
    //        public void Dispose()
    //        {
    //            Dispose(true);
    //        }
    //        private bool _disposedValue = false;
    //        protected virtual void Dispose(bool disposing)
    //        {
    //            if (!_disposedValue)
    //            {
    //                if (disposing)
    //                {
    //                    _writeTimer.Dispose();
    //                    _deleteTimer.Dispose();
    //                }
    //                _disposedValue = true;
    //            }
    //        }

    //        public void OnTopmostChanged(bool isTopmost)
    //        {
    //            if (_settingsView != null)
    //            {
    //                _settingsView.Topmost = isTopmost;
    //            }
    //        }
}