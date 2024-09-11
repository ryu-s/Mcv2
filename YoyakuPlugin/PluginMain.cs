using Mcv.PluginV2.Messages;
using Mcv.PluginV2;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Threading;
using System.IO;

namespace YoyakuPlugin;

[Export(typeof(IPlugin))]
public class PluginMain : IPlugin
{
    public IPluginHost Host { get; set; } = default!;
    public PluginId Id { get; } = new PluginId(new Guid("8907BEBE-68C9-4D0D-99DE-2D51B35BF704"));
    public string Name { get; } = "予約管理プラグイン";
    public List<string> Roles { get; } = [];


    public async Task<IReplyMessageToPluginV2> RequestMessageAsync(IGetMessageToPluginV2 message)
    {
        switch (message)
        {

            default:
                break;
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
                    var loadedOptions = await Host.RequestMessageAsync(new RequestLoadPluginOptions(Name)) as ReplyPluginOptions;
                    if (loadedOptions is not null && loadedOptions.RawOptions is not null)
                    {
                        _options.Deserialize(loadedOptions.RawOptions);
                    }
                    await Host.SetMessageAsync(new SetPluginHello(Id, Name, Roles));
                }
                break;
            case SetLoaded _:
                {
                    await OnLoadedAsync();
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

    private (string? name, string? text) GetData(ISiteMessage message)
    {
        throw new NotImplementedException();
    }
    public void OnMessageReceived(ISiteMessage message, IMessageMetadata messageMetadata)
    {
        if (!_options.IsEnabled || messageMetadata.IsNgUser || messageMetadata.IsInitialComment)
            return;

        var (name, text) = GetData(message);
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(text))
        {
            _model.SetComment(messageMetadata.User.Id, name, text, messageMetadata.User, messageMetadata.SiteContextGuid);
        }
    }
    private readonly IOptions _options = new DynamicOptions();
    SettingsViewModel _vm;
    private Dispatcher _dispatcher;
    public async Task OnLoadedAsync()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;
        _model = CreateModel();
        _model.UsersListChanged += Model_UsersListChanged;
        _vm = CreateSettingsViewModel();
        await LoadRegisteredUsers();
    }
    private IUser GetUser(Guid guid, string userId)
    {
        throw new NotImplementedException();
    }
    private string GetUsersFilePath()
    {
        return $"{Name}_users.txt";
    }
    private async Task LoadRegisteredUsers()
    {
        var s = await Host.RequestMessageAsync(new RequestLoadPluginOptions(GetUsersFilePath())) as ReplyPluginOptions;
        if (s is null || s.RawOptions is null) return;
        var lines = s.RawOptions.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var arr = line.Split('\t');
            if (arr.Length != 5) continue;
            try
            {
                var userId = arr[0];
                var name = arr[1];
                var date = DateTime.Parse(arr[2]);
                var hasCalled = arr[3] == "True";
                var guid = new Guid(arr[4]);
                var user = GetUser(guid, userId);
                _model.AddUser(new User(user)
                {
                    Date = date,
                    HadCalled = hasCalled,
                    Id = userId,
                    Name = name,
                    SitePluginGuid = guid,
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }

    private async void Model_UsersListChanged(object? sender, EventArgs e)
    {
        var users = _model.RegisteredUsers.ToArray();
        var s = "";
        foreach (var user in users)
        {
            s += $"{user.Id}\t{user.Name}\t{user.Date}\t{user.HadCalled}\t{user.SitePluginGuid}" + Environment.NewLine;
        }

        await Host.SetMessageAsync(new RequestSavePluginOptions(GetUsersFilePath(), s));
    }

    protected virtual SettingsViewModel CreateSettingsViewModel()
    {
        return new SettingsViewModel(_model, _dispatcher);
    }
    protected virtual Model CreateModel()
    {
        return new Model(_options, Host);
    }

    public async Task OnClosingAsync()
    {
        var s = _options.Serialize();
        await Host.SetMessageAsync(new RequestSavePluginOptions(GetUsersFilePath(), s));
    }
    public void Run()
    {
    }

    public void ShowSettingView()
    {
        //var left = Host.MainViewLeft;
        //var top = Host.MainViewTop;
        var view = new SettingsView
        {
            //Left = left,
            //Top = top,
            DataContext = _vm
        };
        view.Show();
    }

    //public string GetSettingsFilePath()
    //{
    //    var dir = Host.SettingsDirPath;
    //    return Path.Combine(dir, $"{Name}.txt");
    //}

    public void OnTopmostChanged(bool isTopmost)
    {
        if (_vm != null)
        {
            _vm.Topmost = isTopmost;
        }
    }
    Model _model;
}

public interface IMessageMetadata
{
    bool IsNgUser { get; }
    bool IsInitialComment { get; }
    IUser User { get; }
    Guid SiteContextGuid { get; }
}