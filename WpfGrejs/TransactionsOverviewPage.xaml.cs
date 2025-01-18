using System.Windows;
using System.Windows.Controls;
using WpfGrejs.Models;
using WpfGrejs.ViewModel;

namespace WpfGrejs;

public partial class TransactionsOverviewPage : Page
{

    private readonly MainViewModel _viewModel;
    public TransactionsOverviewPage(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        this.DataContext = _viewModel;
        InitializeComponent();
        TransactionList.ItemsSource = _viewModel.Transactions;
        UsernameLbl.Content = _viewModel.CurrentUser?.Username;
        _viewModel.Transactions.CollectionChanged += (s, e) => RefreshUi();
        _viewModel.GetTransactions();
    }
    
    private void AddTransaction_OnClick(object sender, RoutedEventArgs e)
    {
        //--
        var amount = double.Parse(AmountTxt.Text);
        var description = DescriptionTxt.Text;
        var date = DatePicker.SelectedDate ?? DateTime.Now;
        _viewModel.AddTransaction(amount, description, date);
    }

    private void RefreshUi()
    {
        TransactionList.ItemsSource = _viewModel.Transactions;
        BalanceLbl.Content = _viewModel.Balance;

        AmountTxt.Text = "";
        DescriptionTxt.Text = "";
        DatePicker.SelectedDate = null;

    }

    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Get the button that was clicked
        var button = sender as Button;
        if (button?.CommandParameter is Transaction transaction)
        {
            _viewModel.DeleteTransaction(transaction.Id);
        }
    }

    private void GoToFilter_OnClick(object sender, RoutedEventArgs e)
    {
        // Access the MainFrame in MainWindow and navigate to AdditionalDetailsPage
        if (Window.GetWindow(this) is MainWindow mainWindow)
        {
            mainWindow.MainFrame.Navigate(new TransactionsFiltersPage(_viewModel));
        }
    }

    private void Signout_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentUser = null;
        new LoginWindow().Show();
        Window.GetWindow(this)?.Close();
    }
}