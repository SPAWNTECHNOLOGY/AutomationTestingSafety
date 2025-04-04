using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTestingSafety.Entities
{
    // Класс для хранения информации о пользователе
    public class UserInfo
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public System.DateTime RegistrationDate { get; set; }
        public string PositionName { get; set; }
    }
}
