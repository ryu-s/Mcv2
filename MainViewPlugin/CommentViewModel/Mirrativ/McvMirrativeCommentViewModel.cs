using Mcv.PluginV2;
using MirrativSitePlugin;
using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class McvMirrativCommentViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    public McvMirrativCommentViewModel(IMirrativComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
      : base(connName, options, user)
    {
        _nameItems = MessagePartFactory.CreateMessageItems(comment.UserName);
        MessageItems = MessagePartFactory.CreateMessageItems(comment.Text);
        Thumbnail = null;
        Id = comment.Id;
        PostTime = comment.PostedAt.ToString("HH:mm:ss");
    }
    public McvMirrativCommentViewModel(MirrativSitePlugin.IMirrativJoinRoom item, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
              : base(connName, options, user)
    {
        _nameItems = MessagePartFactory.CreateMessageItems(item.UserName);
        MessageItems = MessagePartFactory.CreateMessageItems(item.Text);
        Thumbnail = item.UserIcon;
        Id = null;
        PostTime = item.PostTime;
    }
    public McvMirrativCommentViewModel(MirrativSitePlugin.IMirrativItem item, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
              : base(connName, options, user)
    {
        _nameItems = MessagePartFactory.CreateMessageItems(item.UserName);
        MessageItems = MessagePartFactory.CreateMessageItems(item.Text);
        Thumbnail = null;
        Id = item.Id;
        PostTime = item.PostedAt.ToString("HH:mm:ss");
    }
    public McvMirrativCommentViewModel(MirrativSitePlugin.IMirrativConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
       : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public McvMirrativCommentViewModel(MirrativSitePlugin.IMirrativDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
      : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems(disconnected.Text);
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.MirrativForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.MirrativBackColor);
    }
}
