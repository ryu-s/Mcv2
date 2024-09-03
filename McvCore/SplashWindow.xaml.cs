using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Mcv.Core.V1;
using Mcv.MainViewPlugin;
using Mcv.PluginV2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Mcv.Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        Dispatcher _dispatcher;
        public SplashWindow()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<RequestCloseSplashWindowMessage>(this, (r, m) =>
            {
                try
                {
                    _dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
        }
    }
    class SplashWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private string _logs;
        public string Logs
        {
            get
            {
                return _logs;
            }
            set
            {
                _logs = value;
                RaisePropertyChanged();
            }
        }
        public SplashWindowViewModel()
        {
            _logs = "";
        }
        public void AddLog(string log)
        {
            var date = DateTime.Now.ToString("HH:mm:ss.fff");
            Logs = date + " " + log + Environment.NewLine + Logs;
        }

        public static void RequestClose()
        {
            WeakReferenceMessenger.Default.Send(new RequestCloseSplashWindowMessage());
        }
    }
    class RequestCloseSplashWindowMessage : RequestMessage<object> { }
}
