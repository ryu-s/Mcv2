using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
namespace CommentGeneratorPlugin;

sealed class ConfigViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly Options _options;
    public bool IsEnabled
    {
        get { return _options.IsEnabled; }
        set { _options.IsEnabled = value; }
    }
    public string HcgSettingFilePath
    {
        get { return _options.HcgSettingFilePath; }
        set { _options.HcgSettingFilePath = value; }
    }
    public ICommand ShowFilePickerCommand { get; }
    private void ShowFilePicker()
    {
        var fileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "HTML5コメジェネの設定ファイルを選択してください",
            Filter = "設定ファイル | setting.xml"
        };
        var result = fileDialog.ShowDialog();
        if (result == true)
        {
            this.HcgSettingFilePath = fileDialog.FileName;
        }
    }
    public bool IsMirrativJoin
    {
        get => _options.IsMirrativeJoin;
        set => _options.IsMirrativeJoin = value;
    }
    public ConfigViewModel()
    {
        if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
        {
            _options = new Options();
            IsEnabled = true;
            HcgSettingFilePath = "HTML5コメジェネ設定ファイルパス";
            IsMirrativJoin = true;
        }
        else
        {
            throw new NotSupportedException();
        }
    }
    public ConfigViewModel(Options options)
    {
        _options = options;
        _options.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(_options.IsEnabled):
                    RaisePropertyChanged(nameof(IsEnabled));
                    break;
                case nameof(_options.HcgSettingFilePath):
                    RaisePropertyChanged(nameof(HcgSettingFilePath));
                    break;
                case nameof(_options.IsMirrativeJoin):
                    RaisePropertyChanged(nameof(IsMirrativJoin));
                    break;
            }
        };
        ShowFilePickerCommand = new RelayCommand(ShowFilePicker);
    }
}
