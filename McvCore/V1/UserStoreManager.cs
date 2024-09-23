using Mcv.PluginV2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mcv.Core.V1;

class UserStoreManager : IUserStoreManager
{
    public event EventHandler<McvUser>? UserAdded;
    public McvUser GetUser(PluginId siteType, string userId)
    {
        var userStore = _dict[siteType];
        var user = userStore.GetUser(userId);
        return user;
    }
    public IEnumerable<McvUser> GetAllUsers(PluginId siteType)
    {
        var userStore = _dict[siteType];
        return userStore.GetAllUsers();
    }
    public void Save(PluginId siteType)
    {
        var userStore = _dict[siteType];
        userStore.Save();
    }
    public void Save()
    {
        var ns = _dict.Values.ToArray();
        foreach (var n in ns)
        {
            n.Save();
        }
    }
    public void SetUserStore(PluginId siteType, IUserStore userStore)
    {
        try
        {
            userStore.UserAdded += (s, e) => UserAdded?.Invoke(s, e);
            _dict.TryAdd(siteType, userStore);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
    readonly ConcurrentDictionary<PluginId, IUserStore> _dict = new();
}
