using Akka.Actor;
using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using Mcv.Core.PluginActorMessages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Mcv.Core;
class PrePlguinInfo(PluginId id, string name)
{
    public PluginId Id { get; } = id;
    public string Name { get; } = name;
}
class PluginInfo(PluginId id, string name, List<string> roles) : PrePlguinInfo(id, name), IPluginInfo
{
    public List<string> Roles { get; } = roles;
}
class PluginManagerActor : ReceiveActor
{
    private readonly ConcurrentDictionary<PluginId, IActorRef> _actorDict = new();
    private readonly ConcurrentDictionary<PluginId, IPluginInfo> _pluginInfoDict = new();
    /// <summary>
    /// 
    /// PluginRolesが登録されるまでの間、仮でNameを保持するためのDictionary
    /// </summary>
    private readonly ConcurrentDictionary<PluginId, PrePlguinInfo> _prePluginDict = new();
    public static Props Props()
    {
        return Akka.Actor.Props.Create(() => new PluginManagerActor()).WithDispatcher("akka.actor.synchronized-dispatcher");
    }
    public PluginManagerActor()
    {
        Receive<AddPlugins>(m =>
        {
            foreach (var plugin in m.Plugins)
            {
                AddPlugin(plugin, m.PluginHost);
            }
        });
        Receive<RemovePlugin>(m =>
        {
            RemovePlugin(m.PluginId);
        });
        Receive<SetPluginRole>(m =>
        {
            var id = m.PluginId;
            if (_prePluginDict.TryGetValue(id, out var prePlugin))
            {
                _prePluginDict.TryRemove(id, out var _);
                var pluginInfo = new PluginInfo(prePlugin.Id, prePlugin.Name, m.PluginRole);
                _pluginInfoDict.TryAdd(id, pluginInfo);
            }
        });
        Receive<GetPluginList>(m =>
        {
            Sender.Tell(GetPluginList());
        });
        Receive<SetNotifyToAPlugin>(m =>
        {
            var target = GetPluginActorById(m.PluginId);
            if (target == null)
            {
                return;
            }
            target.Tell(new NotifyMessageV2(m.Message));
        });
        Receive<SetNotifyToAllPlugin>(m =>
        {
            foreach (var target in GetActors())
            {
                target.Tell(new NotifyMessageV2(m.Message));
            }
        });
        Receive<SetSetToAPlugin>(m =>
        {
            var target = GetPluginActorById(m.PluginId);
            if (target == null)
            {
                return;
            }
            target.Tell(new SetMessageToPluginV2(m.Message));
        });
        Receive<SetSetToAllPlugin>(m =>
        {
            foreach (var target in GetActors())
            {
                target.Tell(new SetMessageToPluginV2(m.Message));
            }
        });
        ReceiveAsync<GetMessage>(async m =>
        {
            Sender.Tell(await RequestMessage(m.PluginId, m.Message));
        });
        Receive<GetDefaultSite>(m =>
        {
            Sender.Tell(GetDefaultSite());
        });
    }
    private static IActorRef CreateActor(IPlugin plugin)
    {
        return Context.ActorOf(PluginActor.Props(plugin).WithDispatcher("akka.actor.synchronized-dispatcher"));
    }
    public void AddPlugin(IPlugin plugin, IPluginHost host)
    {
        var actor = CreateActor(plugin);
        _actorDict.TryAdd(plugin.Id, actor);
        _prePluginDict.TryAdd(plugin.Id, new PrePlguinInfo(plugin.Id, plugin.Name));
        plugin.Host = host;
        actor.Tell(new SetMessageToPluginV2(new SetLoading()));
        //Plugin側がHelloメッセージを送ってくるまで登録はしない。
    }
    internal List<IPluginInfo> GetPluginList()
    {
        //このメソッドはPluginManagerの外部から参照できるから、HelloメッセージによってPluginRoleを登録済みのプラグインのみを返す。
        return [.. _pluginInfoDict.Values];
    }
    private IEnumerable<IActorRef> GetActors()
    {
        return _actorDict.Values;
    }
    internal PluginId? GetDefaultSite()
    {
        return _pluginInfoDict.Where(p => PluginTypeChecker.IsSitePlugin(p.Value.Roles)).Select(p => p.Key).FirstOrDefault();
    }
    /// <summary>
    /// 指定したプラグインに対してGetMessageを送る
    /// </summary>
    /// <param name="pluginId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    internal Task<IReplyMessageToPluginV2> RequestMessage(PluginId pluginId, IGetMessageToPluginV2 message)
    {
        var plugin = GetPluginActorById(pluginId);
        if (plugin is null)
        {
            //指定されたpluginIdを持つプラグインが存在しない
            return Task.FromResult(new ReplyPluginNotfound() as IReplyMessageToPluginV2);
        }
        return plugin.Ask<IReplyMessageToPluginV2>(new GetMessageToPluginV2(message));
    }
    private IActorRef? GetPluginActorById(PluginId pluginId)
    {
        //return _pluginsV2.Find(p => p.Id == pluginId)!;
        if (_actorDict.TryGetValue(pluginId, out var actor))
        {
            return actor;
        }
        else
        {
            return null;
        }
    }
    internal void RemovePlugin(PluginId pluginId)
    {
        try
        {
            _pluginInfoDict.Remove(pluginId, out var _);
            _actorDict.Remove(pluginId, out var _);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
}
