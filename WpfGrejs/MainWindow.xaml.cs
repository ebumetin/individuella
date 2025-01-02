using System.Windows;
using WpfGrejs.ViewModel;

namespace WpfGrejs;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    
    private readonly MainViewModel _viewModel;
    public MainWindow()
    {
        _viewModel = MainViewModel.Instance;
        this.DataContext = _viewModel;
        InitializeComponent();
        
        MainFrame.Navigate(new TransactionsOverviewPage());
    }
    
}