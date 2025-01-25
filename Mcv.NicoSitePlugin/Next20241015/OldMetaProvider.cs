using Mcv.PluginV2;
using NicoSitePlugin.Metadata;
using NicoSitePlugin.V2;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mcv.NicoSitePlugin.Next20241015;

class OldMetaProvider
{
    public event EventHandler<IMetaMessage>? Received;
    Websocket? _ws;
    private readonly NewLogger _logger;

    public async Task ReceiveAsync(string websocketUrl)
    {
        if (_ws != null)
        {
            return;
        }
        _ws = new Websocket();
        _ws.Received += Ws_Received;
        _ws.Opened += Ws_Opened;
        try
        {
            await _ws.ReceiveAsync(websocketUrl);
        }
        finally
        {
            _ws.Received -= Ws_Received;
            _ws.Opened -= Ws_Opened;
            _ws = null;
        }
    }

    private void Ws_Opened(object? sender, EventArgs e)
    {
        var s = "{\"type\":\"startWatching\",\"data\":{\"stream\":{\"quality\":\"abr\",\"protocol\":\"hls\",\"latency\":\"low\",\"chasePlay\":false},\"room\":{\"protocol\":\"webSocket\",\"commentable\":true},\"reconnect\":false}}";
        try
        {
            _ws?.Send(s);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
        }
    }
    public void Send(IMetaMessage message)
    {
        Send(message.Raw);
    }
    public void Send(string message)
    {
        _ws?.Send(message);
    }
    private void Ws_Received(object? sender, string e)
    {
        var raw = e;
        Debug.WriteLine(raw);
        try
        {
            var message = MetaParser.Parse(raw);
            Received?.Invoke(this, message);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
        }
    }
    public void Disconnect()
    {
        _ws?.Disconnect();
    }
    public OldMetaProvider(NewLogger logger)
    {
        _logger = logger;
    }
}
