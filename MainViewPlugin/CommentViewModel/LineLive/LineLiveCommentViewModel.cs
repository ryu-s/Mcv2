using Mcv.PluginV2;
using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class LineLiveCommentViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    public LineLiveCommentViewModel(LineLiveSitePlugin.ILineLiveComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
           : base(connName, options, user)
    {
        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.DisplayName);
        MessageItems = Common.MessagePartFactory.CreateMessageItems(comment.Text);
        Thumbnail = new MessageImage()
        {
            Url = comment.UserIconUrl,
            Height = 40,
            Width = 40,
        };
        Id = null;
        PostTime = comment.PostedAt.ToString("HH:mm:ss");
    }
    public LineLiveCommentViewModel(LineLiveSitePlugin.ILineLiveItem item, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
   : base(connName, options, user)
    {
        var comment = item;

        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.DisplayName);
        MessageItems = comment.CommentItems;
        Thumbnail = new MessageImage()
        {
            Url = comment.UserIconUrl,
            Height = 40,//_optionsにcolumnの幅を動的に入れて、ここで反映させたい。propertyChangedはどうやって発生させるか
            Width = 40,
        };
        Id = null;
        PostTime = comment.PostedAt.ToString("HH:mm:ss");
    }
    public LineLiveCommentViewModel(LineLiveSitePlugin.ILineLiveConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
          : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public LineLiveCommentViewModel(LineLiveSitePlugin.ILineLiveDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
   : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems(disconnected.Text);
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.LineLiveForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.LineLiveBackColor);
    }
}
