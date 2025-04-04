using System;
using System.Data.SqlClient;
using System.Windows;
using AutomationTestingSafety.Database;
using AutomationTestingSafety.Entities;

namespace AutomationTestingSafety
{
    public partial class ChangePasswordWindow : Window
    {
        private UserInfo _user;

        public ChangePasswordWindow(UserInfo user)
        {
            InitializeComponent();
            _user = user;
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = OldPasswordBox.Password.Trim();
            string newPassword = NewPasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = new SqlConnection(ConnectionString._connectionString))
                {
                    connection.Open();
                    string query = @"
                        UPDATE Пользователи
                        SET Пароль = @newPassword
                        WHERE ID_Пользователя = @userID AND Пароль = @oldPassword";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@newPassword", newPassword);
                        command.Parameters.AddWithValue("@userID", _user.UserID);
                        command.Parameters.AddWithValue("@oldPassword", oldPassword);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Пароль успешно изменён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Старый пароль указан неверно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при смене пароля: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
