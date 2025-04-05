using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using AutomationTestingSafety.Database;
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

            // Создаем тест в БД и получаем его Id (если требуется)
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
                        // Обновляем список тестов после редактирования
                        LoadTests();
                        // При изменении структуры теста также обновляем дерево
                        tvTestStructure.ItemsSource = null;
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите тест для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void lvTests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvTests.SelectedItem is TestEntity test)
            {
                // Загружаем полную структуру теста (с вопросами и ответами)
                TestEntity fullTest = TestRepository.GetTestById(test.Id);
                // Заполняем дерево структуры теста только вопросами (с вариантами)
                tvTestStructure.ItemsSource = fullTest?.Questions;
            }
            else
            {
                tvTestStructure.ItemsSource = null;
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            // Реализация смены пароля
        }

        private void ExitProfile(object sender, RoutedEventArgs e)
        {
            // Реализация выхода из профиля
        }

        private void DeleteTest_Click(object sender, RoutedEventArgs e)
        {
            if (lvTests.SelectedItem is TestEntity selectedTest)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранный тест?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Здесь можно вызвать метод репозитория для удаления теста из БД, например:
                    TestRepository.DeleteTest(selectedTest.Id);
                    // Обновляем список тестов
                    LoadTests();
                    // Очищаем дерево структуры, если удалённый тест был выбран
                    tvTestStructure.ItemsSource = null;
                }
            }
            else
            {
                MessageBox.Show("Выберите тест для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
       

    }
}
