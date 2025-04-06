using System.Collections.Generic;
using System.Windows;
using AutomationTestingSafety.Entities;

namespace AutomationTestingSafety
{
    public partial class EmployeeResultsWindow : Window
    {
        public EmployeeResultsWindow(UserInfo user, List<TestResult> results)
        {
            InitializeComponent();
            tbUserInfo.Text = $"Результаты тестов для {user.FullName}";
            dgResults.ItemsSource = results;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
