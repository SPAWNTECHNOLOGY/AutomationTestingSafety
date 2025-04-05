using System.Windows;
using System.Windows.Controls;
using AutomationTestingSafety.Entities;
using AutomationTestingSafety;

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
            lvQuestions.ItemsSource = Test.Questions;
            UpdateTestStructureTree();
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
                // Редактируем текст вопроса с помощью InputBox (требуется ссылка на Microsoft.VisualBasic)
                string newText = Microsoft.VisualBasic.Interaction.InputBox("Введите новый текст вопроса:", "Редактирование вопроса", selectedQuestion.Text);
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
            _selectedQuestion.Answers.Add(new AnswerEntity { Text = "Новый вариант...", IsCorrect = false });
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
                selectedAnswer.Text = Microsoft.VisualBasic.Interaction.InputBox("Введите новый текст ответа:", "Редактирование варианта", selectedAnswer.Text);
                selectedAnswer.IsCorrect = !selectedAnswer.IsCorrect;
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
