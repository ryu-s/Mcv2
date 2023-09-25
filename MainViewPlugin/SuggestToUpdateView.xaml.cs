using CommunityToolkit.Mvvm.Messaging;
using System.Windows;

namespace Mcv.MainViewPlugin;
/// <summary>
/// Interaction logic for SuggestToUpdateView.xaml
/// </summary>
public partial class SuggestToUpdateView : Window
{
    public SuggestToUpdateView()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<SuggestToUpdateViewCloseMessage>(this, (_, _) =>
        {
            Close();
        });
    }
}
