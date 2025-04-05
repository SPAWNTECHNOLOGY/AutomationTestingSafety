using System;
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
            // Перезагружаем тест из БД, чтобы гарантированно получить вопросы и варианты
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
            // Привязываем список вариантов ответа напрямую из Question.Answers
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
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            FinishTest();
        }

        private void FinishTest()
        {
            int totalScore = 0;
            // Проходим по всем вопросам и суммируем баллы за выбранные варианты
            foreach (var question in _test.Questions)
            {
                // Если пользователь выбрал правильный вариант, прибавляем баллы
                var selected = question.Answers.FirstOrDefault(a => a.IsSelected);
                if (selected != null && selected.IsCorrect)
                    totalScore += selected.Points;
            }
            MessageBox.Show($"Ваш результат: {totalScore} баллов.", "Результаты теста", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
    }
}
