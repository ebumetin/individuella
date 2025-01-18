using System.Windows;
using BCrypt.Net;
using WpfGrejs.Utils;

namespace WpfGrejs;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("Login Pressed");

        var username = UsernameTextBox.Text;
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Användarnamn och lösenord får inte vara tomma.");
            return;
        }

        var client = new DbClient();

        try
        {
            // Hämta lösenordshash från databasen
            var user = await client.GetUser(username);

            if (user == null )
            {
                Console.WriteLine("Användaren finns inte.");
                return;
            }
            var passwordHash = await client.GetPasswordHash(username);
            // Verifiera lösenordet
            if (BCrypt.Net.BCrypt.Verify(password, passwordHash))
            {
                Console.WriteLine("Password Verified - Inloggning lyckades!");
                var mainWindow = new MainWindow(user);
                mainWindow.Show();
                Close();
            }
            else
            {
                Console.WriteLine("Password Not Verified - Ogiltigt lösenord.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid inloggning: {ex.Message}");
        }
    }

    
    void SignupButton_OnClick(object sender, RoutedEventArgs e)
    {
        {
            // Skapa och visa registreringsfönstret
            var registrationWindow = new RegistrationWindow();
            registrationWindow.ShowDialog();
        }
    }
    private async Task Signup(string name, string passwordHash)
    {
        var client = new DbClient();
        var result = await client.CreateUser(name, passwordHash);
        Console.WriteLine(result);
    }
}