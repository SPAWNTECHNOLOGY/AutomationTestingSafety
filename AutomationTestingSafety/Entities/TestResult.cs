using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTestingSafety.Entities
{
    public class TestResult
    {
        public int TestResultId { get; set; }
        public int UserId { get; set; }
        public int TestId { get; set; }
        public string TimeTaken { get; set; }
        public int Score { get; set; }
        public int MinimalScore { get; set; }
        public string Status { get; set; }
        public string Details { get; set; }
        public string TestName { get; set; }
    }
}
