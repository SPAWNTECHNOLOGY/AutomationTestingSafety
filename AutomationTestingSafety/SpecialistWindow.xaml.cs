using System;
using System.Windows;
using System.Windows.Controls;
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
            lblUserInfo.Text = $"Добро пожаловать, {_userInfo.FullName} (Специалист). Дата регистрации: {_userInfo.RegistrationDate:d}";
            LoadTests();
        }

        private void LoadTests()
        {
            lvTests.ItemsSource = TestRepository.GetAllTests();
        }

        private void AddTest_Click(object sender, RoutedEventArgs e)
        {
            // Создаем новый тест с базовыми значениями
            TestEntity newTest = new TestEntity
            {
                Name = "Новый тест",
                Description = "Описание теста",
                Active = false
            };

            // Получаем Id вновь созданного теста (если необходимо)
            newTest.Id = TestRepository.CreateTest(newTest);
            LoadTests();
        }

        private void EditTest_Click(object sender, RoutedEventArgs e)
        {
            if (lvTests.SelectedItem is TestEntity selectedTest)
            {
                // Получаем тест с вопросами и вариантами ответов
                TestEntity test = TestRepository.GetTestById(selectedTest.Id);
                if (test != null)
                {
                    EditTestWindow editWindow = new EditTestWindow(test);
                    editWindow.Owner = this;
                    if (editWindow.ShowDialog() == true)
                    {
                        // Сохраняем изменения через репозиторий
                        TestRepository.UpdateTest(test);
                        LoadTests();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите тест для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
