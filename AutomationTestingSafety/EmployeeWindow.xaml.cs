using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AutomationTestingSafety.Entities;

namespace AutomationTestingSafety
{
    public partial class EmployeeWindow : Window
    {
        private UserInfo _userInfo;

        public EmployeeWindow(UserInfo userInfo)
        {
            InitializeComponent();
            _userInfo = userInfo;
            lblUserInfo.Content = $"Добро пожаловать, {_userInfo.FullName} (Сотрудник). Дата регистрации: {_userInfo.RegistrationDate:d}";
        }

        private void ExitProfile(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow(_userInfo);
            changePasswordWindow.Owner = this;
            changePasswordWindow.ShowDialog();
        }
    }
}
