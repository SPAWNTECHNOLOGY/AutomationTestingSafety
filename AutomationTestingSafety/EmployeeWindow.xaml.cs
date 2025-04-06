using System.Collections.Generic;
using System.Linq;
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
            lblUserInfo.Text = $"Добро пожаловать, {_userInfo.FullName} (Сотрудник). Дата регистрации: {_userInfo.RegistrationDate:d}";
            LoadAvailableTests();
        }

        private void LoadAvailableTests()
        {
            var tests = TestRepository.GetAllTests()
                                      .Where(t => t.StatusId == 2)
                                      .ToList();
            lvAvailableTests.ItemsSource = tests;
        }

        private void TakeTest_Click(object sender, RoutedEventArgs e)
        {
            if (lvAvailableTests.SelectedItem is TestEntity selectedTest)
            {
                var fullTest = TestRepository.GetTestById(selectedTest.Id);
                if (fullTest == null || fullTest.Questions.Count == 0)
                {
                    MessageBox.Show("В выбранном тесте отсутствуют вопросы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                EmployeeTestWindow testWindow = new EmployeeTestWindow(fullTest, _userInfo.UserID);
                testWindow.Owner = this;
                testWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите тест для прохождения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow(_userInfo);
            changePasswordWindow.Owner = this;
            changePasswordWindow.ShowDialog();
        }

        private void ExitProfile(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void ViewTestResults_Click(object sender, RoutedEventArgs e)
        {
            // Получаем результаты тестов для текущего пользователя
            List<TestResult> results = TestRepository.GetTestResultsForUser(_userInfo.UserID);
            // Открываем окно для просмотра результатов
            EmployeeResultsWindow resultsWindow = new EmployeeResultsWindow(_userInfo, results);
            resultsWindow.Owner = this;
            resultsWindow.ShowDialog();
        }
    }
}
