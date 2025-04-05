using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTestingSafety.Entities
{
    public class AnswerEntity
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public int Points { get; set; }
        public bool IsSelected { get; set; }
    }
}
