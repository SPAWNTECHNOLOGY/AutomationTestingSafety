using System;
using System.Data.SqlClient;
using System.Windows;
using AutomationTestingSafety.Database;

namespace AutomationTestingSafety
{
    public partial class SetUserPasswordWindow : Window
    {
        public string NewPassword { get; private set; }
        private int _userId;

        public SetUserPasswordWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            NewPassword = passwordBox.Password.Trim();
            if (string.IsNullOrEmpty(NewPassword))
            {
                MessageBox.Show("Введите новый пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Обновляем пароль в БД
            try
            {
                using (var connection = new SqlConnection(ConnectionString._connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Пользователи SET Пароль = @password WHERE ID_Пользователя = @id";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@password", NewPassword);
                        command.Parameters.AddWithValue("@id", _userId);
                        command.ExecuteNonQuery();
                    }
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления пароля: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
