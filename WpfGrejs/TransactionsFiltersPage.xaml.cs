using System.Windows;
using System.Windows.Controls;
using WpfGrejs.ViewModel;

namespace WpfGrejs;

public partial class TransactionsFiltersPage : Page
{
    private readonly MainViewModel _viewModel;
    public TransactionsFiltersPage(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        this.DataContext = _viewModel;
        InitializeComponent();
        FilterList.ItemsSource = _viewModel.FilteredTransactions;
        BalanceLbl.Content = _viewModel.Balance;
        FilterTypeComboBox.SelectedIndex = 0;
        PeriodComboBox.SelectedIndex = 1;
    }

    private void GoBackButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Access the MainFrame in MainWindow and navigate to AdditionalDetailsPage
        if (Window.GetWindow(this) is MainWindow mainWindow)
        {
            mainWindow.MainFrame.Navigate(new TransactionsOverviewPage(_viewModel));
        }
    }

    private void PeriodComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem selectedItem)
        {
            var period = selectedItem.Content switch
            {
                "Yearly" => TransactionsFilterPeriod.Yearly,
                "Monthly" => TransactionsFilterPeriod.Monthly,
                "Weekly" => TransactionsFilterPeriod.Weekly,
                "Daily" => TransactionsFilterPeriod.Daily,
                _ => TransactionsFilterPeriod.Monthly,
            };
        
            _viewModel.TransactionsFilterPeriod = period;
            FilterList.ItemsSource = _viewModel.FilteredTransactions;
        }
    }

    private void FilterTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem selectedItem)
        {
            var type = selectedItem.Content switch
            {
                "Income" => TransactionsFilterType.Income,
                "Expense" => TransactionsFilterType.Expense,
                _ => TransactionsFilterType.Income
            };
            _viewModel.TransactionsFilterType = type;
            FilterList.ItemsSource = _viewModel.FilteredTransactions;
        }
    }
}