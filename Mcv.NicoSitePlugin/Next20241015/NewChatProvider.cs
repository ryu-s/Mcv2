using Mcv.NicoSitePlugin.InternalMessage;
using NicoSitePlugin;
using NicoSitePlugin.V2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mcv.NicoSitePlugin.Next20241015;
interface IChatProviderReturnValue { }
class ChatProviderReturnValueMessage(MessageReceivedEventArgs message) : IChatProviderReturnValue
{
    public MessageReceivedEventArgs Message { get; } = message;
}
class ChatProviderReturnValueException(Exception ex) : IChatProviderReturnValue
{
    public Exception Exception { get; } = ex;
}
class OldChatProvider
{
    private readonly IDataSource _server;
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    CancellationTokenSource? _cts;
    private readonly NewLogger _logger;
    public OldChatProvider(IDataSource server, NewLogger logger)
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
        _cts = null;
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
class NewChatProvider
{
    public System.Threading.Channels.Channel<IChatProviderReturnValue> _channel = System.Threading.Channels.Channel.CreateUnbounded<IChatProviderReturnValue>();
    public ConcurrentQueue<IChatProviderReturnValue> _queue = [];
    CancellationTokenSource? _testCts;
    private readonly OldChatProvider _chatProvider;
    public async Task ConnectAsync(string viewUri)
    {
        //var nicoInput = Nico.Tools.ParseInput(input);
        //if (nicoInput is InvalidInput invalidInput)
        //{
        //    _channel.Writer.TryWrite(new MetaProviderReturnValueErrorMessage(invalidInput.ToString() ?? ""));
        //    AfterDisconnected();
        //    return;
        //}
        _testCts = new CancellationTokenSource();
        try
        {
            await _chatProvider.ReceiveAsync(viewUri);
        }
        catch (Exception ex)
        {
            _channel.Writer.TryWrite(new ChatProviderReturnValueException(ex));
            _channel.Writer.Complete();
        }
        finally
        {
            _testCts = null;
        }
    }
    public void Disconnect()
    {
        AfterDisconnected();
    }
    private void AfterDisconnected()
    {
        _testCts?.Cancel();
        _channel.Writer.Complete();
        _chatProvider.Disconnect();
    }
    public Task<IChatProviderReturnValue> ReceiveAsync()
    {
        return _channel.Reader.ReadAsync().AsTask();
    }
    public NewChatProvider(IDataSource server, NewLogger logger)
    {
        _chatProvider = new OldChatProvider(server, logger);
        _chatProvider.MessageReceived += MetaProvider_Received;
    }
    ~NewChatProvider()
    {
        _chatProvider.MessageReceived -= MetaProvider_Received;
    }
    private void MetaProvider_Received(object? sender, MessageReceivedEventArgs e)
    {
        _channel.Writer.TryWrite(new ChatProviderReturnValueMessage(e));
    }
}
