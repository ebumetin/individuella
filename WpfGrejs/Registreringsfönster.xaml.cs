using System;
using System.Windows;
using BCrypt.Net;
using WpfGrejs.Utils;

namespace WpfGrejs
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }
        
        private async void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Registrering påbörjad...");

            var username = UsernameTextBox.Text.ToString();
            var password = PasswordBox.Password.ToString();
            var confirmPassword = ConfirmPasswordBox.Password.ToString();

            // Validera att fälten inte är tomma
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Alla fält måste fyllas i.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Lösenorden matchar inte.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Hasha lösenordet
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var client = new DbClient();

            try
            {
                int result = await client.CreateUser(username, hashedPassword);

                if (result > 0)
                {
                    Console.WriteLine("Registrering lyckades!");
                    MessageBox.Show("Registrering lyckades!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else if (result == -1)
                {
                    Console.WriteLine("Användarnamnet är redan registrerat.");
                    MessageBox.Show("Användarnamnet är redan registrerat.", "Fel", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    Console.WriteLine("Kunde inte registrera användaren.");
                    MessageBox.Show("Ett fel uppstod vid registrering. Försök igen.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid registrering: {ex.Message}");
                MessageBox.Show($"Fel vid registrering: {ex.Message}", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}