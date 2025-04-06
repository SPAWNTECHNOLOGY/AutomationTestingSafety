using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AutomationTestingSafety.Entities;

namespace AutomationTestingSafety
{
    public partial class AdminTestResultsWindow : Window
    {
        private string _employeeFio;
        public AdminTestResultsWindow(int userId, string employeeFio, List<TestResult> results)
        {
            InitializeComponent();
            _employeeFio = employeeFio;
            tbUserInfo.Text = $"Результаты тестов для сотрудника: {_employeeFio}";
            dgTestResults.ItemsSource = results;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (dgTestResults.SelectedItem is TestResult selectedResult)
            {
                // Открываем диалог сохранения файла
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.Filter = "PDF файл (*.pdf)|*.pdf";
                if (sfd.ShowDialog() == true)
                {
                    ExportResultToPdf(selectedResult, sfd.FileName, _employeeFio);
                    MessageBox.Show("Результат успешно экспортирован в PDF.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите результат для экспорта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExportResultToPdf(TestResult result, string filePath, string employeeFio)
        {
            // Используем шрифт с поддержкой кириллицы
            var baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(@"C:\Windows\Fonts\arial.ttf", iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);
            var normalFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL);
            var titleFont = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);

            var doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 50, 50, 50, 50);
            iTextSharp.text.pdf.PdfWriter.GetInstance(doc, new System.IO.FileStream(filePath, System.IO.FileMode.Create));
            doc.Open();

            // Заголовок
            doc.Add(new iTextSharp.text.Paragraph("Результаты тестирования", titleFont));
            doc.Add(new iTextSharp.text.Paragraph(" "));

            // Добавляем информацию о сотруднике
            doc.Add(new iTextSharp.text.Paragraph($"Сотрудник: {employeeFio}", normalFont));
            doc.Add(new iTextSharp.text.Paragraph(" "));

            // Основные данные результата
            doc.Add(new iTextSharp.text.Paragraph($"Время прохождения: {result.TimeTaken}", normalFont));
            doc.Add(new iTextSharp.text.Paragraph($"Набранные баллы: {result.Score}", normalFont));
            doc.Add(new iTextSharp.text.Paragraph($"Минимальный балл: {result.MinimalScore}", normalFont));
            doc.Add(new iTextSharp.text.Paragraph($"Статус: {result.Status}", normalFont));
            doc.Add(new iTextSharp.text.Paragraph(" "));
            doc.Add(new iTextSharp.text.Paragraph("Детали:", normalFont));
            doc.Add(new iTextSharp.text.Paragraph(" "));

            // Разбиваем детали по строкам и добавляем
            string[] lines = result.Details.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.None);
            foreach (var line in lines)
            {
                doc.Add(new iTextSharp.text.Paragraph(line, normalFont));
            }

            doc.Close();
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                string details = button.Tag.ToString();

                // Создаем окно для отображения деталей
                var detailsWindow = new Window
                {
                    Title = "Детали теста",
                    Content = new ScrollViewer
                    {
                        Content = new TextBlock
                        {
                            Text = details,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(10),
                            FontSize = 14
                        },
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    },
                    Width = 600,
                    Height = 400,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                detailsWindow.ShowDialog();
            }
        }
    }
}
