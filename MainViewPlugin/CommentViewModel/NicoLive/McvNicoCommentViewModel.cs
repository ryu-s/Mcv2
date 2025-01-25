using Mcv.PluginV2;
using Mcv.NicoSitePlugin.MessageV2;
using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class McvNicoSimpleNotificationCommentViewModelV2 : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    public McvNicoSimpleNotificationCommentViewModelV2(INicoSimpleNotification simple, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
        : base(connName, options, user)
    {
        MessageItems = MessagePartFactory.CreateMessageItems(simple.Content);
        PostTime = simple.DateTime.ToLocalTime().ToString("HH:mm:ss");
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.NicoLiveForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.NicoLiveBackColor);
    }
    public override SolidColorBrush Background { get => new SolidColorBrush(_options.NicoLiveSimpleNotificationBackColor); }
    public override SolidColorBrush Foreground { get => new SolidColorBrush(_options.NicoLiveSimpleNotificationForeColor); }
}
class McvNicoCommentViewModelV2 : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    public McvNicoCommentViewModelV2(INicoComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    : base(connName, options, user)
    {
        MessageItems = MessagePartFactory.CreateMessageItems(comment.Content);
        _nameItems = MessagePartFactory.CreateMessageItems(comment.UserName);
        //if (IsValudThumbnailUrl(comment.ThumbnailUrl))
        //{
        //    Thumbnail = new MessageImage
        //    {
        //        Url = comment.ThumbnailUrl,
        //        Height = 40,
        //        Width = 40,
        //    };
        //}
        //Id = comment.Id;
        PostTime = comment.DateTime.ToLocalTime().ToString("HH:mm:ss");
        Id = $"{comment.No}";
    }

    //public McvNicoCommentViewModelV2(INicoAd ad, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
    //: base(connName, options, user)
    //{
    //    //_nameItems = MessagePartFactory.CreateMessageItems(ad.UserName);
    //    MessageItems = MessagePartFactory.CreateMessageItems(ad);
    //    PostTime = ad.PostedAt.ToString("HH:mm:ss");
    //    Info = "広告";
    //}
    public McvNicoCommentViewModelV2(INicoGift item, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    : base(connName, options, user)
    {
        //_nameItems = MessagePartFactory.CreateMessageItems(item.UserName);
        //名無しさんがギフト「応援うさぎもどき（20pt）」を贈りました
        MessageItems = MessagePartFactory.CreateMessageItems(item.Content);
        PostTime = "";// item.PostedAt.ToString("HH:mm:ss");
        Info = $"ギフト（{item.ItemName}）";
    }
    public McvNicoCommentViewModelV2(INicoConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public McvNicoCommentViewModelV2(INicoDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
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
