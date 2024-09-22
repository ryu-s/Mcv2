using Mcv.PluginV2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Mcv.MainViewPlugin;

class ConnectionsViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();

    public void AddConnection(ConnectionViewModel connVm)
    {
        Connections.Add(connVm);
    }
    public void RemoveConnection(ConnectionViewModel connVm)
    {
        Connections.Remove(connVm);
    }
    public List<ConnectionId> GetSelectedConnections()
    {
        return Connections.Where(c => c.IsSelected).Select(c => c.Id).ToList();
    }
    private readonly IMainViewPluginOptions _options;
    #region ConnectionsView
    #region ConnectionsViewSelection
    public int ConnectionsViewSelectionDisplayIndex
    {
        get { return _options.ConnectionsViewSelectionDisplayIndex; }
        set { _options.ConnectionsViewSelectionDisplayIndex = value; }
    }
    public double ConnectionsViewSelectionWidth
    {
        get { return _options.ConnectionsViewSelectionWidth; }
        set { _options.ConnectionsViewSelectionWidth = value; }
    }
    public bool IsShowConnectionsViewSelection
    {
        get { return _options.IsShowConnectionsViewSelection; }
        set { _options.IsShowConnectionsViewSelection = value; }
    }
    #endregion
    #region ConnectionsViewSite
    public int ConnectionsViewSiteDisplayIndex
    {
        get { return _options.ConnectionsViewSiteDisplayIndex; }
        set { _options.ConnectionsViewSiteDisplayIndex = value; }
    }
    public double ConnectionsViewSiteWidth
    {
        get { return _options.ConnectionsViewSiteWidth; }
        set { _options.ConnectionsViewSiteWidth = value; }
    }
    public bool IsShowConnectionsViewSite
    {
        get { return _options.IsShowConnectionsViewSite; }
        set { _options.IsShowConnectionsViewSite = value; }
    }
    #endregion
    #region ConnectionsViewConnectionName
    public int ConnectionsViewConnectionNameDisplayIndex
    {
        get { return _options.ConnectionsViewConnectionNameDisplayIndex; }
        set { _options.ConnectionsViewConnectionNameDisplayIndex = value; }
    }
    public double ConnectionsViewConnectionNameWidth
    {
        get { return _options.ConnectionsViewConnectionNameWidth; }
        set { _options.ConnectionsViewConnectionNameWidth = value; }
    }
    public bool IsShowConnectionsViewConnectionName
    {
        get { return _options.IsShowConnectionsViewConnectionName; }
        set { _options.IsShowConnectionsViewConnectionName = value; }
    }
    #endregion
    #region ConnectionsViewInput
    public int ConnectionsViewInputDisplayIndex
    {
        get { return _options.ConnectionsViewInputDisplayIndex; }
        set { _options.ConnectionsViewInputDisplayIndex = value; }
    }
    public double ConnectionsViewInputWidth
    {
        get { return _options.ConnectionsViewInputWidth; }
        set { _options.ConnectionsViewInputWidth = value; }
    }
    public bool IsShowConnectionsViewInput
    {
        get { return _options.IsShowConnectionsViewInput; }
        set { _options.IsShowConnectionsViewInput = value; }
    }
    #endregion
    #region ConnectionsViewBrowser
    public int ConnectionsViewBrowserDisplayIndex
    {
        get { return _options.ConnectionsViewBrowserDisplayIndex; }
        set { _options.ConnectionsViewBrowserDisplayIndex = value; }
    }
    public double ConnectionsViewBrowserWidth
    {
        get { return _options.ConnectionsViewBrowserWidth; }
        set { _options.ConnectionsViewBrowserWidth = value; }
    }
    public bool IsShowConnectionsViewBrowser
    {
        get { return _options.IsShowConnectionsViewBrowser; }
        set { _options.IsShowConnectionsViewBrowser = value; }
    }
    #endregion
    #region ConnectionsViewConnection
    public int ConnectionsViewConnectionDisplayIndex
    {
        get { return _options.ConnectionsViewConnectionDisplayIndex; }
        set { _options.ConnectionsViewConnectionDisplayIndex = value; }
    }
    public double ConnectionsViewConnectionWidth
    {
        get { return _options.ConnectionsViewConnectionWidth; }
        set { _options.ConnectionsViewConnectionWidth = value; }
    }
    public bool IsShowConnectionsViewConnection
    {
        get { return _options.IsShowConnectionsViewConnection; }
        set { _options.IsShowConnectionsViewConnection = value; }
    }
    #endregion
    #region ConnectionsViewDisconnection
    public int ConnectionsViewDisconnectionDisplayIndex
    {
        get { return _options.ConnectionsViewDisconnectionDisplayIndex; }
        set { _options.ConnectionsViewDisconnectionDisplayIndex = value; }
    }
    public double ConnectionsViewDisconnectionWidth
    {
        get { return _options.ConnectionsViewDisconnectionWidth; }
        set { _options.ConnectionsViewDisconnectionWidth = value; }
    }
    public bool IsShowConnectionsViewDisconnection
    {
        get { return _options.IsShowConnectionsViewDisconnection; }
        set { _options.IsShowConnectionsViewDisconnection = value; }
    }
    #endregion
    #region ConnectionsViewSave
    public int ConnectionsViewSaveDisplayIndex
    {
        get { return _options.ConnectionsViewSaveDisplayIndex; }
        set { _options.ConnectionsViewSaveDisplayIndex = value; }
    }
    public double ConnectionsViewSaveWidth
    {
        get { return _options.ConnectionsViewSaveWidth; }
        set { _options.ConnectionsViewSaveWidth = value; }
    }
    public bool IsShowConnectionsViewSave
    {
        get { return _options.IsShowConnectionsViewSave; }
        set { _options.IsShowConnectionsViewSave = value; }
    }
    #endregion
    #region ConnectionsViewLoggedinUsername
    public int ConnectionsViewLoggedinUsernameDisplayIndex
    {
        get { return _options.ConnectionsViewLoggedinUsernameDisplayIndex; }
        set { _options.ConnectionsViewLoggedinUsernameDisplayIndex = value; }
    }
    public double ConnectionsViewLoggedinUsernameWidth
    {
        get { return _options.ConnectionsViewLoggedinUsernameWidth; }
        set { _options.ConnectionsViewLoggedinUsernameWidth = value; }
    }
    public bool IsShowConnectionsViewLoggedinUsername
    {
        get { return _options.IsShowConnectionsViewLoggedinUsername; }
        set { _options.IsShowConnectionsViewLoggedinUsername = value; }
    }
    #endregion
    #region ConnectionsViewConnectionBackground
    public int ConnectionsViewConnectionBackgroundDisplayIndex
    {
        get { return _options.ConnectionsViewConnectionBackgroundDisplayIndex; }
        set { _options.ConnectionsViewConnectionBackgroundDisplayIndex = value; }
    }
    public double ConnectionsViewConnectionBackgroundWidth
    {
        get { return _options.ConnectionsViewConnectionBackgroundWidth; }
        set { _options.ConnectionsViewConnectionBackgroundWidth = value; }
    }
    public bool IsShowConnectionsViewConnectionBackground
    {
        get { return _options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection; }
    }
    #endregion
    #region ConnectionsViewConnectionForeground
    public int ConnectionsViewConnectionForegroundDisplayIndex
    {
        get { return _options.ConnectionsViewConnectionForegroundDisplayIndex; }
        set { _options.ConnectionsViewConnectionForegroundDisplayIndex = value; }
    }
    public double ConnectionsViewConnectionForegroundWidth
    {
        get { return _options.ConnectionsViewConnectionForegroundWidth; }
        set { _options.ConnectionsViewConnectionForegroundWidth = value; }
    }
    public bool IsShowConnectionsViewConnectionForeground
    {
        get { return _options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection; }
    }
    #endregion
    #endregion

    public double ConnectionColorColumnWidth
    {
        get
        {
            if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            {
                return 100;
            }
            else
            {
                return 0;
            }
        }
    }
    public System.Windows.Controls.DataGridGridLinesVisibility GridLinesVisibility
    {
        get
        {
            if (_options.IsShowHorizontalGridLine && _options.IsShowVerticalGridLine)
                return System.Windows.Controls.DataGridGridLinesVisibility.All;
            else if (_options.IsShowHorizontalGridLine)
                return System.Windows.Controls.DataGridGridLinesVisibility.Horizontal;
            else if (_options.IsShowVerticalGridLine)
                return System.Windows.Controls.DataGridGridLinesVisibility.Vertical;
            else
                return System.Windows.Controls.DataGridGridLinesVisibility.None;
        }
    }
    public ConnectionsViewModel(IMainViewPluginOptions options)
    {
        _options = options;
        options.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(_options.IsEnabledSiteConnectionColor):
                case nameof(_options.SiteConnectionColorType):
                    RaisePropertyChanged(nameof(IsShowConnectionsViewConnectionBackground));
                    RaisePropertyChanged(nameof(IsShowConnectionsViewConnectionForeground));
                    RaisePropertyChanged(nameof(ConnectionColorColumnWidth));
                    break;
            }
        };
    }
}
