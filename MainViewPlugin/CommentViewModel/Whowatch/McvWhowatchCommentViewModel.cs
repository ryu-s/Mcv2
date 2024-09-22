using Mcv.PluginV2;
using System;
using System.ComponentModel;
using System.Windows.Media;
using WhowatchSitePlugin;

namespace Mcv.MainViewPlugin;
class McvWhowatchCommentViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    private readonly WhowatchSitePlugin.IWhowatchMessage _message;
    public McvWhowatchCommentViewModel(IWhowatchComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
   : base(connName, options, user)
    {
        _message = comment;

        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.UserName);
        MessageItems = Common.MessagePartFactory.CreateMessageItems(comment.Comment);
        Thumbnail = comment.UserIcon;
        Id = comment.Id;
        PostTime = comment.PostTime;
    }
    public McvWhowatchCommentViewModel(IWhowatchItem item, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
       : base(connName, options, user)
    {
        var comment = item;
        _message = comment;

        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.UserName);
        MessageItems = Common.MessagePartFactory.CreateMessageItems(comment.Comment);
        Thumbnail = new MessageImage
        {
            Url = comment.UserIconUrl,
            Alt = "",
            Height = 40,//_optionsにcolumnの幅を動的に入れて、ここで反映させたい。propertyChangedはどうやって発生させるか
            Width = 40,
        };
        Id = comment.Id.ToString();
        PostTime = UnixtimeToDateTime(comment.PostedAt / 1000).ToString("HH:mm:ss");
        Info = item.ItemCount == 1 ? item.ItemName : $"{item.ItemName} × {item.ItemCount}";
    }
    public McvWhowatchCommentViewModel(IWhowatchConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    : base(connName, options, user)
    {
        _message = connected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public McvWhowatchCommentViewModel(IWhowatchDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
      : base(connName, options, user)
    {
        _message = disconnected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(disconnected.Text);
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.WhowatchForeColor);
    }
    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.WhowatchBackColor);
    }
    private static DateTime UnixtimeToDateTime(long unixTimeStamp)
    {
        var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        return dt.AddSeconds(unixTimeStamp).ToLocalTime();
    }
}
