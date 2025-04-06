using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using AutomationTestingSafety.Entities;

namespace AutomationTestingSafety
{
    public partial class EmployeeTestWindow : Window
    {
        private TestEntity _test;
        private int _currentQuestionIndex = 0;
        private readonly DispatcherTimer _timer;
        private TimeSpan _timeRemaining;
        private int _userId; // ID пользователя, проходящего тест
        public EmployeeTestWindow(TestEntity test, int userId)
        {
            InitializeComponent();
            _userId = userId;
            _test = TestRepository.GetTestById(test.Id);
            if (_test == null || _test.Questions.Count == 0)
            {
                MessageBox.Show("В выбранном тесте отсутствуют вопросы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            _timeRemaining = TimeSpan.FromMinutes(15);
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            LoadCurrentQuestion();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1));
            tbTimer.Text = $"Осталось: {_timeRemaining:mm\\:ss}";
            if (_timeRemaining.TotalSeconds <= 0)
            {
                _timer.Stop();
                FinishTest();
            }
        }

        private void LoadCurrentQuestion()
        {
            if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _test.Questions.Count)
                return;

            var question = _test.Questions[_currentQuestionIndex];
            tbQuestion.Text = question.Text;
            // Привязываем список ответов напрямую (свойство IsSelected присутствует в AnswerEntity)
            lbAnswers.ItemsSource = question.Answers;
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex > 0)
            {
                _currentQuestionIndex--;
                LoadCurrentQuestion();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex < _test.Questions.Count - 1)
            {
                _currentQuestionIndex++;
                LoadCurrentQuestion();
            }
            else
            {
                // Если последний вопрос, можно сразу завершить тест
                FinishTest();
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            FinishTest();
        }

        private void FinishTest()
        {
            int totalScore = 0;
            var results = new List<ResultItem>();

            foreach (var question in _test.Questions)
            {
                var correct = question.Answers.FirstOrDefault(a => a.IsCorrect);
                var userAnswer = question.Answers.FirstOrDefault(a => a.IsSelected);
                string correctText = correct != null ? correct.Text : "Нет";
                string userText = userAnswer != null ? userAnswer.Text : "Не выбран";
                string resultText = (userAnswer != null && userAnswer.IsCorrect) ? "Правильно" : "Неправильно";
                if (userAnswer != null && userAnswer.IsCorrect)
                    totalScore += userAnswer.Points;

                results.Add(new ResultItem
                {
                    QuestionText = question.Text,
                    YourAnswer = userText,
                    CorrectAnswer = correctText,
                    IsCorrectText = resultText
                });
            }

            TimeSpan timeTaken = TimeSpan.FromMinutes(15) - _timeRemaining;
            string status = totalScore >= _test.MinimalScore ? "Сдал(а)" : "Не сдал(а)";
            string summary = $"Тест завершен.\nВремя прохождения: {timeTaken:mm\\:ss}.\n" +
                             $"Набрано баллов: {totalScore} (Мин. требуемо: {_test.MinimalScore}).\nСтатус: {status}.";

            var testResult = new TestResult
            {
                UserId = _userId,  // Теперь подставляется корректный userId
                TestId = _test.Id,
                TimeTaken = timeTaken.ToString(@"mm\:ss"),
                Score = totalScore,
                MinimalScore = _test.MinimalScore,
                Status = status,
                Details = string.Join("\n-----------------\n", results.Select(r =>
                    $"Вопрос: {r.QuestionText}\nОтвет: {r.YourAnswer}\nПравильный: {r.CorrectAnswer}\nРезультат: {r.IsCorrectText}"))
            };

            TestRepository.SaveTestResult(testResult);

            EmployeeTestResultWindow resultWindow = new EmployeeTestResultWindow(summary, results);
            resultWindow.Owner = this;
            resultWindow.ShowDialog();
            Close();
        }
    }

    public class ResultItem
    {
        public string QuestionText { get; set; }
        public string YourAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public string IsCorrectText { get; set; }
    }
}
