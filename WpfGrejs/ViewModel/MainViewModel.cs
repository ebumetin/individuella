using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using WpfGrejs.Models;

namespace WpfGrejs.ViewModel;

public class MainViewModel : INotifyCollectionChanged, INotifyPropertyChanged
{

    private static MainViewModel? _instance;

    public bool FilterView { get; set; } = false;
    
    private MainViewModel()
    {
        Transactions.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Balance));
    }
    
    public static MainViewModel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MainViewModel();
            }

            return _instance;
        }
    }
    
    public ObservableCollection<Transaction> Transactions { get; } = new()
    {
        new Transaction { Id = 1, Amount = 100, TransactionDate = new DateTime(2022, 7, 2), Description = "Mat" },
        new Transaction { Id = 2, Amount = 200, TransactionDate = new DateTime(2023, 5, 3), Description = "Kläder" },
        new Transaction { Id = 3, Amount = 100, TransactionDate = new DateTime(2023, 5, 4), Description = "Mat" },
        new Transaction { Id = 4, Amount = 200, TransactionDate = new DateTime(2023, 5, 5), Description = "Kläder" },
        new Transaction { Id = 5, Amount = 100, TransactionDate = new DateTime(2023, 5, 6), Description = "Mat" },
        new Transaction { Id = 6, Amount = 200, TransactionDate = new DateTime(2024, 1, 1), Description = "Kläder" },
        new Transaction { Id = 7, Amount = 100, TransactionDate = new DateTime(2024, 3, 1), Description = "Mat" },
        new Transaction { Id = 8, Amount = 200, TransactionDate = new DateTime(2024, 3, 1), Description = "Kläder" }
    };

    public double Balance => Transactions.Sum(t => t.Amount);

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
            Console.WriteLine(transactionsFilter.Count);
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
            Console.WriteLine(transactionsFilter.Count);
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
            Console.WriteLine(transactionsFilter.Count);
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
            Console.WriteLine(transactionsFilter.Count);
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


        Console.WriteLine(transactionsFilter.Count);
        return transactionsFilter;
    }

    public void RefreshTransactions()
    {
        OnPropertyChanged(nameof(Transactions));
        OnPropertyChanged(nameof(FilteredTransactions));
    }

    //--
    public void AddTransaction(double amount, string description, DateTime date)
    {
        var nextId = Transactions.Select(t => t.Id).Max() + 1;
        Transactions.Add(new Transaction
        {
            Id = nextId,
            Amount = amount,
            TransactionDate = date,
            Description = description
        });
    }

    //--
    public void DeleteTransaction(int id)
    {
        var transaction = Transactions.FirstOrDefault(t => t.Id == id);
        if (transaction != null)
        {
            Transactions.Remove(transaction);
        }
        else
        {
            Console.WriteLine($"Transaction {id} not found");
        }
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

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
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