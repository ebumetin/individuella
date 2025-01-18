using System.Windows;
using WpfGrejs.Models;
using WpfGrejs.ViewModel;

namespace WpfGrejs;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(User user)
    {
        var viewModel = new MainViewModel
        {
            CurrentUser = user
        };
        this.DataContext = viewModel;
        InitializeComponent();
        MainFrame.Navigate(new TransactionsOverviewPage(viewModel));
    }
    
}