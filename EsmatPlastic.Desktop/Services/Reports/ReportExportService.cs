using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using EsmatPlastic.Desktop.Models.Reports;

namespace EsmatPlastic.Desktop.Services.Reports;

public class ReportExportService
{
    public void ExportToCsv(IEnumerable<StockReportResponse> items, string filePath)
    {
        var sb = new StringBuilder();

        // Write CSV Header with UTF-8 BOM
        sb.AppendLine("المنتج,الصنف,المقاس,الوارد,الصادر,المتاح الحقيقي");

        foreach (var item in items)
        {
            string product = EscapeCsv(item.ProductName);
            string variant = EscapeCsv(item.VariantName);
            string size = EscapeCsv(item.Size ?? string.Empty);
            string totalIn = item.TotalIn.ToString("G");
            string totalOut = item.TotalOut.ToString("G");
            string currentQty = item.CurrentQuantity.ToString("G");

            sb.AppendLine($"\"{product}\",\"{variant}\",\"{size}\",{totalIn},{totalOut},{currentQty}");
        }

        // Use UTF-8 with BOM for Excel compatibility with Arabic
        File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
    }

    private static string EscapeCsv(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text.Replace("\"", "\"\"");
    }

    public void PrintReport(
        IEnumerable<StockReportResponse> items,
        decimal totalIn,
        decimal totalOut,
        decimal currentStock,
        int variantCount)
    {
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true)
            return;

        var doc = new FlowDocument
        {
            PagePadding = new Thickness(40),
            ColumnWidth = printDialog.PrintableAreaWidth,
            FlowDirection = FlowDirection.RightToLeft,
            FontFamily = new FontFamily("Segoe UI, Tahoma")
        };

        // Header Title
        var headerPara = new Paragraph(new Run("تقرير حركة ورصيد المخزون"))
        {
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(13, 148, 136)),
            Margin = new Thickness(0, 0, 0, 4)
        };
        doc.Blocks.Add(headerPara);

        // Date & Time Subtitle
        var datePara = new Paragraph(new Run($"تاريخ التصدير: {DateTime.Now:yyyy-MM-dd HH:mm:ss} | شركة عصمت للبلاستيك"))
        {
            FontSize = 11,
            TextAlignment = TextAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
            Margin = new Thickness(0, 0, 0, 16)
        };
        doc.Blocks.Add(datePara);

        // Summary Statistics Box
        var summaryTable = new Table { CellSpacing = 6 };
        summaryTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        summaryTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        summaryTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        summaryTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

        var summaryGroup = new TableRowGroup();
        var summaryRow = new TableRow();

        summaryRow.Cells.Add(CreateStatCell("إجمالي الوارد", totalIn.ToString("N0")));
        summaryRow.Cells.Add(CreateStatCell("إجمالي الصادر", totalOut.ToString("N0")));
        summaryRow.Cells.Add(CreateStatCell("المخزون الحقيقي", currentStock.ToString("N0")));
        summaryRow.Cells.Add(CreateStatCell("عدد الأصناف", variantCount.ToString()));

        summaryGroup.Rows.Add(summaryRow);
        summaryTable.RowGroups.Add(summaryGroup);
        doc.Blocks.Add(summaryTable);

        // Spacer
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 12, 0, 0) });

        // Data Table
        var table = new Table { CellSpacing = 0, BorderThickness = new Thickness(1), BorderBrush = Brushes.LightGray };
        table.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) }); // Product
        table.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) }); // Variant
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // Size
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // In
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // Out
        table.Columns.Add(new TableColumn { Width = new GridLength(1.2, GridUnitType.Star) }); // Balance

        var headerGroup = new TableRowGroup();
        var headerRow = new TableRow { Background = new SolidColorBrush(Color.FromRgb(241, 245, 249)) };

        headerRow.Cells.Add(CreateHeaderCell("المنتج"));
        headerRow.Cells.Add(CreateHeaderCell("الصنف"));
        headerRow.Cells.Add(CreateHeaderCell("المقاس"));
        headerRow.Cells.Add(CreateHeaderCell("الوارد"));
        headerRow.Cells.Add(CreateHeaderCell("الصادر"));
        headerRow.Cells.Add(CreateHeaderCell("المتاح"));

        headerGroup.Rows.Add(headerRow);
        table.RowGroups.Add(headerGroup);

        var dataGroup = new TableRowGroup();
        bool isAlternate = false;

        foreach (var item in items)
        {
            var row = new TableRow
            {
                Background = isAlternate ? new SolidColorBrush(Color.FromRgb(248, 250, 252)) : Brushes.White
            };

            row.Cells.Add(CreateDataCell(item.ProductName, false, true));
            row.Cells.Add(CreateDataCell(item.VariantName, false, false));
            row.Cells.Add(CreateDataCell(item.Size ?? "-", true, false));
            row.Cells.Add(CreateDataCell(item.TotalIn.ToString("N0"), true, false));
            row.Cells.Add(CreateDataCell(item.TotalOut.ToString("N0"), true, false));
            row.Cells.Add(CreateDataCell(item.CurrentQuantity.ToString("N0"), true, true));

            dataGroup.Rows.Add(row);
            isAlternate = !isAlternate;
        }

        table.RowGroups.Add(dataGroup);
        doc.Blocks.Add(table);

        // Print Document
        IDocumentPaginatorSource paginator = doc;
        printDialog.PrintDocument(paginator.DocumentPaginator, "تقرير حركة المخزون");
    }

    private TableCell CreateStatCell(string title, string value)
    {
        var cell = new TableCell
        {
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(8),
            Background = new SolidColorBrush(Color.FromRgb(248, 250, 252))
        };

        var pTitle = new Paragraph(new Run(title))
        {
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
            TextAlignment = TextAlignment.Center
        };
        var pValue = new Paragraph(new Run(value))
        {
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };

        cell.Blocks.Add(pTitle);
        cell.Blocks.Add(pValue);
        return cell;
    }

    private TableCell CreateHeaderCell(string text)
    {
        return new TableCell
        {
            Padding = new Thickness(6, 8, 6, 8),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Blocks = { new Paragraph(new Run(text)) { FontWeight = FontWeights.Bold, FontSize = 11 } }
        };
    }

    private TableCell CreateDataCell(string text, bool center, bool bold)
    {
        return new TableCell
        {
            Padding = new Thickness(6, 6, 6, 6),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(0, 0, 0, 0.5),
            Blocks =
            {
                new Paragraph(new Run(text))
                {
                    FontSize = 11,
                    TextAlignment = center ? TextAlignment.Center : TextAlignment.Right,
                    FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal
                }
            }
        };
    }
}
