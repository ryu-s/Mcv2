using System.ComponentModel;
using System.Windows.Media;
using TwicasSitePlugin;

namespace Mcv.MainViewPlugin;
class TwicasCommentViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    private readonly ITwicasMessage _message;
    public TwicasCommentViewModel(ITwicasComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
         : base(connName, options, user)
    {
        _message = comment;

        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.UserName);
        MessageItems = comment.CommentItems;
        Thumbnail = comment.UserIcon;
        Id = comment.Id?.ToString();
        PostTime = comment.PostTime;
    }
    public TwicasCommentViewModel(ITwicasItem item, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
          : base(connName, options, user)
    {
        _message = item;

        _nameItems = Common.MessagePartFactory.CreateMessageItems(item.UserName);
        MessageItems = item.CommentItems;
        Thumbnail = item.UserIcon;
        Id = null;
        PostTime = null;// comment.PostTime;
        Info = item.ItemName;
    }
    public TwicasCommentViewModel(ITwicasConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
        : base(connName, options, user)
    {
        _message = connected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public TwicasCommentViewModel(ITwicasDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
            : base(connName, options, user)
    {
        _message = disconnected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(disconnected.Text);
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.TwicasForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.TwicasBackColor);
    }
}
