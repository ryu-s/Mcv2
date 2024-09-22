using System.Collections.Generic;
using Mcv.PluginV2;
using System;
using System.Threading.Tasks;

namespace Mcv.MainViewPlugin;
interface IShareMethods
{
    Task<(bool updateExists, string url, string current, string latest)> CheckIfUpdateExistsAsync();
    MyUser GetUser(string userId);
}
interface IMainViewHostAdapter : IConnectionNameHost, IConnectionViewModelHost, IPluginMenuItemViewModelHost, IShareMethods
{
    event EventHandler<ConnectionAddedEventArgs>? ConnectionAdded;
    event EventHandler<ConnectionRemovedEventArgs>? ConnectionRemoved;
    event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
    event EventHandler<SiteAddedEventArgs>? SiteAdded;
    event EventHandler<BrowserAddedEventArgs>? BrowserAdded;
    event EventHandler<BrowserRemovedEventArgs>? BrowserRemoved;
    event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    event EventHandler<MetadataUpdatedEventArgs>? MetadataUpdated;
    event EventHandler<SelectedSiteChangedEventArgs>? SelectedSiteChanged;
    event EventHandler<UserAddedEventArgs>? UserAdded;
    event EventHandler<UserRemovedEventArgs>? UserRemoved;
    event EventHandler<SuggestToUpdateEventArgs>? SuggestToUpdateEvent;
    event EventHandler<UpdateProgressChangedEventArgs>? UpdateProgressChanged;
    event EventHandler<PluginAddedEventArgs>? PluginAdded;
    IMainViewPluginOptions Options { get; }
    void RemoveConnections(List<ConnectionId> selectedConnections);
    void RequestAddConnection();
    Task<List<(PluginId, IOptionsTabPage)>> RequestSettingsPanels();
    void RequestCloseApp();

    Task<string> GetAppName();
    Task<string> GetVersion();
    Task<string> GetAppSolutionConfiguration();
    void RequestUpdate(string latest, string url);

}
interface IConnectionNameHost
{
    Task<string> GetConnectionName(ConnectionId connId);
    void SetConnectionName(ConnectionId connId, string newConnectionName);
}
interface IConnectionViewModelHost
{
    void RequestChangeConnectionStatus(ConnectionStatusDiff connectionStatusDiff);
    Task SetConnectSite(PluginId selectedSite, ConnectionId connId, string input, BrowserProfileId browserProfileId);
    void SetException(Exception ex);
    Task SetDisconectSiteAsync(PluginId selectedSite, ConnectionId connId);
    void AfterInputChanged(ConnectionId connId, string input);//TODO:戻り値をTaskにする
}
interface IPluginMenuItemViewModelHost
{
    void RequestShowSettingsPanel(PluginId pluginId);
}
