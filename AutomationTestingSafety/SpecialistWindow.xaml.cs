using System.Windows;
using AutomationTestingSafety.Entities;

namespace AutomationTestingSafety
{
    public partial class SpecialistWindow : Window
    {
        private UserInfo _userInfo;

        public SpecialistWindow(UserInfo userInfo)
        {
            InitializeComponent();
            _userInfo = userInfo;
            lblUserInfo.Content = $"Добро пожаловать, {_userInfo.FullName} (Специалист). Дата регистрации: {_userInfo.RegistrationDate:d}";
        }

        private void ExitProfile(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
