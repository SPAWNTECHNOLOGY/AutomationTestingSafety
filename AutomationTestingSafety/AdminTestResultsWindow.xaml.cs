using System.Collections.Generic;
using System.Windows;
using AutomationTestingSafety.Entities;

namespace AutomationTestingSafety
{
    public partial class AdminTestResultsWindow : Window
    {
        public AdminTestResultsWindow(int userId, string userFio, List<TestResult> results)
        {
            InitializeComponent();
            tbUserInfo.Text = $"Результаты тестов для сотрудника: {userId} (ID: {userFio})";
            dgTestResults.ItemsSource = results;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
