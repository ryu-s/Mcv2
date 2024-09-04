using CommunityToolkit.Mvvm.Messaging;
using Mcv.MainViewPlugin;
using System.Windows;

namespace Mcv.MainViewPlugin;
/// <summary>
/// Interaction logic for UsingLatest.xaml
/// </summary>
public partial class UsingLatestView : Window
{
    public UsingLatestView()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<CloseUsingLatestViewMessage>(this, (_, _) =>
        {
            Close();
        });
    }
}
