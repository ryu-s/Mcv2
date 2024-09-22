using Mcv.PluginV2;
using OpenrecSitePlugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;
class OpenrecCommentViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    private readonly IOpenrecMessage _message;
    public OpenrecCommentViewModel(IOpenrecComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
           : base(connName, options, user)
    {
        _message = comment;

        _nameItems = comment.NameItems;
        MessageItems = comment.MessageItems;
        Thumbnail = null;
        Id = comment.Id;
        PostTime = comment.PostTime.ToString("HH:mm:ss");
    }
    public OpenrecCommentViewModel(IOpenrecStamp stamp, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
            : base(connName, options, user)
    {
        _message = stamp;

        _nameItems = stamp.NameItems;
        MessageItems = new List<IMessagePart> { stamp.Stamp };
        Thumbnail = stamp.UserIcon;
        Id = stamp.Id.ToString();
        PostTime = stamp.PostTime.ToString("HH:mm:ss");
    }
    public OpenrecCommentViewModel(IOpenrecYell yell, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
        : base(connName, options, user)
    {
        _message = yell;
        //messageItems.Add(MessagePartFactory.CreateMessageText("エールポイント：" + commentData.YellPoints + Environment.NewLine));

        var messageItems = new List<IMessagePart>();
        messageItems.Add(Common.MessagePartFactory.CreateMessageText("エールポイント：" + yell.YellPoints));
        if (yell.Message != null)
        {
            messageItems.Add(Common.MessagePartFactory.CreateMessageText(Environment.NewLine));
            messageItems.Add(Common.MessagePartFactory.CreateMessageText(yell.Message));
        }
        _nameItems = yell.NameItems;
        MessageItems = messageItems;
        Thumbnail = yell.UserIcon;
        Id = yell.Id.ToString();
        PostTime = yell.PostTime.ToString("HH:mm:ss");
    }
    //public OpenrecCommentViewModel(IOpenrecItem item, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionStatus)
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
    public OpenrecCommentViewModel(IOpenrecConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
         : base(connName, options, user)
    {
        _message = connected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public OpenrecCommentViewModel(IOpenrecDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
        : base(connName, options, user)
    {
        _message = disconnected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(disconnected.Text);
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.OpenrecForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.OpenrecBackColor);
    }
}

