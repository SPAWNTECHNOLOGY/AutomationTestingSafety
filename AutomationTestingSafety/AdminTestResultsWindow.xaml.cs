using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AutomationTestingSafety.Entities;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Microsoft.Win32;
using System.Linq;


namespace AutomationTestingSafety
{
    public partial class AdminTestResultsWindow : Window
    {
        private string _employeeFio;
        private List<TestResult> _allResults;

        public AdminTestResultsWindow(int userId, string employeeFio, List<TestResult> results)
        {
            InitializeComponent();
            _employeeFio = employeeFio;
            tbUserInfo.Text = $"Результаты тестов для сотрудника: {_employeeFio}";
            _allResults = results;
            dgTestResults.ItemsSource = results;
            tbFilterTest.TextChanged += FilterOrSortChanged;
            tbFilterTimeTaken.TextChanged += FilterOrSortChanged;
            cbTimeFilterType.SelectionChanged += FilterOrSortChanged;
            cbSortField.SelectionChanged += FilterOrSortChanged;
        }

        private void FilterOrSortChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void ApplyFilterAndSort()
        {
            string testFilter = tbFilterTest.Text?.Trim().ToLower();
            string timeTakenFilter = tbFilterTimeTaken.Text?.Trim();
            string timeFilterType = (cbTimeFilterType.SelectedItem as ComboBoxItem)?.Tag as string;
            var filtered = _allResults.AsEnumerable();
            if (!string.IsNullOrEmpty(testFilter))
            {
                filtered = filtered.Where(r => (r.TestName ?? "").ToLower().Contains(testFilter));
            }
            if (!string.IsNullOrEmpty(timeTakenFilter) && TimeSpan.TryParse(timeTakenFilter, out TimeSpan filterTime))
            {
                if (timeFilterType == "Greater")
                {
                    filtered = filtered.Where(r => TimeSpan.TryParse(r.TimeTaken, out TimeSpan resultTime) && resultTime > filterTime);
                }
                else if (timeFilterType == "Less")
                {
                    filtered = filtered.Where(r => TimeSpan.TryParse(r.TimeTaken, out TimeSpan resultTime) && resultTime < filterTime);
                }
            }
            // Сортировка
            string sortField = (cbSortField.SelectedItem as ComboBoxItem)?.Tag as string;
            if (!string.IsNullOrEmpty(sortField))
            {
                switch (sortField)
                {
                    case "TestName": filtered = filtered.OrderBy(r => r.TestName); break;
                    case "Score": filtered = filtered.OrderByDescending(r => r.Score); break;
                    case "MinimalScore": filtered = filtered.OrderByDescending(r => r.MinimalScore); break;
                    case "Status": filtered = filtered.OrderBy(r => r.Status); break;
                    case "TimeTaken": filtered = filtered.OrderBy(r => r.TimeTaken); break;
                }
            }
            dgTestResults.ItemsSource = filtered.ToList();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            tbFilterTest.Text = string.Empty;
            tbFilterTimeTaken.Text = string.Empty;
            cbSortField.SelectedIndex = -1;
            dgTestResults.ItemsSource = _allResults;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (dgTestResults.SelectedItem is TestResult selectedResult)
            {
                SaveFileDialog sfd = new SaveFileDialog();
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

        private void ExportButtonWord_Click(object sender, RoutedEventArgs e)
        {
            if (dgTestResults.SelectedItem is TestResult selectedResult)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Word файл (*.docx)|*.docx";
                if (sfd.ShowDialog() == true)
                {
                    ExportResultToWord(selectedResult, sfd.FileName, _employeeFio);
                    MessageBox.Show("Результат успешно экспортирован в Word.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите результат для экспорта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExportResultToPdf(TestResult result, string filePath, string employeeFio)
        {
            // PDF-экспорт (как ранее)
            var baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(@"C:\Windows\Fonts\arial.ttf",
                                iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);
            var normalFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL);
            var titleFont = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);

            var doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 50, 50, 50, 50);
            iTextSharp.text.pdf.PdfWriter.GetInstance(doc, new System.IO.FileStream(filePath, System.IO.FileMode.Create));
            doc.Open();

            doc.Add(new iTextSharp.text.Paragraph("Результаты тестирования", titleFont));
            doc.Add(new iTextSharp.text.Paragraph(" "));
            doc.Add(new iTextSharp.text.Paragraph($"Сотрудник: {employeeFio}", normalFont));
            doc.Add(new iTextSharp.text.Paragraph($"Тест: {result.TestName}", normalFont));
            doc.Add(new iTextSharp.text.Paragraph(" "));
            doc.Add(new iTextSharp.text.Paragraph($"Время прохождения: {result.TimeTaken}", normalFont));
            doc.Add(new iTextSharp.text.Paragraph($"Набранные баллы: {result.Score}", normalFont));
            doc.Add(new iTextSharp.text.Paragraph($"Минимальный балл: {result.MinimalScore}", normalFont));
            doc.Add(new iTextSharp.text.Paragraph($"Статус: {result.Status}", normalFont));
            doc.Add(new iTextSharp.text.Paragraph(" "));
            doc.Add(new iTextSharp.text.Paragraph("Детали:", normalFont));
            doc.Add(new iTextSharp.text.Paragraph(" "));

            string[] lines = result.Details.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.None);
            foreach (var line in lines)
            {
                doc.Add(new iTextSharp.text.Paragraph(line, normalFont));
            }

            doc.Close();
        }

        private void ExportResultToWord(TestResult result, string filePath, string employeeFio)
        {
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Стиль для заголовка
                Paragraph title = new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Center },
                        new SpacingBetweenLines() { After = "200" }
                    ),
                    new Run(
                        new RunProperties(
                            new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" },
                            new FontSize() { Val = "28" },
                            new Bold()
                        ),
                        new Text("Результаты тестирования")
                    )
                );
                body.AppendChild(title);

                // Основная информация
                AddFormattedParagraph(body, $"Сотрудник: {employeeFio}", spacingAfter: 200);
                AddFormattedParagraph(body, $"Тест: {result.TestName}", spacingAfter: 200);
                AddFormattedParagraph(body, $"Время прохождения: {result.TimeTaken}", spacingAfter: 200);
                AddFormattedParagraph(body, $"Набранные баллы: {result.Score}", spacingAfter: 200);
                AddFormattedParagraph(body, $"Минимальный балл: {result.MinimalScore}", spacingAfter: 200);
                AddFormattedParagraph(body, $"Статус: {result.Status}", spacingAfter: 400);

                // Разделительная линия
                body.AppendChild(new Paragraph(
                    new ParagraphProperties(
                        new ParagraphBorders(
                            new BottomBorder() { Val = BorderValues.Single, Size = 6, Space = 1 }
                        ),
                        new SpacingBetweenLines() { After = "400" }
                    )
                ));

                // Заголовок "Детали:"
                AddFormattedParagraph(body, "Детали:", isBold: true, spacingAfter: 400);

                // Обработка деталей с правильным форматированием
                string[] questionBlocks = result.Details.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var block in questionBlocks)
                {
                    string[] lines = block.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(line => line.Trim())
                                        .Where(line => !string.IsNullOrWhiteSpace(line))
                                        .ToArray();

                    foreach (var line in lines)
                    {
                        if (line.StartsWith("Вопрос:") || line.StartsWith("Правильный:") || line.StartsWith("Результат:"))
                        {
                            AddFormattedParagraph(body, line, spacingAfter: 100);
                        }
                        else if (line.StartsWith("Ответ:"))
                        {
                            AddFormattedParagraph(body, line, spacingAfter: 200);
                        }
                    }

                    // Добавляем пустую строку между вопросами
                    body.AppendChild(new Paragraph(new Run(new Text("")))
                    {
                        ParagraphProperties = new ParagraphProperties()
                        {
                            SpacingBetweenLines = new SpacingBetweenLines() { After = "200" }
                        }
                    });
                }

                mainPart.Document.Save();
            }
        }

        private void AddFormattedParagraph(Body body, string text, int spacingAfter = 100, bool isBold = false)
        {
            Paragraph paragraph = new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines() { After = spacingAfter.ToString() },
                    new Justification() { Val = JustificationValues.Left }
                ),
                new Run(
                    new RunProperties(
                        new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" },
                        new FontSize() { Val = "24" },
                        new Bold() { Val = isBold }
                    ),
                    new Text(text)
                )
            );
            body.AppendChild(paragraph);
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
