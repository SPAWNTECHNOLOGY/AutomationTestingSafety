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
            lbAnswers.ItemsSource = question.Answers;

            // Обновляем счётчик вопросов
            tbQuestionCounter.Text = $"Вопрос {_currentQuestionIndex + 1} из {_test.Questions.Count}";

            // Если текущий вопрос последний, скрываем кнопку "Следующий"
            btnNext.Visibility = (_currentQuestionIndex == _test.Questions.Count - 1)
                ? Visibility.Collapsed : Visibility.Visible;
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
            // Если последнего вопроса нет, можно вызвать завершение теста
            else
            {
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
            int correctCount = 0;
            var results = new List<ResultItem>();

            foreach (var question in _test.Questions)
            {
                var userAnswer = question.Answers.FirstOrDefault(a => a.IsSelected);
                var correct = question.Answers.FirstOrDefault(a => a.IsCorrect);

                bool isAnswerCorrect = (userAnswer != null && userAnswer.IsCorrect);
                if (isAnswerCorrect)
                {
                    totalScore += userAnswer.Points;
                    correctCount++;
                }

                results.Add(new ResultItem
                {
                    QuestionText = question.Text,
                    YourAnswer = userAnswer != null ? userAnswer.Text : "Не выбран",
                    CorrectAnswer = correct != null ? correct.Text : "Нет",
                    IsCorrect = isAnswerCorrect,
                    IsCorrectText = isAnswerCorrect ? "Правильно" : "Неправильно"
                });
            }

            int totalQuestions = _test.Questions.Count;
            TimeSpan timeTaken = TimeSpan.FromMinutes(15) - _timeRemaining;
            string status = totalScore >= _test.MinimalScore ? "Сдал(а)" : "Не сдал(а)";
            string summary = $"Тест завершен.\nВремя прохождения: {timeTaken:mm\\:ss}.\n" +
                             $"Набрано баллов: {totalScore} (Мин. требуемо: {_test.MinimalScore}).\n" +
                             $"Правильных ответов: {correctCount} из {totalQuestions}.\nСтатус: {status}.";

            var testResult = new TestResult
            {
                UserId = _userId,
                TestId = _test.Id,
                TimeTaken = timeTaken.ToString(@"mm\:ss"),
                Score = totalScore,
                MinimalScore = _test.MinimalScore,
                Status = status,
                Details = string.Join("\n-----------------\n", results.Select(r =>
                    $"Вопрос: {r.QuestionText}\nОтвет: {r.YourAnswer}\nПравильный: {r.CorrectAnswer}\nРезультат: {r.IsCorrectText}"))
            };

            TestRepository.SaveTestResult(testResult);

            EmployeeTestResultWindow resultWindow = new EmployeeTestResultWindow(
                $"Тест завершен.\nВремя: {timeTaken:mm\\:ss}\nБаллы: {totalScore} из {_test.MinimalScore}\nПравильных ответов: {correctCount} из {totalQuestions}\nСтатус: {status}",
                results);
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
        public bool IsCorrect { get; set; }
    }
}
