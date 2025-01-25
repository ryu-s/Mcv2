using Mcv.PluginV2;
using NicoSitePlugin.V2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Mcv.NicoSitePlugin.Next20241015;

class NewConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<ConnectionId, Connection> _connDict = new();

    private readonly NewLogger _logger;

    public void AddConnection(ConnectionId connId, PluginId pluginId, IPluginHost host)
    {
        if (!_connDict.TryAdd(connId, new Connection(_logger, host, connId, pluginId)))
        {
            throw new NotImplementedException();
        }
    }
    public async Task ConnectAsync(ConnectionId connId, string input, List<Cookie> cookies)
    {
        if (!_connDict.TryGetValue(connId, out var conn))
        {
            throw new NotImplementedException();
        }
        try
        {
            await conn.ConnectAsync(input, cookies).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
        }
    }
    public async Task DisconnectAsync(ConnectionId connId)
    {
        if (!_connDict.TryGetValue(connId, out var conn))
        {
            throw new NotImplementedException();
        }
        conn.Disconnect();
        await Task.CompletedTask;
    }
    public void RemoveConnection(ConnectionId connId)
    {
        _connDict.TryRemove(connId, out _);
    }
    public NewConnectionManager(NewLogger logger)
    {
        _logger = logger;
    }
}
