using Mcv.PluginV2;
using Mcv.PluginV2.Messages;
using NicoSitePlugin.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace NicoSitePlugin.V2
{
    class CommentProviderHost
    {
        private readonly IPluginHost _host;
        private readonly ConnectionId _connId;
        private readonly PluginId _pluginId;

        internal void NotifyMetadataUpdated(IMetadata e)
        {
            _host.SetMessageAsync(new SetMetadata(_connId, _pluginId, e));
        }

        internal void NotifyMessageReceived(ISiteMessage message, string? userId, IEnumerable<IMessagePart>? usernameItems, string? newNickname, bool isInitialComment)
        {
            _host.SetMessageAsync(new SetMessage(_connId, _pluginId, message, userId, usernameItems, newNickname, isInitialComment));
        }
        public CommentProviderHost(IPluginHost host, ConnectionId connId, PluginId pluginId)
        {
            _host = host;
            _connId = connId;
            _pluginId = pluginId;
        }
    }
    class CommentProviderWrapper
    {
        private readonly ICommentProvider _commentProvider;
        private readonly CommentProviderHost _host;

        public CommentProviderWrapper(ICommentProvider commentProvider, CommentProviderHost host)
        {
            _commentProvider = commentProvider;
            _host = host;
            commentProvider.MessageReceived += CommentProvider_MessageReceived;
            commentProvider.MetadataUpdated += CommentProvider_MetadataUpdated;
        }
        ~CommentProviderWrapper()
        {
            _commentProvider.MessageReceived -= CommentProvider_MessageReceived;
            _commentProvider.MetadataUpdated -= CommentProvider_MetadataUpdated;
        }
        private void CommentProvider_MetadataUpdated(object? sender, IMetadata e)
        {
            _host.NotifyMetadataUpdated(e);
        }

        private void CommentProvider_MessageReceived(object? sender, IMessageContext e)
        {
            _host.NotifyMessageReceived(e.Message, e.UserId, e.UsernameItems, e.NewNickname, e.IsInitialComment);
        }

        internal Task ConnectAsync(string input, List<Cookie> cookies)
        {
            return _commentProvider.ConnectAsync(input, cookies);
        }
        internal void Disconnect()
        {
            _commentProvider.Disconnect();
        }
    }
    public interface IConnectionManager
    {
        void AddConnection(ConnectionId connId, PluginId pluginId, IPluginHost host);
        void RemoveConnection(ConnectionId connId);
        Task ConnectAsync(ConnectionId connId, string input, List<Cookie> cookies);
        Task DisconnectAsync(ConnectionId connId);
    }

    class OldConnectionManager : IConnectionManager
    {
        private readonly Dictionary<ConnectionId, CommentProviderWrapper> _connDict = [];
        private readonly Dictionary<ConnectionId, Task> _connectionTaskDict = [];
        private readonly NicoSiteContext _context;
        public OldConnectionManager(NicoSiteContext context)
        {
            _context = context;
        }
        public void AddConnection(ConnectionId connId, PluginId pluginId, IPluginHost host)
        {
            var provider = _context.CreateCommentProvider();
            var wrappter = new CommentProviderWrapper(provider, new CommentProviderHost(host, connId, pluginId));
            if (_connDict.ContainsKey(connId))
            {
                _connDict[connId] = wrappter;
            }
            else
            {
                _connDict.Add(connId, wrappter);
            }
        }
        public async Task ConnectAsync(ConnectionId connId, string input, List<Cookie> cookies)
        {
            if (!_connDict.TryGetValue(connId, out var wrapper))
            {
                return;
            }
            var connectionTask = wrapper.ConnectAsync(input, cookies);
            _connectionTaskDict.Add(connId, connectionTask);
            await Task.CompletedTask;
        }
        public async Task DisconnectAsync(ConnectionId connId)
        {
            if (!_connDict.TryGetValue(connId, out var wrapper))
            {
                return;
            }
            wrapper.Disconnect();
            var connectionTask = _connectionTaskDict[connId];
            try
            {
                await connectionTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

            }
            _connectionTaskDict.Remove(connId);
        }
        public void RemoveConnection(ConnectionId connId)
        {
            _connDict.Remove(connId);
        }
    }
    [Export(typeof(IPlugin))]
    public class PluginMain : IPlugin
    {
        public IPluginHost Host { get; set; } = default!;
        public PluginId Id { get; } = new PluginId(new Guid("852C766E-B60E-4FA9-92FE-387F310C0124"));
        public string Name { get; } = "NicoSitePlugin";
        public List<string> Roles { get; } = new List<string> { "site:nicolive", "gui" };
        private IConnectionManager? _connectionManager;
        private readonly ConcurrentDictionary<ConnectionId, Task> _connectionTaskDict = [];
        //protected virtual async Task<IConnectionManager> CreateConnectionManagerAsync()
        //{
        //    var userAgent = await GetUserAgent();
        //    var context = new NicoSiteContext(new DataSource(userAgent), new Logger(Host));
        //    var res = await Host.RequestMessageAsync(new RequestLoadPluginOptions(Name)) as ReplyPluginOptions;
        //    context.LoadOptions(res?.RawOptions ?? "");
        //    return new OldConnectionManager(context);
        //}
        protected virtual async Task<IConnectionManager> CreateConnectionManagerAsync()
        {
            await Task.CompletedTask;
            return new Mcv.NicoSitePlugin.Next20241015.NewConnectionManager(new NewLogger(Host));
        }
        private async Task<string> GetUserAgent()
        {
            var res = await Host.RequestMessageAsync(new GetUserAgent()) as ReplyUserAgent;
            return res?.UserAgent ?? "";
        }
        public async Task SetMessageAsync(ISetMessageToPluginV2 message)
        {
            switch (message)
            {
                case SetLoading _:
                    {
                        _connectionManager = await CreateConnectionManagerAsync();
                        await Host.SetMessageAsync(new SetPluginHello(Id, Name, Roles));
                    }
                    break;
                case SetLoaded _:
                    {
                    }
                    break;
                case SetClosing _:
                    {
                    }
                    break;
                case SetCreateCommentProvider createCommentProvider:
                    {
                        _connectionManager?.AddConnection(createCommentProvider.ConnId, Id, Host);
                    }
                    break;
                case SetDestroyCommentProvider destroyCommentProvider:
                    {
                        _connectionManager?.RemoveConnection(destroyCommentProvider.ConnId);
                    }
                    break;
                case SetConnectSite connect:
                    {
                        if (_connectionManager is not null)
                        {
                            await Task.CompletedTask.ConfigureAwait(false);
                            var t = _connectionManager.ConnectAsync(connect.ConnId, connect.Input, connect.Cookies);
                            _connectionTaskDict.TryAdd(connect.ConnId, t);
                            await Host.SetMessageAsync(new NotifySiteConnected(connect.ConnId)).ConfigureAwait(false);

                        }
                    }
                    break;
                case SetDisconnectSite disconnect:
                    {
                        if (_connectionManager is not null)
                        {
                            await _connectionManager.DisconnectAsync(disconnect.ConnId);
                            var task = _connectionTaskDict[disconnect.ConnId];
                            await task.ConfigureAwait(false);
                            await Host.SetMessageAsync(new NotifySiteDisconnected(disconnect.ConnId)).ConfigureAwait(false);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public async Task SetMessageAsync(INotifyMessageV2 message)
        {
            await Task.CompletedTask;
        }

        public async Task<IReplyMessageToPluginV2> RequestMessageAsync(IGetMessageToPluginV2 message)
        {
            switch (message)
            {
                case GetSitePluginDisplayName _:
                    return new ReplySitePluginDisplayName("ニコ生");
                case GetIsValidSiteUrl isValidUrl:
                    return new ReplyIsValidSiteUrl(Tools.IsValidInput(isValidUrl.Input));
                case GetSiteDomain _:
                    return new ReplySiteDomain("nicovideo.jp");
                case GetSettingsPanel _:
                    return new AnswerSettingsPanel(new TempTagPanel());
            }
            throw new NotImplementedException();
        }
        private async Task SetExceptionAsync(Exception exception)
        {
            await Host.SetMessageAsync(new SetException(exception, "", ""));
        }
    }
    class TempTagPanel : IOptionsTabPage
    {
        public string HeaderText { get; } = "";
        public System.Windows.Controls.UserControl TabPagePanel { get; } = new();

        public void Apply()
        {
        }

        public void Cancel()
        {
        }
    }
    class NewLogger(IPluginHost host)
    {
        public void LogException(Exception ex, string message = "", string detail = "")
        {
            host.SetMessageAsync(new SetException(ex, message, detail));
        }
    }
    class Logger : ILogger
    {
        private readonly IPluginHost _host;

        public string GetExceptions()
        {
            return "";
        }

        public void LogException(Exception ex, string message = "", string detail = "")
        {
            _host.SetMessageAsync(new SetException(ex, message, detail));
        }
        public Logger(IPluginHost host)
        {
            _host = host;
        }
    }
}
