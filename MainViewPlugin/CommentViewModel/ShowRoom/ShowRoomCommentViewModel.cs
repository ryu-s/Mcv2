using ShowRoomSitePlugin;
using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;
class ShowRoomCommentViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    private readonly IShowRoomMessage _message;
    public ShowRoomCommentViewModel(IShowRoomComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
           : base(connName, options, user)
    {
        _message = comment;

        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.UserName);
        MessageItems = Common.MessagePartFactory.CreateMessageItems(comment.Text);
        Thumbnail = null;
        Id = null;
        PostTime = comment.PostedAt.ToString("HH:mm:ss");
    }
    public ShowRoomCommentViewModel(IShowRoomJoin join, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
           : base(connName, options, user)
    {
        _message = join;

        //_nameItems = join.NameItems;
        //MessageItems = join.CommentItems;
        //Thumbnail = join..UserIcon;
        //Id = join.Id.ToString();
        //PostTime = join.PostTime;
    }
    public ShowRoomCommentViewModel(IShowRoomLeave leave, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
               : base(connName, options, user)
    {
        _message = leave;

        //_nameItems = leave.NameItems;
        //MessageItems = leave.CommentItems;
        //Thumbnail = join..UserIcon;
        //Id = join.Id.ToString();
        //PostTime = join.PostTime;
    }
    //public ShowRoomCommentViewModel(IShowRoomItem item, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionStatus)
    //    : this(metadata, methods, connectionStatus)
    //{
    //    var comment = item;
    //    _message = comment;

    //    _nameItems = comment.NameItems;
    //    MessageItems = comment.CommentItems;
    //    Thumbnail = new Common.MessageImage
    //    {
    //        Url = comment.UserIconUrl,
    //        Alt = "",
    //        Height = 40,//_optionsにcolumnの幅を動的に入れて、ここで反映させたい。propertyChangedはどうやって発生させるか
    //        Width = 40,
    //    };
    //    Id = comment.Id.ToString();
    //    PostTime = UnixtimeToDateTime(comment.PostedAt / 1000).ToString("HH:mm:ss");
    //}
    public ShowRoomCommentViewModel(IShowRoomConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
              : base(connName, options, user)
    {
        _message = connected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public ShowRoomCommentViewModel(IShowRoomDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
           : base(connName, options, user)
    {
        _message = disconnected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(disconnected.Text);
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.ShowRoomForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.ShowRoomBackColor);
    }
}