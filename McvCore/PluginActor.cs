using Akka.Actor;
using Mcv.PluginV2;
using Mcv.Core.PluginActorMessages;
using System.Diagnostics;
using System;

namespace Mcv.Core;

class PluginActor : ReceiveActor
{
    private readonly ICoreLogger _logger;
    public PluginActor(IPlugin plugin, ICoreLogger logger)
    {
        _logger = logger;

        Debug.WriteLine($"PluginActor::PluginActor(IPlugin) plugin.Name:{plugin.Name}");
        logger.AddLog($"PluginActor::PluginActor(IPlugin) plugin.Name:{plugin.Name}", LogType.Debug);

        ReceiveAsync<GetMessageToPluginV2>(async m =>
        {
            try
            {
                var reply = await plugin.RequestMessageAsync(m.Message);
                Sender.Tell(reply);
            }
            catch (Exception ex)
            {
                _logger.AddLog(ex);
            }
        });
        ReceiveAsync<NotifyMessageV2>(async m =>
        {
            try
            {
                await plugin.SetMessageAsync(m.Message);
            }
            catch (Exception ex)
            {
                _logger.AddLog(ex);
            }
        });
        ReceiveAsync<SetMessageToPluginV2>(async m =>
        {
            try
            {
                await plugin.SetMessageAsync(m.Message);
            }
            catch (Exception ex)
            {
                _logger.AddLog(ex);
            }
        });
    }
    public static Props Props(IPlugin plugin, ICoreLogger logger)
    {
        return Akka.Actor.Props.Create(() => new PluginActor(plugin, logger));
    }
}
