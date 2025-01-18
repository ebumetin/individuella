using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using WpfGrejs.Models;
using WpfGrejs.Utils;

namespace WpfGrejs.ViewModel;

public sealed class MainViewModel : INotifyCollectionChanged, INotifyPropertyChanged
{
    
    private readonly DbClient _dbClient = new DbClient();
    
    public bool FilterView { get; set; } = false;
    public User? CurrentUser { get; set; } = null;

    public void GetTransactions()
    {
        if (CurrentUser == null)
        {
            Console.WriteLine("You are not signed in.");
            return;
        }

        _ = GetTransactionsAsync();
    }

    private async Task GetTransactionsAsync()
    {
        try
        {
            var transactions = await _dbClient.GetTransactions(CurrentUser.Id);
            Console.WriteLine("{0} transactions retrieved.", transactions.Count);

            // Update the collection on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                Transactions.Clear(); // Clear existing items
                foreach (var transaction in transactions)
                {
                    Transactions.Add(transaction); // Add items one by one
                }
                
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving transactions: {ex.Message}");
        }
    }
    
    public void AddTransaction(double amount, string description, DateTime date)
    {
        if (CurrentUser == null)
        {
            Console.WriteLine("You are not signed in.");
            return;
        }
        var transaction = new Transaction
        {
            UserId = CurrentUser.Id,
            Amount = amount,
            TransactionDate = date,
            Description = description
        };

        _ = AddTransactionAsync(transaction);
    }

    private async Task AddTransactionAsync(Transaction transaction)
    {
        try
        {
            var transactionId = await _dbClient.AddTransaction(transaction);
            Console.WriteLine("new transaction id {0}", transactionId);

            if (!transactionId.HasValue)
            {
                Console.WriteLine("Failed to create new transaction.");
                return;
            }
            
            transaction.Id = transactionId.Value;

            // Update the collection on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                Transactions.Add(transaction);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding transaction: {ex.Message}");
        }
    }
    
    public void DeleteTransaction(int id)
    {
        if (CurrentUser == null)
        {
            Console.WriteLine("You are not signed in.");
            return;
        }
        var transaction = Transactions.FirstOrDefault(t => t.Id == id);
        if (transaction != null)
        {
            _ = DeleteTransactionAsync(transaction);
        }
        else
        {
            Console.WriteLine($"Transaction {id} not found");
        }
    }

    private async Task DeleteTransactionAsync(Transaction transaction)
    {
        try
        {
            var rowsAffected = await _dbClient.DeleteTransaction(transaction.Id);
            Console.WriteLine("rows affect {0}", rowsAffected);

            switch (rowsAffected)
            {
                case 0:
                    Console.WriteLine("Failed to delete transaction.");
                    return;
                case > 1:
                    Console.WriteLine("WARNING: Too many rows were deleted.");
                    return;
                default:
                    // Update the collection on the UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Transactions.Remove(transaction);
                    });
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting transaction: {ex.Message}");
        }
    }



    public ObservableCollection<Transaction> Transactions { get; set; } = new();

    public double Balance
    {
        get
        {
            return Transactions.Sum(transaction => transaction.Amount);
        }
    }

    public TransactionsFilterType TransactionsFilterType { get; set; } = TransactionsFilterType.Income; // 1 = Income, 2 = Expense
    public TransactionsFilterPeriod TransactionsFilterPeriod { get; set; } = TransactionsFilterPeriod.Monthly; // 1 = Yearly, 2 = Monthly, 3 = Weekly, 4 = Daily
    public ObservableCollection<TransactionsFilter> FilteredTransactions { get => GetTransactionsFilter(); }
    
    private ObservableCollection<TransactionsFilter> GetTransactionsFilter()
    {
        var transactions = Transactions.Where(t => t.Amount > 0).ToList();
        if (TransactionsFilterType == TransactionsFilterType.Expense)
        {
            transactions = Transactions.Where(t => t.Amount < 0).ToList();
        }

        ObservableCollection<TransactionsFilter> transactionsFilter;

        if (TransactionsFilterPeriod == TransactionsFilterPeriod.Yearly) // Yearly
        {
            transactionsFilter = new ObservableCollection<TransactionsFilter>(transactions
                .GroupBy(t => t.TransactionDate.Year)
                .Select(g => new TransactionsFilter
                {
                    Id = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Period = g.Key.ToString(),
                    Transactions = g.Count()
                }).ToList());
            return transactionsFilter;
        }

        if (TransactionsFilterPeriod == TransactionsFilterPeriod.Monthly) // Monthly
        {
            transactionsFilter = new ObservableCollection<TransactionsFilter>(transactions
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new TransactionsFilter
                {
                    Id = g.Key.Year * 100 + g.Key.Month, // Unique Id for year and month
                    Amount = g.Sum(t => t.Amount), // Sum the amount for each month
                    Period = $"{g.Key.Year}-{g.Key.Month:00}", // Period as Year-Month
                    Transactions = g.Count() // Count the number of transactions for each month
                }).ToList());
            return transactionsFilter;
        }

        if (TransactionsFilterPeriod == TransactionsFilterPeriod.Weekly) // Weekly
        {
            // Return individual transactions within this week
            transactionsFilter = new ObservableCollection<TransactionsFilter>(transactions
                .GroupBy(t => new { year = t.TransactionDate.Year, weeknr = GetWeekNumber(t.TransactionDate) })
                .Select(g =>
                new TransactionsFilter
                {
                    Id = g.Key.year * 100 + g.Key.weeknr,
                    Amount = g.Sum(t => t.Amount),
                    Period = $"{g.Key.year} - Week {g.Key.weeknr:00}",
                    Transactions = g.Count()
                }).ToList());
            return transactionsFilter;
        }

        if (TransactionsFilterPeriod == TransactionsFilterPeriod.Daily) // Daily
        {
        
            // Return individual transactions for today
            transactionsFilter = new ObservableCollection<TransactionsFilter>(transactions
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month, t.TransactionDate.Day })
                .Select(g =>
                new TransactionsFilter
                {
                    Id = g.Key.Year * 10000 + g.Key.Month * 100 + g.Key.Day,
                    Amount = g.Sum(t => t.Amount),
                    Period = $"{g.Key.Year}-{g.Key.Month:00}-{g.Key.Day:00}",
                    Transactions = g.Count()
                }).ToList());
            return transactionsFilter;
        }

        // Default: return all transactions without grouping
        transactionsFilter = new ObservableCollection<TransactionsFilter>(transactions.Select(t =>
            new TransactionsFilter
            {
                Id = t.Id,
                Amount = t.Amount,
                Period = t.TransactionDate.ToString("yyyy-MM-dd")
            }).ToList());
        
        return transactionsFilter;
    }

    
    public int GetWeekNumber(DateTime date)
    {
        // Use the current culture's calendar system
        var calendar = CultureInfo.CurrentCulture.Calendar;
        
        // Define the rule for the first week of the year and the first day of the week
        var calendarWeekRule = CalendarWeekRule.FirstFourDayWeek;
        var firstDayOfWeek = DayOfWeek.Monday; // Change this if your week starts on a different day
        
        // Get the week number for the specified date
        return calendar.GetWeekOfYear(date, calendarWeekRule, firstDayOfWeek);
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public enum TransactionsFilterType
{
    Income = 1,
    Expense = 2
}

public enum TransactionsFilterPeriod
{
    Yearly = 1,
    Monthly = 2,
    Weekly = 3,
    Daily = 4
}