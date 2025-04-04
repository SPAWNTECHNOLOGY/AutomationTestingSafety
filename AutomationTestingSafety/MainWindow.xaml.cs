using System;
using System.Data.SqlClient;
using System.Windows;
using AutomationTestingSafety.Database;
using AutomationTestingSafety.Entities;

namespace AutomationTestingSafety
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = UsernameBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (AuthenticateUser(login, password, out UserInfo user))
            {
                Window roleWindow = null;

                if (user.PositionName == "Администратор")
                {
                    roleWindow = new AdminWindow(user);
                }
                else if (user.PositionName == "Специалист")
                {
                    roleWindow = new SpecialistWindow(user);
                }
                else if (user.PositionName == "Сотрудник")
                {
                    roleWindow = new EmployeeWindow(user);
                }

                if (roleWindow != null)
                {
                    roleWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Роль пользователя не определена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Функция проверки логина и пароля
        private bool AuthenticateUser(string login, string password, out UserInfo userInfo)
        {
            userInfo = null;
            using (var connection = new SqlConnection(ConnectionString._connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT u.ID_Пользователя, u.ФИО, u.Логин, u.ДатаРегистрации, d.НазваниеДолжности
                    FROM Пользователи u
                    INNER JOIN Должности d ON u.ID_Должности = d.ID_Должности
                    WHERE u.Логин = @login AND u.Пароль = @password";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userInfo = new UserInfo
                            {
                                UserID = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                Login = reader.GetString(2),
                                RegistrationDate = reader.GetDateTime(3),
                                PositionName = reader.GetString(4)
                            };
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
