using Mcv.PluginV2;
using NicoSitePlugin;
using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class McvNicoCommentViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    public McvNicoCommentViewModel(INicoComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
    : base(connName, options, user)
    {
        MessageItems = MessagePartFactory.CreateMessageItems(comment.Text);
        if (IsValudThumbnailUrl(comment.ThumbnailUrl))
        {
            Thumbnail = new MessageImage
            {
                Url = comment.ThumbnailUrl,
                Height = 40,
                Width = 40,
            };
        }
        Id = comment.Id;
        PostTime = comment.PostedAt.ToLocalTime().ToString("HH:mm:ss");
    }

    public McvNicoCommentViewModel(NicoSitePlugin.INicoAd ad, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
    : base(connName, options, user)
    {
        //_nameItems = MessagePartFactory.CreateMessageItems(ad.UserName);
        MessageItems = MessagePartFactory.CreateMessageItems(ad.Text);
        PostTime = ad.PostedAt.ToString("HH:mm:ss");
        Info = "広告";
    }
    public McvNicoCommentViewModel(NicoSitePlugin.INicoGift item, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
    : base(connName, options, user)
    {
        //_nameItems = MessagePartFactory.CreateMessageItems(item.UserName);
        MessageItems = MessagePartFactory.CreateMessageItems(item.Text);
        PostTime = item.PostedAt.ToString("HH:mm:ss");
        Info = "ギフト";
    }
    public McvNicoCommentViewModel(NicoSitePlugin.INicoSpi item, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
    : base(connName, options, user)
    {
        //_nameItems = MessagePartFactory.CreateMessageItems(item.UserName);
        MessageItems = MessagePartFactory.CreateMessageItems(item.Text);
        PostTime = item.PostedAt.ToString("HH:mm:ss");
        Info = "リクエスト";
    }
    public McvNicoCommentViewModel(NicoSitePlugin.INicoEmotion item, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
    : base(connName, options, user)
    {
        //_nameItems = MessagePartFactory.CreateMessageItems(item.UserName);
        MessageItems = MessagePartFactory.CreateMessageItems(item.Content);
        PostTime = item.PostedAt.ToString("HH:mm:ss");
        Info = "エモーション";
    }
    public McvNicoCommentViewModel(NicoSitePlugin.INicoInfo info, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
    : base(connName, options, user)
    {
        //_nameItems = MessagePartFactory.CreateMessageItems(info.UserName);
        MessageItems = MessagePartFactory.CreateMessageItems(info.Text);
        PostTime = info.PostedAt.ToString("HH:mm:ss");
    }
    public McvNicoCommentViewModel(NicoSitePlugin.INicoConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
    : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public McvNicoCommentViewModel(NicoSitePlugin.INicoDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
    : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems(disconnected.Text);
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.NicoLiveForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.NicoLiveBackColor);
    }
    private static bool IsValudThumbnailUrl(string thumbnailUrl)
    {
        return !string.IsNullOrEmpty(thumbnailUrl);
    }
}
