using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class DesignTimeMainViewPluginOptions : IMainViewPluginOptions
{
    public FontFamily FontFamily { get; set; } = new FontFamily("Segoe UI");
    public FontStyle FontStyle { get; set; } = FontStyles.Normal;
    public FontWeight FontWeight { get; set; } = FontWeights.Normal;
    public int FontSize { get; set; } = 20;
    public FontFamily FirstCommentFontFamily { get; set; } = new FontFamily("Segoe UI");
    public FontStyle FirstCommentFontStyle { get; set; } = FontStyles.Normal;
    public FontWeight FirstCommentFontWeight { get; set; } = FontWeights.Normal;
    public int FirstCommentFontSize { get; set; } = 20;
    public Color FirstCommentBackColor { get; set; } = Colors.White;
    public Color FirstCommentForeColor { get; set; } = Colors.Black;
    public Color BackColor { get; set; } = Colors.White;
    public Color ForeColor { get; set; } = Colors.Black;
    public Color HorizontalGridLineColor { get; set; }
    public Color VerticalGridLineColor { get; set; }
    public Color InfoForeColor { get; set; }
    public Color InfoBackColor { get; set; }
    public Color BroadcastInfoBackColor { get; set; }
    public Color BroadcastInfoForeColor { get; set; }
    public Color SelectedRowBackColor { get; set; }
    public Color SelectedRowForeColor { get; set; }
    public bool IsUserNameWrapping { get; set; }
    public string SettingsDirPath { get; set; } = "";
    public bool IsActiveCountEnabled { get; set; }
    public int ActiveCountIntervalSec { get; set; }
    public int ActiveMeasureSpanMin { get; set; }
    public double MainViewHeight { get; set; }
    public double MainViewWidth { get; set; }
    public double MainViewLeft { get; set; }
    public double MainViewTop { get; set; }
    public double ConnectionViewHeight { get; set; }
    public double MetadataViewHeight { get; set; }
    public bool IsShowMetaConnectionName { get; set; }
    public int MetadataViewConnectionNameDisplayIndex { get; set; }
    public bool IsShowMetaTitle { get; set; }
    public int MetadataViewTitleDisplayIndex { get; set; }
    public bool IsShowMetaElapse { get; set; }
    public int MetadataViewElapsedDisplayIndex { get; set; }
    public bool IsShowMetaCurrentViewers { get; set; }
    public int MetadataViewCurrentViewersDisplayIndex { get; set; }
    public bool IsShowMetaTotalViewers { get; set; }
    public int MetadataViewTotalViewersDisplayIndex { get; set; }
    public bool IsShowMetaActive { get; set; }
    public int MetadataViewActiveDisplayIndex { get; set; }
    public bool IsShowMetaOthers { get; set; }
    public int MetadataViewOthersDisplayIndex { get; set; }
    public double ConnectionNameWidth { get; set; }
    public bool IsShowConnectionName { get; set; }
    public int ConnectionNameDisplayIndex { get; set; }
    public double ThumbnailWidth { get; set; }
    public int ThumbnailDisplayIndex { get; set; }
    public bool IsShowThumbnail { get; set; }
    public double CommentIdWidth { get; set; }
    public int CommentIdDisplayIndex { get; set; }
    public bool IsShowCommentId { get; set; }
    public double UsernameWidth { get; set; }
    public bool IsShowUsername { get; set; }
    public int UsernameDisplayIndex { get; set; }
    public double MessageWidth { get; set; }
    public bool IsShowMessage { get; set; }
    public int MessageDisplayIndex { get; set; }
    public double PostTimeWidth { get; set; }
    public bool IsShowPostTime { get; set; }
    public int PostTimeDisplayIndex { get; set; }
    public double InfoWidth { get; set; }
    public bool IsShowInfo { get; set; }
    public int InfoDisplayIndex { get; set; }
    public bool IsShowVerticalGridLine { get; set; }
    public bool IsShowHorizontalGridLine { get; set; }
    public double MetadataViewConnectionNameColumnWidth { get; set; }
    public double MetadataViewTitleColumnWidth { get; set; }
    public double MetadataViewElapsedColumnWidth { get; set; }
    public double MetadataViewCurrentViewersColumnWidth { get; set; }
    public double MetadataViewTotalViewersColumnWidth { get; set; }
    public double MetadataViewActiveColumnWidth { get; set; }
    public double MetadataViewOthersColumnWidth { get; set; }
    public Color TitleForeColor { get; set; }
    public Color TitleBackColor { get; set; }
    public Color ViewBackColor { get; set; }
    public Color WindowBorderColor { get; set; }
    public Color SystemButtonForeColor { get; set; }
    public Color SystemButtonBackColor { get; set; }
    public Color SystemButtonBorderColor { get; set; }
    public Color SystemButtonMouseOverBackColor { get; set; }
    public Color SystemButtonMouseOverForeColor { get; set; }
    public Color SystemButtonMouseOverBorderColor { get; set; }
    public Color MenuBackColor { get; set; }
    public Color MenuForeColor { get; set; }
    public Color MenuItemCheckMarkColor { get; set; }
    public Color MenuItemMouseOverBackColor { get; set; }
    public Color MenuItemMouseOverForeColor { get; set; }
    public Color MenuItemMouseOverBorderColor { get; set; }
    public Color MenuItemMouseOverCheckMarkColor { get; set; }
    public Color MenuSeparatorBackColor { get; set; }
    public Color MenuPopupBorderColor { get; set; }
    public Color ButtonBackColor { get; set; }
    public Color ButtonForeColor { get; set; }
    public Color ButtonBorderColor { get; set; }
    public Color CommentListBackColor { get; set; }
    public Color CommentListHeaderBackColor { get; set; }
    public Color CommentListHeaderForeColor { get; set; }
    public Color CommentListHeaderBorderColor { get; set; }
    public Color CommentListBorderColor { get; set; }
    public Color CommentListSeparatorColor { get; set; }
    public Color ConnectionListBackColor { get; set; }
    public Color ConnectionListHeaderBackColor { get; set; }
    public Color ConnectionListHeaderForeColor { get; set; }
    public Color ConnectionListRowBackColor { get; set; }
    public Color ScrollBarBackColor { get; set; }
    public Color ScrollBarBorderColor { get; set; }
    public Color ScrollBarThumbBackColor { get; set; }
    public Color ScrollBarThumbMouseOverBackColor { get; set; }
    public Color ScrollBarThumbPressedBackColor { get; set; }
    public Color ScrollBarButtonBackColor { get; set; }
    public Color ScrollBarButtonForeColor { get; set; }
    public Color ScrollBarButtonBorderColor { get; set; }
    public Color ScrollBarButtonDisabledBackColor { get; set; }
    public Color ScrollBarButtonDisabledForeColor { get; set; }
    public Color ScrollBarButtonDisabledBorderColor { get; set; }
    public Color ScrollBarButtonMouseOverBackColor { get; set; }
    public Color ScrollBarButtonMouseOverForeColor { get; set; }
    public Color ScrollBarButtonMouseOverBorderColor { get; set; }
    public Color ScrollBarButtonPressedBackColor { get; set; }
    public Color ScrollBarButtonPressedForeColor { get; set; }
    public Color ScrollBarButtonPressedBorderColor { get; set; }
    public bool IsAutoCheckIfUpdateExists { get; set; }
    public bool IsAddingNewCommentTop { get; set; }
    public bool IsTopmost { get; set; }
    public bool IsPixelScrolling { get; set; }
    public bool IsEnabledSiteConnectionColor { get; set; }
    public SiteConnectionColorType SiteConnectionColorType { get; set; }
    public Color YouTubeLiveBackColor { get; set; }
    public Color YouTubeLiveForeColor { get; set; }
    public Color OpenrecBackColor { get; set; }
    public Color OpenrecForeColor { get; set; }
    public Color MixchBackColor { get; set; }
    public Color MixchForeColor { get; set; }
    public Color TwitchBackColor { get; set; }
    public Color TwitchForeColor { get; set; }
    public Color NicoLiveBackColor { get; set; }
    public Color NicoLiveForeColor { get; set; }
    public Color TwicasBackColor { get; set; }
    public Color TwicasForeColor { get; set; }
    public Color LineLiveBackColor { get; set; }
    public Color LineLiveForeColor { get; set; }
    public Color WhowatchBackColor { get; set; }
    public Color WhowatchForeColor { get; set; }
    public Color MirrativBackColor { get; set; }
    public Color MirrativForeColor { get; set; }
    public Color PeriscopeBackColor { get; set; }
    public Color PeriscopeForeColor { get; set; }
    public Color ShowRoomBackColor { get; set; }
    public Color ShowRoomForeColor { get; set; }
    public Color BigoBackColor { get; set; }
    public Color BigoForeColor { get; set; }
    public InfoType ShowingInfoLevel { get; set; }
    public int ConnectionsViewSelectionDisplayIndex { get; set; }
    public double ConnectionsViewSelectionWidth { get; set; }
    public bool IsShowConnectionsViewSelection { get; set; }
    public int ConnectionsViewSiteDisplayIndex { get; set; }
    public double ConnectionsViewSiteWidth { get; set; }
    public bool IsShowConnectionsViewSite { get; set; }
    public int ConnectionsViewConnectionNameDisplayIndex { get; set; }
    public double ConnectionsViewConnectionNameWidth { get; set; }
    public bool IsShowConnectionsViewConnectionName { get; set; }
    public int ConnectionsViewInputDisplayIndex { get; set; }
    public double ConnectionsViewInputWidth { get; set; }
    public bool IsShowConnectionsViewInput { get; set; }
    public int ConnectionsViewBrowserDisplayIndex { get; set; }
    public double ConnectionsViewBrowserWidth { get; set; }
    public bool IsShowConnectionsViewBrowser { get; set; }
    public int ConnectionsViewConnectionDisplayIndex { get; set; }
    public double ConnectionsViewConnectionWidth { get; set; }
    public bool IsShowConnectionsViewConnection { get; set; }
    public int ConnectionsViewDisconnectionDisplayIndex { get; set; }
    public double ConnectionsViewDisconnectionWidth { get; set; }
    public bool IsShowConnectionsViewDisconnection { get; set; }
    public int ConnectionsViewSaveDisplayIndex { get; set; }
    public double ConnectionsViewSaveWidth { get; set; }
    public bool IsShowConnectionsViewSave { get; set; }
    public int ConnectionsViewLoggedinUsernameDisplayIndex { get; set; }
    public double ConnectionsViewLoggedinUsernameWidth { get; set; }
    public bool IsShowConnectionsViewLoggedinUsername { get; set; }
    public int ConnectionsViewConnectionBackgroundDisplayIndex { get; set; }
    public double ConnectionsViewConnectionBackgroundWidth { get; set; }
    public int ConnectionsViewConnectionForegroundDisplayIndex { get; set; }
    public double ConnectionsViewConnectionForegroundWidth { get; set; }
    public double UserInfoViewHeight { get; set; }
    public double UserInfoViewWidth { get; set; }
    public double UserInfoViewLeft { get; set; }
    public double UserInfoViewTop { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IMainViewPluginOptions Clone()
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

    public void Set(IMainViewPluginOptions options)
    {
        throw new NotImplementedException();
    }
}
