using System.Windows;
using System.Windows.Controls;
using WpfGrejs.Models;
using WpfGrejs.ViewModel;

namespace WpfGrejs;

public partial class TransactionsOverviewPage : Page
{

    private readonly MainViewModel _viewModel;
    public TransactionsOverviewPage()
    {
        _viewModel = MainViewModel.Instance;
        this.DataContext = _viewModel;
        InitializeComponent();
        TransactionList.ItemsSource = _viewModel.Transactions;
        BalanceLbl.Content = _viewModel.Balance;
    }
    
    private void AddTransaction_OnClick(object sender, RoutedEventArgs e)
    {
        //--
        var amount = double.Parse(AmountTxt.Text);
        var description = DescriptionTxt.Text;
        var date = DatePicker.SelectedDate ?? DateTime.Now;
        _viewModel.AddTransaction(amount, description, date);
        RefreshUi();
    }

    private void RefreshUi()
    {
        TransactionList.ItemsSource = _viewModel.Transactions;
        BalanceLbl.Content = _viewModel.Balance;
    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Get the button that was clicked
        var button = sender as Button;
        if (button?.CommandParameter is Transaction transaction)
        {
            _viewModel.DeleteTransaction(transaction.Id);
            RefreshUi();
        }
    }

    private void GoToFilter_OnClick(object sender, RoutedEventArgs e)
    {
        // Access the MainFrame in MainWindow and navigate to AdditionalDetailsPage
        if (Window.GetWindow(this) is MainWindow mainWindow)
        {
            mainWindow.MainFrame.Navigate(new TransactionsFiltersPage());
        }
    }
}