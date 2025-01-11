namespace WpfGrejs.Models;

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public double Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TransactionsFilter
{
    public int Id { get; set; }
    public double Amount { get; set; }
    public string Period { get; set; } = string.Empty;
    public int Transactions { get; set; }
}