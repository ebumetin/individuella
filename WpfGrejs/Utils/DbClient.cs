using Npgsql;
using WpfGrejs.Models;

namespace WpfGrejs.Utils;

public class DbClient
{
    private const string ConnString = "Server=127.0.0.1;Port=5432;Database=postgres;User Id=postgres;Password=password;";

    private async Task<NpgsqlConnection> GetConnection()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnString);
        var dataSource = dataSourceBuilder.Build();

        return await dataSource.OpenConnectionAsync();
    }

    public async Task<List<Transaction>> GetTransactions(int userId)
    {
        var conn = await GetConnection();
        using var cmd = new NpgsqlCommand(
            "SELECT * FROM transactions WHERE userId = @userId"
            , conn);
        cmd.Parameters.AddWithValue("userid", userId);
        using var reader = await cmd.ExecuteReaderAsync();
        var transactions = new List<Transaction>();
        while (await reader.ReadAsync()) {
            transactions.Add(new Transaction
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                Amount = (double) reader.GetDecimal(2),
                TransactionDate = reader.GetDateTime(3),
                Description = reader.GetString(4),
            });
        }

        return transactions;
    }

    public async Task<int> AddTransaction(int userId, double amount, string description, DateTime date)
    {
        var conn = await GetConnection();
                     
        await using (var cmd = new NpgsqlCommand(
                         "INSERT INTO transactions (userid, amount, type, datetime) VALUES (@userid, @amount, @type, @datetime)"
                         , conn))
        {
            cmd.Parameters.AddWithValue("userid", userId);
            cmd.Parameters.AddWithValue("amount", amount);
            cmd.Parameters.AddWithValue("type", description);
            cmd.Parameters.AddWithValue("datetime", date);
            return await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task<int> DeleteTransaction(int id)
    {
        var conn = await GetConnection();
        await using (var cmd = new NpgsqlCommand(
                         "DELETE FROM transactions where transactionid = @id"
                         , conn))
        {
            cmd.Parameters.AddWithValue("id", id);
            return await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task<int> CreateUser(string username, string hashedPassword)
    {
        var conn = await GetConnection();

        await using (var cmd = new NpgsqlCommand(
                         "INSERT INTO users (username, hashedpassword) VALUES (@username, @hashedpassword)", conn))
        {
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("hashedpassword", hashedPassword);
            return await cmd.ExecuteNonQueryAsync();
        }
    }
}