using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json;
using NicoSitePlugin.Metadata;
using Mcv.PluginV2;
using Mcv.NicoSitePlugin.InternalMessage;
using System.Diagnostics;
using System.IO;
using MessageV2 = Mcv.NicoSitePlugin.MessageV2;
using Mcv.NicoSitePlugin.MessageV2;
namespace NicoSitePlugin
{
    class TestCommentProvider : CommentProviderBase, INicoCommentProvider, IDisposable
    {
        private readonly ILogger _logger;
        private readonly INicoSiteOptions _siteOptions;
        private readonly IDataSource _server;
        private readonly Metadata.MetaProvider _metaProvider;
        CancellationTokenSource _disconnectCts;
        private DataProps? ExtractDataProps(string livePagehtml)
        {
            var match = Regex.Match(livePagehtml, "<script [^>]+ data-props=\"([^>]+)\"></script>");
            if (!match.Success) return null;
            var pre = match.Groups[1].Value;
            var dataPropsJson = pre.Replace("&quot;", "\"");
            var dataProps = new DataProps(dataPropsJson);
            return dataProps;
        }
        public override async Task ConnectAsync(string input, List<Cookie> cookies)
        {
            BeforeConnect();
            var nicoInput = Tools.ParseInput(input);
            if (nicoInput is InvalidInput invalidInput)
            {
                SendSystemInfo("未対応の形式のURLが入力されました", InfoType.Error);
                AfterDisconnected();
                return;
            }
            _isFirstConnection = true;
reload:
            _isDisconnectedExpected = false;
            _disconnectCts = new CancellationTokenSource();
            try
            {
                await ConnectInternalAsync(nicoInput, cookies);
            }
            catch (ApiGetCommunityLivesException ex)
            {
                _isDisconnectedExpected = true;
                SendSystemInfo("コミュニティの配信状況の取得に失敗しました", InfoType.Error);
                _logger.LogException(ex, "", $"input:{input}");
            }
            catch (SpecChangedException ex)
            {
                _isDisconnectedExpected = true;
                SendSystemInfo("サイトの仕様変更があったためコメント取得を継続できません", InfoType.Error);
                _logger.LogException(ex, "", $"input:{input}");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "", $"input:{input}");
            }
            _dataProps = null;
            if (!_isDisconnectedExpected)
            {
                _isFirstConnection = false;
                goto reload;
            }
            var m = new MessageV2.NicoDisconnected();
            var c = new NicoMessageContext(m, null, null, false, null);
            RaiseMessageReceived(c);
            AfterDisconnected();
        }
        private async Task<string> GetChannelLiveId(ChannelUrl channelUrl)
        {
check:
            var currentLiveId = await Api.GetCurrentChannelLiveId(_server, channelUrl.ChannelScreenName);
            if (currentLiveId != null)
            {
                return currentLiveId;
            }
            else
            {
                RaiseMetadataUpdated(new TestMetadata
                {
                    Title = "（次の配信が始まるまで待機中...）",
                });
                await Task.Delay(30 * 1000, _disconnectCts.Token);
                goto check;
            }
        }
        private async Task<string> GetCommunityLiveId(CommunityUrl communityUrl, CookieContainer cc)
        {
check:
            var currentLiveId = await Api.GetCurrentCommunityLiveId(_server, communityUrl.CommunityId, cc);
            if (currentLiveId != null)
            {
                return currentLiveId;
            }
            else
            {
                RaiseMetadataUpdated(new TestMetadata
                {
                    Title = "（次の配信が始まるまで待機中...）",
                });
                await Task.Delay(30 * 1000, _disconnectCts.Token);
                goto check;
            }
        }
        CookieContainer _cc;
        public async Task ConnectInternalAsync(IInput input, List<Cookie> cookies)
        {
            var cc = CreateCookieContainer(cookies);
            string vid;
            if (input is LivePageUrl livePageUrl)
            {
                vid = livePageUrl.LiveId;
            }
            else if (input is ChannelUrl channelUrl)
            {
                vid = await GetChannelLiveId(channelUrl);
            }
            else if (input is CommunityUrl communityUrl)
            {
                vid = await GetCommunityLiveId(communityUrl, cc);
            }
            else if (input is LiveId liveId)
            {
                vid = liveId.Raw;
            }
            else
            {
                throw new InvalidOperationException("bug");
            }
            var url = "https://live.nicovideo.jp/watch/" + vid;


            var liveHtml = await _server.GetAsync(url, cc);
            _dataProps = ExtractDataProps(liveHtml);
            if (_dataProps == null)
            {
                throw new SpecChangedException("data-propsが無い", liveHtml);
            }
            if (_dataProps.Status == "ENDED")
            {
                SendSystemInfo("この番組は終了しました", InfoType.Notice);
                if (input is LivePageUrl)//チャンネルやコミュニティのURLを入力した場合は次の配信が始まるまで待機する
                {
                    _isDisconnectedExpected = true;
                }
                return;
            }
            _vposBaseTime = UnixTimeConverter.FromUnixTime(_dataProps.VposBaseTime);
            _localTime = DateTime.Now;
            RaiseMetadataUpdated(new TestMetadata
            {
                Title = _dataProps.Title,
            });

            var metaTask = _metaProvider.ReceiveAsync(_dataProps.WebsocketUrl);
            _tasks.Add(metaTask);
            _mainLooptcs = new TaskCompletionSource<object>();
            _tasks.Add(_mainLooptcs.Task);

            while (_tasks.Count > 1)//1の場合は_mainLooptcs.Taskだからループを終了する
            {
                var t = await Task.WhenAny(_tasks);
                if (t == _mainLooptcs.Task)
                {
                    _tasks.Remove(_mainLooptcs.Task);
                    _tasks.AddRange(_toAdd);
                    _toAdd.Clear();
                    _mainLooptcs = new TaskCompletionSource<object>();
                    _tasks.Add(_mainLooptcs.Task);
                }
                else if (t == metaTask)
                {
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    _tasks.Remove(metaTask);
                }
                else//roomTask
                {
                    _metaProvider?.Disconnect();
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    _tasks.Remove(metaTask);
                    try
                    {
                        await t;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    _tasks.Clear();//本当はchatのTaskだけ取り除きたいけど、変数に取ってなくて無理だから全部消しちゃう
                }
            }
            return;
        }
        /// <summary>
        /// 初期コメント取得中か
        /// </summary>
        private bool _isInitialCommentsReceiving;
        protected readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();
        /// <summary>
        /// 意図的な切断か
        /// </summary>
        private bool _isDisconnectedExpected;
        /// <summary>
        /// 一番最初の接続か。再接続時はfalse。
        /// 再接続時は初期コメントが不要だから主にその判別に使うフラグ
        /// </summary>
        private bool _isFirstConnection;

        readonly List<Task> _tasks = [];
        readonly List<Task> _toAdd = [];
        TaskCompletionSource<object> _mainLooptcs;
        private readonly ChatProvider2 _chatProvider2;
        DataProps? _dataProps;
        private bool _disposedValue;

        private void MetaProvider_Received(object? sender, Metadata.IMetaMessage e)
        {
            var message = e;

            try
            {
                switch (message)
                {
                    case Metadata.Ping ping:
                        _metaProvider?.Send(new Metadata.Pong());
                        break;
                    case Metadata.Statistics stat:
                        RaiseMetadataUpdated(new TestMetadata
                        {
                            TotalViewers = stat.Viewers.ToString(),
                            Others = $"コメント数:{stat.Comments} 広告ポイント:{stat.AdPoints} ギフトポイント:{stat.GiftPoints}",
                        });
                        break;
                    case Metadata.Disconnect disconnect:
                        SendSystemInfo($"メタデータサーバーとの接続が切断されました{Environment.NewLine}原因:{disconnect.Reason}", InfoType.Notice);
                        //Disconnect();
                        break;
                    case Metadata.ServerTime serverTime:
                        break;
                    case Metadata.MessageServer server:
                        {
                            var t = _chatProvider2.ReceiveAsync(server.ViewUri);
                            _toAdd.Add(t);
                            _mainLooptcs.SetResult(null);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        DateTime? _vposBaseTime;
        DateTime? _localTime;
        /// <summary>
        /// 意図的な切断
        /// </summary>
        public override void Disconnect()
        {
            _isDisconnectedExpected = true;
            _disconnectCts.Cancel();
            _metaProvider?.Disconnect();
            _chatProvider2?.Disconnect();
        }

        public override async Task<ICurrentUserInfo> GetCurrentUserInfo(List<Cookie> cookies)
        {
            var cc = CreateCookieContainer(cookies);
            try
            {
                var myInfo = await Api.GetMyInfo(_server, cc);
                return await Task.FromResult(new CurrentUserInfo(Username: myInfo.Nickname, IsLoggedIn: myInfo.IsLogin));
            }
            catch (NotLoggedInException)
            {
                return await Task.FromResult(new CurrentUserInfo(Username: "(未ログイン)", IsLoggedIn: false));
            }
        }
        record CurrentUserInfo(string Username, bool IsLoggedIn) : ICurrentUserInfo;
        public override Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }

        public override void SetMessage(string raw)
        {
        }

        Task INicoCommentProvider.PostCommentAsync(string comment, bool is184, string color, string size, string position)
        {
            var elapsed = DateTime.Now.AddHours(-9) - _vposBaseTime.Value;
            var ms = elapsed.TotalMilliseconds;
            var vpos = (long)Math.Round(ms / 10);
            var postComment = new PostComment(comment, vpos, is184, color, size, position);
            _metaProvider.Send(postComment);
            return Task.CompletedTask;
        }
        public TestCommentProvider(INicoSiteOptions siteOptions, IDataSource server, ILogger logger) : base(logger)
        {
            _logger = logger;
            _siteOptions = siteOptions;
            _server = server;
            _metaProvider = new Metadata.MetaProvider(_logger);
            _metaProvider.Received += MetaProvider_Received;
            _chatProvider2 = new ChatProvider2(server, logger);
            _chatProvider2.MessageReceived += ChatProvider2_MessageReceived;
        }
        static DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        static DateTime FromUnixTime(long unixTime)
        {
            return baseTime.AddSeconds(unixTime);
        }
        private void ChatProvider2_MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Chat is Mcv.NicoSitePlugin.InternalMessage.Chat chat)
            {
                Debug.WriteLine($"secs={e.Meta.At.Seconds} nanos={e.Meta.At.Nanos} vpos={chat.Vpos} content={chat.Content}");
                var comment = new MessageV2.NicoComment
                {
                    Content = chat.Content,
                    No = chat.No,
                    UserId = chat.RawUserId?.ToString() ?? chat.HashedUserId ?? "",
                    UserName = chat.Name,
                    Vpos = chat.Vpos,
                    DateTime = FromUnixTime(e.Meta.At.Seconds),
                };
                var context = new NicoMessageContext(comment, chat.HashedUserId, null, e.IsInitialComment, chat.Name);
                RaiseMessageReceived(context);
            }
            else if (e.Message.Gift is Mcv.NicoSitePlugin.InternalMessage.Gift gift)
            {
                RaiseMessageReceived(new NicoMessageContext(new MessageV2.NicoGift()
                {
                    ItemId = gift.ItemId,
                    UserName = gift.AdvertiserName,
                    Message = gift.Message,
                    UserId = gift.AdvertiserUserId,
                    ItemName = gift.ItemName,
                    Content = gift.Content,
                    DateTime = FromUnixTime(e.Meta.At.Seconds),
                }, gift.AdvertiserUserId?.ToString(), null, false, gift.AdvertiserName));
            }
            else if (e.Message.Nicoad is Mcv.NicoSitePlugin.InternalMessage.Nicoad ad)
            {
                var _dateTime = FromUnixTime(e.Meta.At.Seconds);
            }
            else if (e.Message.SimpleNotification is Mcv.NicoSitePlugin.InternalMessage.SimpleNotification sim)
            {
                RaiseMessageReceived(new NicoMessageContext(new NicoSimpleNotification(sim, FromUnixTime(e.Meta.At.Seconds)),
                    null, null, false, null));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _metaProvider.Received -= MetaProvider_Received;
                    _chatProvider2.MessageReceived -= ChatProvider2_MessageReceived;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TestCommentProvider()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    class MessageReceivedEventArgs(Mcv.NicoSitePlugin.InternalMessage.Meta meta, Mcv.NicoSitePlugin.InternalMessage.NicoliveMessage message, bool isInitialComment) : EventArgs
    {
        public Mcv.NicoSitePlugin.InternalMessage.Meta Meta { get; } = meta;
        public Mcv.NicoSitePlugin.InternalMessage.NicoliveMessage Message { get; } = message;
        public bool IsInitialComment { get; } = isInitialComment;
    }
    class ChatProvider2
    {
        private readonly IDataSource _server;
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        CancellationTokenSource? _cts;
        private readonly ILogger _logger;
        public ChatProvider2(IDataSource server, ILogger logger)
        {
            _server = server;
            _logger = logger;
        }
        internal async Task ReceiveAsync(string uri)
        {
            if (_cts is not null)
            {
                throw new InvalidOperationException("既にReceiveAsync()が呼ばれている");
            }
            _cts = new CancellationTokenSource();

            var urlz = uri + "?at=now";
            var isLiveEnded = false;
            var isBackWardReceived = false;//BackwardSegmentは一番最初に送られてくるとは限らない。
            while (!isLiveEnded && _cts is not null && !_cts.IsCancellationRequested)
            {
                Debug.WriteLine("取得中");
                List<ChunkedEntry> entries;
                byte[]? rawChunkedEntry = null;
                try
                {
                    rawChunkedEntry = await _server.GetBytesAsync(urlz);
                    Debug.WriteLine("取得完了");
                    entries = ChunkedEntry.Create(rawChunkedEntry);
                }
                catch (Exception ex)
                {
                    if (rawChunkedEntry is not null)
                    {
                        _logger.LogException(ex, "", $"data:{ToHex(rawChunkedEntry)}");
                    }
                    else
                    {
                        _logger.LogException(ex);
                    }
                    continue;
                }

                foreach (var entry in entries)
                {
                    if (entry.Backward is BackwardSegment backward && !isBackWardReceived)
                    {
                        var bytes = await _server.GetBytesAsync(backward.Segment);
                        var a = PackedSegment.Create(bytes);
                        foreach (var m in a.Messages)
                        {
                            if (m.Message is NicoliveMessage message)
                            {
                                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(m.Meta, message, true));
                            }
                        }
                        isBackWardReceived = true;
                    }
                }
                foreach (var entry in entries)
                {
                    if (entry.Previous is MessageSegment previous)
                    {
                        var bytes = await _server.GetBytesAsync(previous.Uri);
                        var ms = ChunkedMessage.Create2(bytes);
                        foreach (var m in ms)
                        {
                            if (m.Message is NicoliveMessage message)
                            {
                                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(m.Meta, message, false));
                            }
                        }
                    }

                }
                foreach (var entry in entries)
                {
                    if (entry.Segment is MessageSegment segment)
                    {
                        List<ChunkedMessage> ns;
                        byte[]? rawChunkedMessage = null;
                        try
                        {
                            rawChunkedMessage = await _server.GetBytesAsync(segment.Uri);
                            ns = ChunkedMessage.Create2(rawChunkedMessage);
                        }
                        catch (Exception ex)
                        {
                            if (rawChunkedMessage is not null)
                            {
                                _logger.LogException(ex, "", $"data:{ToHex(rawChunkedMessage)}");
                            }
                            else
                            {
                                _logger.LogException(ex);
                            }
                            continue;
                        }
                        foreach (var n in ns)
                        {
                            if (n.Message is Mcv.NicoSitePlugin.InternalMessage.NicoliveMessage message)
                            {
                                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(n.Meta, message, false));
                            }
                            else if (n.Signal is Signal.Flushed)
                            {

                            }
                            else if (n.State is NicoliveState state)
                            {
                                if (state?.State == ProgramState.Ended)
                                {
                                    isLiveEnded = true;
                                }
                            }
                            else
                            {

                            }
                        }
                        Debug.WriteLine($"from={segment.From} until={segment.Until}");
                    }
                }

                foreach (var entry in entries)
                {
                    if (entry.Next is ReadyForNext next)
                    {
                        urlz = uri + "?at=" + next.At;
                        Debug.WriteLine("======");
                        break;
                    }
                }
            }
            Debug.WriteLine("ChatProvider.ReceiveAsync() finished");
        }
        public void Disconnect()
        {
            _cts?.Cancel();
        }
        private static string ToHex(byte[] bytes)
        {
            return "0x" + BitConverter.ToString(bytes).Replace("-", ",0x");
        }
    }
}
