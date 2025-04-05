using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTestingSafety.Entities
{
    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MinimalScore { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public List<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
    }
}
