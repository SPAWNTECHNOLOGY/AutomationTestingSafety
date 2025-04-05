using System.Windows;
using System.Windows.Controls;
using AutomationTestingSafety.Entities;
using AutomationTestingSafety;
using Microsoft.VisualBasic; // для использования Interaction.InputBox

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
            txtTestName.Text = Test.Name;
            txtTestDesc.Text = Test.Description;
            txtMinScore.Text = Test.MinimalScore.ToString();
            lvQuestions.ItemsSource = Test.Questions;
            UpdateTestStructureTree();
            groupBoxTestStructure.Header = $"Структура теста (Минимальный балл: {Test.MinimalScore})";
        }

        private void UpdateTestStructureTree()
        {
            tvTestStructure.ItemsSource = null;
            tvTestStructure.ItemsSource = Test.Questions;
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            var newQuestion = new QuestionEntity { Text = "Новый вопрос..." };
            Test.Questions.Add(newQuestion);
            lvQuestions.Items.Refresh();
            UpdateTestStructureTree();
        }

        private void EditQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (lvQuestions.SelectedItem is QuestionEntity selectedQuestion)
            {
                string newText = Interaction.InputBox("Введите новый текст вопроса:", "Редактирование вопроса", selectedQuestion.Text);
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    selectedQuestion.Text = newText;
                    lvQuestions.Items.Refresh();
                    UpdateTestStructureTree();
                }
            }
            else
            {
                MessageBox.Show("Выберите вопрос для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void lvQuestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvQuestions.SelectedItem is QuestionEntity question)
            {
                _selectedQuestion = question;
                // Предполагается, что список ответов уже находится в question (если нужно, можно дополнительно загружать их)
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
            _selectedQuestion.Answers.Add(new AnswerEntity { Text = "Новый вариант...", IsCorrect = false, Points = 0 });
            lvAnswers.Items.Refresh();
            UpdateTestStructureTree();
        }

        private void DeleteAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (lvAnswers.SelectedItem is AnswerEntity selectedAnswer && _selectedQuestion != null)
            {
                _selectedQuestion.Answers.Remove(selectedAnswer);
                lvAnswers.Items.Refresh();
                UpdateTestStructureTree();
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
                // Редактирование текста ответа
                string newText = Interaction.InputBox("Введите новый текст ответа:", "Редактирование варианта", selectedAnswer.Text);
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    selectedAnswer.Text = newText;
                }

                // Запрашиваем статус правильности только один раз
                MessageBoxResult result = MessageBox.Show("Сделать ответ правильным?", "Редактирование варианта", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    return; // отмена изменений
                }
                else if (result == MessageBoxResult.Yes)
                {
                    selectedAnswer.IsCorrect = true;
                    // Запрашиваем баллы только для правильного ответа
                    string ptsInput = Interaction.InputBox("Введите количество баллов за ответ:", "Редактирование варианта", selectedAnswer.Points.ToString());
                    if (int.TryParse(ptsInput, out int pts))
                    {
                        selectedAnswer.Points = pts;
                    }
                    else
                    {
                        MessageBox.Show("Некорректное значение баллов. Баллы не изменены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else if (result == MessageBoxResult.No)
                {
                    // Если выбран No – делаем ответ неправильным и сбрасываем баллы в 0
                    selectedAnswer.IsCorrect = false;
                    selectedAnswer.Points = 0;
                }

                lvAnswers.Items.Refresh();
                UpdateTestStructureTree();
            }
            else
            {
                MessageBox.Show("Выберите вариант ответа для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void SaveTest_Click(object sender, RoutedEventArgs e)
        {
            Test.Name = txtTestName.Text;
            Test.Description = txtTestDesc.Text;
            if (int.TryParse(txtMinScore.Text, out int minScore))
                Test.MinimalScore = minScore;
            // Сохраняем изменения в БД (метод UpdateTest должен обновлять тест, вопросы и варианты ответов)
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

        private void DeleteQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (lvQuestions.SelectedItem is QuestionEntity selectedQuestion)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранный вопрос?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Удаляем вопрос из БД
                    TestRepository.DeleteQuestion(selectedQuestion.Id);
                    // Затем удаляем вопрос из коллекции теста
                    Test.Questions.Remove(selectedQuestion);
                    lvQuestions.Items.Refresh();
                    lvAnswers.ItemsSource = null;
                    UpdateTestStructureTree();
                }
            }
            else
            {
                MessageBox.Show("Выберите вопрос для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
