using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Win32;
using System.Diagnostics;

namespace Flashcards.Views
{
    public partial class YearlyReportPage : Page
    {
        private readonly string connectionString = "Data Source=DESKTOP-UI42SK7\\SQLEXPRESS;Initial Catalog=Flashcards;Integrated Security=True";
        private DataTable currentReportData;
        private int currentYear;

        public YearlyReportPage()
        {
            InitializeComponent();
            QuestPDF.Settings.License = LicenseType.Community;
            PopulateYears();
        }

        private void PopulateYears()
        {
            var years = new List<int>();
            int currentYearValue = DateTime.Now.Year;

            // Add years from 5 years ago to current year
            for (int i = currentYearValue - 5; i <= currentYearValue; i++)
            {
                years.Add(i);
            }

            cmbYear.ItemsSource = years;
            cmbYear.SelectedItem = currentYearValue;
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (cmbYear.SelectedItem == null)
            {
                MessageBox.Show("Please select a year.", "Selection Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int selectedYear = (int)cmbYear.SelectedItem;
            currentYear = selectedYear;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if there are any sessions for the selected year
                var checkSessionsCmd = connection.CreateCommand();
                checkSessionsCmd.CommandText = "SELECT COUNT(*) FROM StudySessions WHERE YEAR(Date) = @year";
                checkSessionsCmd.Parameters.AddWithValue("@year", selectedYear);
                int sessionCount = (int)checkSessionsCmd.ExecuteScalar();

                if (sessionCount == 0)
                {
                    MessageBox.Show($"No study sessions found for year {selectedYear}.", "No Data",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    txtNoData.Visibility = Visibility.Visible;
                    ReportScrollViewer.Visibility = Visibility.Collapsed;
                    if (btnExportPDF != null) btnExportPDF.IsEnabled = false;
                    currentReportData = null;
                    return;
                }

                // Generate the pivot report
                var viewSessionYearlyReport = connection.CreateCommand();
                viewSessionYearlyReport.CommandText = @"
                    SELECT StackId,
                        ISNULL([1], 0) AS Jan,
                        ISNULL([2], 0) AS Feb,
                        ISNULL([3], 0) AS Mar,
                        ISNULL([4], 0) AS Apr,
                        ISNULL([5], 0) AS May,
                        ISNULL([6], 0) AS Jun,
                        ISNULL([7], 0) AS Jul,
                        ISNULL([8], 0) AS Aug,
                        ISNULL([9], 0) AS Sep,
                        ISNULL([10], 0) AS Oct,
                        ISNULL([11], 0) AS Nov,
                        ISNULL([12], 0) AS Dec
                    FROM
                    (
                        SELECT StackId, MONTH(Date) AS Month, COUNT(*) AS SessionsCount
                        FROM StudySessions
                        WHERE YEAR(Date) = @year
                        GROUP BY StackId, MONTH(Date)
                    ) AS SourceTable
                    PIVOT
                    (
                        SUM(SessionsCount)
                        FOR Month IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])
                    ) AS PivotTable
                    ORDER BY StackId";

                viewSessionYearlyReport.Parameters.AddWithValue("@year", selectedYear);

                SqlDataAdapter adapter = new SqlDataAdapter(viewSessionYearlyReport);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                if (dataTable.Rows.Count > 0)
                {
                    currentReportData = dataTable;
                    dgReport.ItemsSource = dataTable.DefaultView;
                    txtReportTitle.Text = $"Study Sessions Count by Stack for Year {selectedYear}";
                    txtNoData.Visibility = Visibility.Collapsed;
                    ReportScrollViewer.Visibility = Visibility.Visible;
                    if (btnExportPDF != null) btnExportPDF.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("No data to display.", "No Data",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    if (btnExportPDF != null) btnExportPDF.IsEnabled = false;
                    currentReportData = null;
                }
            }
        }

        private void BtnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            if (currentReportData == null)
            {
                MessageBox.Show("Please generate a report first.", "No Data",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Open save file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"FlashcardsYearlyReport_{currentYear}.pdf",
                Title = "Save Yearly Report as PDF"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    GeneratePDF(saveFileDialog.FileName);
                    
                    var result = MessageBox.Show(
                        $"PDF saved successfully!\n\nLocation: {saveFileDialog.FileName}\n\nWould you like to open the file?",
                        "Success",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating PDF: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GeneratePDF(string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .AlignCenter()
                        .Column(column =>
                        {
                            column.Item().Text($"Flashcards Study Session Report - Year {currentYear}")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().PaddingTop(5).Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // Add description
                            column.Item().Text("This report shows the number of study sessions completed for each stack throughout the year.")
                                .FontSize(11)
                                .Italic();

                            // Create table
                            column.Item().Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1); // StackId
                                    columns.RelativeColumn(1); // Jan
                                    columns.RelativeColumn(1); // Feb
                                    columns.RelativeColumn(1); // Mar
                                    columns.RelativeColumn(1); // Apr
                                    columns.RelativeColumn(1); // May
                                    columns.RelativeColumn(1); // Jun
                                    columns.RelativeColumn(1); // Jul
                                    columns.RelativeColumn(1); // Aug
                                    columns.RelativeColumn(1); // Sep
                                    columns.RelativeColumn(1); // Oct
                                    columns.RelativeColumn(1); // Nov
                                    columns.RelativeColumn(1); // Dec
                                });

                                // Header
                                table.Header(header =>
                                {
                                    foreach (DataColumn col in currentReportData.Columns)
                                    {
                                        header.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(5)
                                            .AlignCenter()
                                            .Text(col.ColumnName)
                                            .FontColor(Colors.White)
                                            .Bold();
                                    }
                                });

                                // Data rows
                                foreach (DataRow row in currentReportData.Rows)
                                {
                                    foreach (var item in row.ItemArray)
                                    {
                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(5)
                                            .AlignCenter()
                                            .Text(item?.ToString() ?? "0");
                                    }
                                }
                            });

                            // Add summary statistics
                            column.Item().PaddingTop(15).Column(summaryColumn =>
                            {
                                summaryColumn.Item().Text("Summary Statistics")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                int totalSessions = 0;
                                foreach (DataRow row in currentReportData.Rows)
                                {
                                    for (int i = 1; i < currentReportData.Columns.Count; i++)
                                    {
                                        totalSessions += Convert.ToInt32(row[i]);
                                    }
                                }

                                summaryColumn.Item().PaddingTop(5).Text($"Total Study Sessions: {totalSessions}")
                                    .FontSize(11);
                                summaryColumn.Item().Text($"Number of Stacks: {currentReportData.Rows.Count}")
                                    .FontSize(11);
                                summaryColumn.Item().Text($"Average Sessions per Stack: {(currentReportData.Rows.Count > 0 ? (double)totalSessions / currentReportData.Rows.Count : 0):F2}")
                                    .FontSize(11);
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            })
            .GeneratePdf(filePath);
        }
    }
}
