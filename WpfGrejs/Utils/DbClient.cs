using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using WpfGrejs.Models;

namespace WpfGrejs.Utils;

public class DbClient
{
    private const string ConnString = "Server=127.0.0.1;Port=5432;Database=individuella;User Id=postgres;Password=password;";

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

    public async Task<int?> AddTransaction(Transaction transaction)
    {
        var conn = await GetConnection();
                     
        await using (var cmd = new NpgsqlCommand(
                         "INSERT INTO transactions (userid, amount, description, transactiondate) VALUES (@userid, @amount, @description, @transactiondate) RETURNING transactionid",
                         conn))
        {
            cmd.Parameters.AddWithValue("userid", transaction.UserId);
            cmd.Parameters.AddWithValue("amount", transaction.Amount);
            cmd.Parameters.AddWithValue("description", transaction.Description);
            cmd.Parameters.AddWithValue("transactiondate", transaction.TransactionDate);

            var newId = await cmd.ExecuteScalarAsync();
            return (int)newId!;
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
        try
        {
            var conn = await GetConnection(); // Skapa anslutning till databasen
            try
            {
                await using (var cmd = new NpgsqlCommand(
                                 "INSERT INTO users (username, hashedpassword) VALUES (@username, @hashedpassword)", conn))
                {
                    //cmd.Parameters.AddWithValue("username", username); // Lägg till användarnamn som parameter
                    //cmd.Parameters.AddWithValue("hashedpassword", hashedPassword); // Lägg till hashat lösenord som parameter
                
                    cmd.Parameters.Add("username", NpgsqlDbType.Text).Value = username;
                    cmd.Parameters.Add("hashedpassword", NpgsqlDbType.Text).Value = hashedPassword;
                
                    return await cmd.ExecuteNonQueryAsync(); // Kör kommandot och returnera nya id:t
                }
            } catch (Exception e) {
                Console.WriteLine($"DBError: {e.Message}");
                throw;
            }
            
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // Unik constraint-överträdelse
        {
            Console.WriteLine("Användarnamnet är redan registrerat.");
            return -1; // Indikerar att användarnamnet redan finns
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid registrering: {ex.Message}");
            return 0; // Indikerar att ett generellt fel inträffade
        }
    }

    
    public async Task<string?> GetPasswordHash(string username)
    {
        try
        {
            var conn = await GetConnection();
            await using (var cmd = new NpgsqlCommand(
                             "SELECT hashedpassword FROM users WHERE username = @username::text", conn)) // Lägg till "::text"
            {
                cmd.Parameters.AddWithValue("username", username);
                Console.WriteLine($"Executing SQL: SELECT hashedpassword FROM users WHERE username = '{username}'");
                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid hämtning av lösenord: {ex.Message}");
            return null;
        }
    }

    public async Task<User?> GetUser(string username)
    {
        try
        {
            var conn = await GetConnection();
            await using (var cmd = new NpgsqlCommand(
                             "SELECT * FROM users WHERE username = @username", conn)) // Lägg till "::text"
            {
                cmd.Parameters.AddWithValue("username", username);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User()
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                        };
                    }
                    return null;
                }
            }

        }
        catch (Exception e)
        {
            Console.WriteLine($"Fel vid hämtning av lösenord: {e.Message}");
            return null;
        }
    }
}
