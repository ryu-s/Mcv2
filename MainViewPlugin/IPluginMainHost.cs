using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using System.Threading.Tasks;

namespace Mcv.MainViewPlugin;

interface IPluginMainHost : IShareMethods
{
    void OnConnectionAdded(IConnectionStatus connSt);
    void OnConnectionRemoved(ConnectionId connId);
    void OnConnectionStatusChanged(IConnectionStatusDiff connStDiff);
    void OnMessageReceived(NotifyMessageReceived messageReceived, MyUser? user);
    void OnPluginAdded(IPluginInfo pluginInfo);
    void OnPluginAdded(PluginId pluginId, string pluginName);
    void OnMetadataUpdated(NotifyMetadataUpdated metadataUpdated);
    Task<string> GetSitePluginDisplayName(PluginId pluginId);
    void OnSiteAdded(PluginId siteId, string siteDisplayName);
    void AddBrowserProfile(ProfileInfo browserProfileInfo);
    void AddEmptyBrowserProfile();
    Task LoadUserStoreAsync();
    void SuggestToUpdate(string url, string current, string latest);
    Task SaveUserStoreAsync();
}
