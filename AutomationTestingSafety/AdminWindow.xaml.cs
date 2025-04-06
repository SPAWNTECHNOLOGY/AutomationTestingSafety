using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AutomationTestingSafety.Database;
using AutomationTestingSafety.Entities;

namespace AutomationTestingSafety
{
    public partial class AdminWindow : Window
    {
        private UserInfo _userInfo;
        private DataTable usersTable;
        private SqlDataAdapter adapter;

        public AdminWindow(UserInfo userInfo)
        {
            InitializeComponent();
            _userInfo = userInfo;
            lblUserInfo.Text = $"Добро пожаловать, {_userInfo.FullName} (Администратор). Дата регистрации: {_userInfo.RegistrationDate:d}";
            LoadUsers();
        }

        private void ExitProfile(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var changePasswordWindow = new ChangePasswordWindow(_userInfo);
            changePasswordWindow.Owner = this;
            changePasswordWindow.ShowDialog();
        }

        private void LoadUsers()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString._connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT 
                    u.ID_Пользователя,
                    u.ФИО,
                    u.ДатаРождения,
                    u.Логин,
                    u.Пароль,
                    u.ДатаРегистрации,
                    d.НазваниеДолжности AS Должность
                FROM Пользователи u
                INNER JOIN Должности d ON u.ID_Должности = d.ID_Должности";
                    adapter = new SqlDataAdapter(query, connection);
                    usersTable = new DataTable();
                    adapter.Fill(usersTable);

                    // Обновленный UPDATE: добавлено обновление поля ДатаРождения
                    adapter.UpdateCommand = new SqlCommand(
                        @"UPDATE Пользователи 
                  SET ФИО = @fio, ДатаРождения = @dateBirth, Логин = @login, Пароль = @password 
                  WHERE ID_Пользователя = @id", connection);
                    adapter.UpdateCommand.Parameters.Add("@fio", SqlDbType.NVarChar, 200, "ФИО");
                    adapter.UpdateCommand.Parameters.Add("@dateBirth", SqlDbType.NVarChar, 200, "ДатаРождения");
                    adapter.UpdateCommand.Parameters.Add("@login", SqlDbType.NVarChar, 100, "Логин");
                    adapter.UpdateCommand.Parameters.Add("@password", SqlDbType.NVarChar, 100, "Пароль");
                    adapter.UpdateCommand.Parameters.Add("@id", SqlDbType.Int, 0, "ID_Пользователя");

                    // Обновленный INSERT: добавлено поле ДатаРождения
                    adapter.InsertCommand = new SqlCommand(
                        @"INSERT INTO Пользователи (ФИО, ДатаРождения, Логин, Пароль, ID_Должности) 
                  VALUES (@fio, @dateBirth, @login, @password, 
                  (SELECT TOP 1 ID_Должности FROM Должности WHERE НазваниеДолжности = @position))", connection);
                    adapter.InsertCommand.Parameters.Add("@fio", SqlDbType.NVarChar, 200, "ФИО");
                    adapter.InsertCommand.Parameters.Add("@dateBirth", SqlDbType.NVarChar, 200, "ДатаРождения");
                    adapter.InsertCommand.Parameters.Add("@login", SqlDbType.NVarChar, 100, "Логин");
                    adapter.InsertCommand.Parameters.Add("@password", SqlDbType.NVarChar, 100, "Пароль");
                    adapter.InsertCommand.Parameters.Add("@position", SqlDbType.NVarChar, 200, "Должность");

                    adapter.DeleteCommand = new SqlCommand(
                        @"DELETE FROM Пользователи WHERE ID_Пользователя = @id", connection);
                    adapter.DeleteCommand.Parameters.Add("@id", SqlDbType.Int, 0, "ID_Пользователя");

                    UsersDataGrid.ItemsSource = usersTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Автоматическое сохранение изменений при завершении редактирования строки
        private void UsersDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Используем Dispatcher для вызова SaveData после завершения редактирования, чтобы избежать рекурсии
                Dispatcher.BeginInvoke(new Action(() => SaveData(false)), DispatcherPriority.Background);
            }
        }

        // Если showNotification == true, выводится сообщение об успехе
        private void SaveData(bool showNotification = true)
        {
            // Заменяем DBNull в поле "Пароль" на пустую строку
            foreach (DataRow row in usersTable.Rows)
            {
                if (row.RowState != DataRowState.Deleted && row["Пароль"] == DBNull.Value)
                {
                    row["Пароль"] = "";
                }
            }

            try
            {
                using (var connection = new SqlConnection(ConnectionString._connectionString))
                {
                    connection.Open();
                    adapter.UpdateCommand.Connection = connection;
                    adapter.InsertCommand.Connection = connection;
                    adapter.DeleteCommand.Connection = connection;
                    adapter.Update(usersTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения изменений: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Открытие окна для добавления нового пользователя
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddUserWindow();
            addWindow.Owner = this;
            if (addWindow.ShowDialog() == true)
            {
                DataRow newRow = usersTable.NewRow();
                newRow["ФИО"] = addWindow.FullName;
                newRow["ДатаРождения"] = addWindow.BirthDate;
                newRow["Логин"] = addWindow.Login;
                newRow["Пароль"] = ""; // пароль устанавливается отдельно
                newRow["ДатаРегистрации"] = DateTime.Now;
                newRow["Должность"] = addWindow.Position;
                usersTable.Rows.Add(newRow);
                SaveData();
                MessageBox.Show("Пользователь успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers(); // Перезагружаем данные из БД, чтобы подтянуть сгенерированный ID
            }
        }


        // Удаление выбранного пользователя
        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is DataRowView row)
            {
                row.Row.Delete();
                SaveData();
                MessageBox.Show("Пользователь успешно удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Открывает окно для установки нового пароля для выбранного пользователя
        private void SetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is DataRowView row)
            {
                // Если ID_Пользователя равен DBNull, сообщаем об ошибке и выходим
                if (row["ID_Пользователя"] == DBNull.Value)
                {
                    MessageBox.Show("Ошибка: ID пользователя не задан.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int userId = Convert.ToInt32(row["ID_Пользователя"]);
                var win = new SetUserPasswordWindow(userId);
                win.Owner = this;
                if (win.ShowDialog() == true)
                {
                    row["Пароль"] = win.NewPassword;
                    SaveData();
                    MessageBox.Show("Пароль успешно изменён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для установки пароля.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewTestResults_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is DataRowView row)
            {
                if (row["ID_Пользователя"] == DBNull.Value)
                {
                    MessageBox.Show("Ошибка: ID пользователя не задан.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                int userId = Convert.ToInt32(row["ID_Пользователя"]);
                string employeeFio = row["ФИО"].ToString();
                List<TestResult> results = TestRepository.GetTestResultsForUser(userId);
                var win = new AdminTestResultsWindow(userId, employeeFio, results);
                win.Owner = this;
                win.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


    }
}
