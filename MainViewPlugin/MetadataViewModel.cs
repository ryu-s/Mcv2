using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class MetadataViewModel : ViewModelBase, INotifyPropertyChanged
{
    public MetadataViewModel(ConnectionName connectionName, IMainViewPluginOptions options)
    {
        ConnectionName = connectionName;
        _options = options;
        _title = "";
        _elapsed = "";
        _currentViewers = "";
        _totalViewers = "";
        _active = "";
        _others = "";
        options.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(options.IsEnabledSiteConnectionColor):
                case nameof(options.SiteConnectionColorType):
                    RaisePropertyChanged(nameof(Background));
                    RaisePropertyChanged(nameof(Foreground));
                    break;
            }
        };
        connectionName.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(connectionName.BackColor):
                    RaisePropertyChanged(nameof(Background));
                    break;
                case nameof(connectionName.ForeColor):
                    RaisePropertyChanged(nameof(Foreground));
                    break;
            }
        };
    }
    private string _title;

    public string Title
    {
        get { return _title; }
        set
        {
            _title = value;
            RaisePropertyChanged();
        }

    }
    private string _elapsed;

    public string Elapsed
    {
        get { return _elapsed; }
        set
        {
            _elapsed = value;
            RaisePropertyChanged();
        }

    }
    private string _currentViewers;

    public string CurrentViewers
    {
        get { return _currentViewers; }
        set
        {
            _currentViewers = value;
            RaisePropertyChanged();
        }

    }
    private string _totalViewers;

    public string TotalViewers
    {
        get { return _totalViewers; }
        set
        {
            _totalViewers = value;
            RaisePropertyChanged();
        }

    }
    private string _active;

    public string Active
    {
        get { return _active; }
        set
        {
            _active = value;
            RaisePropertyChanged();
        }

    }
    private string _others;
    private readonly IMainViewPluginOptions _options;

    public string Others
    {
        get { return _others; }
        set
        {
            _others = value;
            RaisePropertyChanged();
        }

    }
    public ConnectionName ConnectionName { get; }
    public Brush Background
    {
        get
        {
            if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            {
                return new SolidColorBrush(ConnectionName.BackColor);
            }
            else
            {
                return new SolidColorBrush(Colors.White);
            }
        }
    }
    public Brush Foreground
    {
        get
        {
            if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            {
                return new SolidColorBrush(ConnectionName.ForeColor);
            }
            else
            {
                return new SolidColorBrush(Colors.Black);
            }
        }
    }
}
