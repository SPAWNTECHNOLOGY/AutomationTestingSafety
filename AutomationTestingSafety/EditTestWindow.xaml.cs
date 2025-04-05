using System.Windows;
using System.Windows.Controls;
using AutomationTestingSafety.Entities;
using Microsoft.VisualBasic;

namespace AutomationTestingSafety
{
    public partial class EditTestWindow : Window
    {
        public TestEntity Test { get; set; }
        private QuestionEntity _selectedQuestion;

        public EditTestWindow(TestEntity test)
        {
            InitializeComponent();
            Test = test;
            // Инициализация полей
            txtTestName.Text = Test.Name;
            txtTestDesc.Text = Test.Description;
            lvQuestions.ItemsSource = Test.Questions;
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            // Добавление нового вопроса
            var newQuestion = new QuestionEntity { Text = "Новый вопрос..." };
            Test.Questions.Add(newQuestion);
            lvQuestions.Items.Refresh();
        }

        private void lvQuestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvQuestions.SelectedItem is QuestionEntity question)
            {
                _selectedQuestion = question;
                lvAnswers.ItemsSource = _selectedQuestion.Answers;
                lvAnswers.Items.Refresh();
            }
            else
            {
                _selectedQuestion = null;
                lvAnswers.ItemsSource = null;
            }
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedQuestion == null)
            {
                MessageBox.Show("Выберите вопрос для добавления варианта ответа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Добавление нового варианта ответа
            _selectedQuestion.Answers.Add(new AnswerEntity { Text = "Новый вариант...", IsCorrect = false });
            lvAnswers.Items.Refresh();
        }

        private void DeleteAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (lvAnswers.SelectedItem is AnswerEntity selectedAnswer && _selectedQuestion != null)
            {
                _selectedQuestion.Answers.Remove(selectedAnswer);
                lvAnswers.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Выберите вариант ответа для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EditAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (lvAnswers.SelectedItem is AnswerEntity selectedAnswer)
            {
                // Пример: откроем окно редактирования варианта ответа или просто изменим его свойства через InputBox
                // Здесь для простоты – меняем текст и переключаем правильность
                selectedAnswer.Text = Interaction.InputBox("Введите новый текст ответа:", "Редактирование варианта", selectedAnswer.Text);

                // Переключаем флаг правильного (это можно заменить более удобным UI)
                selectedAnswer.IsCorrect = !selectedAnswer.IsCorrect;
                lvAnswers.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Выберите вариант ответа для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveTest_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем данные теста
            Test.Name = txtTestName.Text;
            Test.Description = txtTestDesc.Text;
            // Здесь можно добавить дополнительные проверки и валидацию

            // Сохраняем изменения в БД через репозиторий (метод UpdateTest обновит тест, вопросы и ответы)
            TestRepository.UpdateTest(Test);

            MessageBox.Show("Изменения сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
