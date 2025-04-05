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

        public EmployeeTestWindow(TestEntity test)
        {
            InitializeComponent();
            // Перезагружаем тест из БД, чтобы гарантированно получить заполненные вопросы и варианты ответов
            _test = TestRepository.GetTestById(test.Id);
            if (_test == null || _test.Questions.Count == 0)
            {
                MessageBox.Show("В выбранном тесте отсутствуют вопросы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            // Таймер на 15 минут
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

            // Собираем результаты для каждого вопроса
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

            // Вычисляем время прохождения
            TimeSpan timeTaken = TimeSpan.FromMinutes(15) - _timeRemaining;
            // Определяем статус прохождения теста
            string status = totalScore >= _test.MinimalScore ? "Сдал" : "Не сдал";
            string summary = $"Тест завершен.\nВремя прохождения: {timeTaken:mm\\:ss}.\n" +
                             $"Набрано баллов: {totalScore} (Мин. требуемо: {_test.MinimalScore}).\nСтатус: {status}.";

            // Создаем объект результата для сохранения в БД
            var testResult = new TestResult
            {
                // Здесь замените 1 на реальное значение идентификатора пользователя, например, _userInfo.ID
                UserId = 1,
                TestId = _test.Id,
                TimeTaken = timeTaken.ToString(@"mm\:ss"),
                Score = totalScore,
                MinimalScore = _test.MinimalScore,
                Status = status,
                Details = string.Join("\n-----------------\n", results.Select(r =>
                    $"Вопрос: {r.QuestionText}\nВаш ответ: {r.YourAnswer}\nПравильный: {r.CorrectAnswer}\nРезультат: {r.IsCorrectText}"))
            };

            // Сохраняем результат в БД
            TestRepository.SaveTestResult(testResult);

            // Показываем окно с результатами теста
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
