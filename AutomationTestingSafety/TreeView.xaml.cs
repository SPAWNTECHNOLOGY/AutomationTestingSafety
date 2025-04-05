using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutomationTestingSafety
{
    /// <summary>
    /// Логика взаимодействия для TreeView.xaml
    /// </summary>
    public partial class TreeView : Window
    {
        public TreeView()
        {
            InitializeComponent();
        }
    }

    public class TestStatus
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MinimalScore { get; set; }
        public int StatusId { get; set; }  // Используется для хранения выбранного статуса теста
        public string StatusName { get; set; } // Опционально для отображения текстового значения статуса
        public List<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
    }



    public class QuestionEntity
    {
        public int Id { get; set; }
        public string Text { get; set; }             // Текст вопроса
        public List<AnswerEntity> Answers { get; set; } = new List<AnswerEntity>();
    }

    public class AnswerEntity
    {
        public int Id { get; set; }
        public string Text { get; set; }             // Текст варианта ответа
        public bool IsCorrect { get; set; }          // Флаг: правильный ответ или нет
        public int Points { get; set; }              // Количество баллов за этот вариант ответа
    }
}
