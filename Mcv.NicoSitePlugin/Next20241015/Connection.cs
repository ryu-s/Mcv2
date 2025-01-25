using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using NicoSitePlugin;
using NicoSitePlugin.Metadata;
using NicoSitePlugin.V2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nico = NicoSitePlugin;

namespace Mcv.NicoSitePlugin.Next20241015;
static class CancellationTokenSourceUtils
{
    public static Task AsTask(this CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<object>();
        cancellationToken.Register(() => tcs.TrySetCanceled(),
            useSynchronizationContext: false);
        return tcs.Task;
    }
}
class Connection
{
    public int LeastIntervalMs { get; set; } = 1000;
    CancellationTokenSource? _cts;
    private readonly NewLogger _logger;
    private readonly IPluginHost _host;
    private readonly ConnectionId _connId;
    private readonly PluginId _pluginId;
    private readonly IDataSource _server;
    public async Task ConnectAsync(string input, List<Cookie> cookies)
    {
        await AutoReconnectLoopAsync(input, cookies);
    }
    private async Task AutoReconnectLoopAsync(string input, List<Cookie> cookies)
    {
        if (_cts is not null)
        {
            throw new NotImplementedException();
        }
        _cts = new CancellationTokenSource();
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var startedAt = DateTime.Now;
                await ConnectOnceAsync(input, cookies, _cts.Token);
                var elapsed = DateTime.Now - startedAt;
                if (elapsed.TotalMilliseconds < LeastIntervalMs)
                {
                    await Task.Delay(LeastIntervalMs - (int)elapsed.TotalMilliseconds, _cts.Token).ContinueWith(_ => { });
                }
            }
        }
        finally
        {
            _cts = null;
        }
    }
    protected virtual CookieContainer CreateCookieContainer(List<Cookie> cookies)
    {
        var cc = new CookieContainer();
        try
        {
            foreach (var cookie in cookies)
            {
                cc.Add(cookie);
            }
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
        }
        return cc;
    }
    private async Task ConnectOnceAsync(string input, List<Cookie> cookies, CancellationToken ct)
    {
        var cc = CreateCookieContainer(cookies);
        var nicoInput = Nico.Tools.ParseInput(input);
        if (nicoInput is InvalidInput invalidInput)
        {
            //SendSystemInfo("未対応の形式のURLが入力されました", InfoType.Error);
            //AfterDisconnected();
            return;
        }
        string vid;
        if (nicoInput is LivePageUrl livePageUrl)
        {
            vid = livePageUrl.LiveId;
        }
        else if (nicoInput is ChannelUrl channelUrl)
        {
            vid = await GetChannelLiveId(channelUrl, ct, _server);
        }
        else if (nicoInput is CommunityUrl communityUrl)
        {
            vid = await GetCommunityLiveId(communityUrl, cc, ct, _server);
        }
        else if (nicoInput is LiveId liveId)
        {
            vid = liveId.Raw;
        }
        else
        {
            throw new InvalidOperationException("bug");
        }
        var dataProps = await DataProps.GetDataProps(_server, vid, cc);
        if (dataProps is null)
        {
            return;
        }
        var chat = new NewChatProvider(_server, _logger);

        var meta = new NewMetaProvider(_logger);
        var metaTask = meta.ConnectAsync(dataProps.WebsocketUrl);
        Task<IMetaProviderReturnValue>? metaReceiveTask = null;
        Task<IChatProviderReturnValue>? chatReceiveTask = null;
        Task? chatTask = null;
        while (!ct.IsCancellationRequested)
        {
            var tasks = new List<Task>();
            tasks.Add(metaTask);
            if (metaReceiveTask is null)
            {
                metaReceiveTask = meta.ReceiveAsync();
            }
            tasks.Add(metaReceiveTask);

            if (chatTask is not null && chatReceiveTask is null)
            {
                chatReceiveTask = chat.ReceiveAsync();
            }
            if (chatTask is not null)
            {
                tasks.Add(chatTask);
            }
            if (chatTask is not null && chatReceiveTask is not null)
            {
                tasks.Add(chatReceiveTask);
            }
            var cancelTask = ct.AsTask();
            tasks.Add(cancelTask);
            var t = await Task.WhenAny(tasks);
            if (t == chatReceiveTask)
            {
                var val = await chatReceiveTask;
                chatReceiveTask = null;
                if (val is ChatProviderReturnValueMessage m)
                {
                    var message = m.Message;
                    if (message.Message.Chat is not null)
                    {
                        Debug.WriteLine(message.Message.Chat.Content);
                    }
                }
            }
            else if (t == metaReceiveTask)
            {
                var val = await metaReceiveTask;
                metaReceiveTask = null;
                if (val is MetaProviderReturnValueMessage m)
                {
                    switch (m.Message)
                    {
                        case ServerTime serverTime:
                            break;
                        case Seat seat:
                            break;
                        case MessageServer messageServer:
                            chatTask = chat.ConnectAsync(messageServer.ViewUri);
                            break;
                        case Statistics statistics:
                            break;
                        case Disconnect disconnect:
                            break;
                        case Ping _:
                            meta.Send(new Pong());
                            break;
                        default:
                            break;
                    }
                }
                else if (val is MetaProviderReturnValueException ex)
                {

                }
                else if (val is MetaProviderReturnValueErrorMessage errMsg)
                {

                }
                else
                {

                }
            }
            else if (t == cancelTask)
            {
                meta.Disconnect();
                chat.Disconnect();

            }
            else
            {

            }
        }
        await Task.CompletedTask;
    }
    public void Disconnect()
    {
        _cts?.Cancel();
    }
    internal void NotifyMetadataUpdated(IMetadata e)
    {
        _host.SetMessageAsync(new SetMetadata(_connId, _pluginId, e));
    }

    internal void NotifyMessageReceived(ISiteMessage message, string? userId, IEnumerable<IMessagePart>? usernameItems, string? newNickname, bool isInitialComment)
    {
        _host.SetMessageAsync(new SetMessage(_connId, _pluginId, message, userId, usernameItems, newNickname, isInitialComment));
    }
    public Connection(NewLogger logger, IPluginHost host, ConnectionId connId, PluginId pluginId)
    {
        _logger = logger;
        _host = host;
        _connId = connId;
        _pluginId = pluginId;
        _server = new DataSource("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
    }
    private static async Task<string> GetChannelLiveId(ChannelUrl channelUrl, CancellationToken token, IDataSource _server)
    {
check:
        var currentLiveId = await Api.GetCurrentChannelLiveId(_server, channelUrl.ChannelScreenName);
        if (currentLiveId != null)
        {
            return currentLiveId;
        }
        else
        {
            //RaiseMetadataUpdated(new TestMetadata
            //{
            //    Title = "（次の配信が始まるまで待機中...）",
            //});
            await Task.Delay(30 * 1000, token);
            goto check;
        }
    }
    private static async Task<string> GetCommunityLiveId(CommunityUrl communityUrl, CookieContainer cc, CancellationToken token, IDataSource _server)
    {
check:
        var currentLiveId = await Api.GetCurrentCommunityLiveId(_server, communityUrl.CommunityId, cc);
        if (currentLiveId != null)
        {
            return currentLiveId;
        }
        else
        {
            //RaiseMetadataUpdated(new TestMetadata
            //{
            //    Title = "（次の配信が始まるまで待機中...）",
            //});
            await Task.Delay(30 * 1000, token);
            goto check;
        }
    }
}
