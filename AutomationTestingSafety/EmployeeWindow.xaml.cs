using System.Windows;
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
            LoadAvailableTests();
        }

        private void LoadAvailableTests()
        {
            var tests = TestRepository.GetAllTests();
            lvAvailableTests.ItemsSource = tests;
        }

        private void TakeTest_Click(object sender, RoutedEventArgs e)
        {
            if (lvAvailableTests.SelectedItem is TestEntity selectedTest)
            {
                // Получаем тест с вопросами из БД
                var fullTest = TestRepository.GetTestById(selectedTest.Id);
                if (fullTest == null || fullTest.Questions.Count == 0)
                {
                    MessageBox.Show("В выбранном тесте отсутствуют вопросы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                EmployeeTestWindow testWindow = new EmployeeTestWindow(fullTest);
                testWindow.Owner = this;
                testWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите тест для прохождения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
