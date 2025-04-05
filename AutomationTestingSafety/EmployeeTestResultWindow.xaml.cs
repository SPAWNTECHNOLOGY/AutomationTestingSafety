using System.Collections.Generic;
using System.Windows;

namespace AutomationTestingSafety
{
    public partial class EmployeeTestResultWindow : Window
    {
        public EmployeeTestResultWindow(string summary, List<ResultItem> results)
        {
            InitializeComponent();
            tbSummary.Text = summary;
            dgResults.ItemsSource = results;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
