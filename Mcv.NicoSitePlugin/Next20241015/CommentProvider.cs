using Mcv.PluginV2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Mcv.NicoSitePlugin.Next20241015;
internal class CommentProvider : ICommentProvider
{
    public bool CanConnect { get; }
    public bool CanDisconnect { get; }

    public event EventHandler<ConnectedEventArgs>? Connected;
    public event EventHandler<IMessageContext>? MessageReceived;
    public event EventHandler<IMetadata>? MetadataUpdated;
    public event EventHandler? CanConnectChanged;
    public event EventHandler? CanDisconnectChanged;

    public async Task ConnectAsync(string input, List<Cookie> cookies)
    {
        //切断条件
        //・例外が発生した場合
        //・切断ボタンが押された場合
        //・入力値が配信URLで、その配信が終了した時
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public void Disconnect()
    {
        throw new NotImplementedException();
    }

    public Task<ICurrentUserInfo> GetCurrentUserInfo(List<Cookie> cookies)
    {
        throw new NotImplementedException();
    }

    public Task PostCommentAsync(string text)
    {
        throw new NotImplementedException();
    }

    public void SetMessage(string raw)
    {
        throw new NotImplementedException();
    }
}
