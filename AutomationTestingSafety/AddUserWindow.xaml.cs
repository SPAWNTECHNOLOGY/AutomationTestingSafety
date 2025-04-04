using System.Windows;
using System.Windows.Controls;

namespace AutomationTestingSafety
{
    public partial class AddUserWindow : Window
    {
        public string FullName { get; private set; }
        public string Login { get; private set; }
        public string Position { get; private set; }

        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtLogin.Text) ||
                cbPosition.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            FullName = txtFullName.Text.Trim();
            Login = txtLogin.Text.Trim();
            Position = ((ComboBoxItem)cbPosition.SelectedItem).Content.ToString();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
