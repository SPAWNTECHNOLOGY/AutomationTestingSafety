using System.Windows;
using AutomationTestingSafety.Entities;

namespace AutomationTestingSafety
{
    public partial class AdminWindow : Window
    {
        private UserInfo _userInfo;

        public AdminWindow(UserInfo userInfo)
        {
            InitializeComponent();
            _userInfo = userInfo;
            lblUserInfo.Content = $"Добро пожаловать, {_userInfo.FullName} (Администратор). Дата регистрации: {_userInfo.RegistrationDate:d}";
        }

        private void ExitProfile(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
