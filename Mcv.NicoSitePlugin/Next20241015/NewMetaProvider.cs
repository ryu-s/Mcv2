using NicoSitePlugin.Metadata;
using NicoSitePlugin.V2;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mcv.NicoSitePlugin.Next20241015;

class NewMetaProvider
{
    public System.Threading.Channels.Channel<IMetaProviderReturnValue> _channel = System.Threading.Channels.Channel.CreateUnbounded<IMetaProviderReturnValue>();
    public ConcurrentQueue<IMetaProviderReturnValue> _queue = [];
    CancellationTokenSource? _testCts;
    private readonly OldMetaProvider _metaProvider;
    public async Task ConnectAsync(string websocketUrl)
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
            await _metaProvider.ReceiveAsync(websocketUrl);
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
        _metaProvider.Disconnect();
    }
    public Task<IMetaProviderReturnValue> ReceiveAsync()
    {
        return _channel.Reader.ReadAsync().AsTask();
    }
    public void Send(IMetaMessage message)
    {
        _metaProvider.Send(message);
    }
    public NewMetaProvider(NewLogger logger)
    {
        _metaProvider = new OldMetaProvider(logger);
        _metaProvider.Received += MetaProvider_Received;
    }
    ~NewMetaProvider()
    {
        _metaProvider.Received -= MetaProvider_Received;
    }
    private void MetaProvider_Received(object? sender, IMetaMessage e)
    {
        _channel.Writer.TryWrite(new MetaProviderReturnValueMessage(e));
    }
}
