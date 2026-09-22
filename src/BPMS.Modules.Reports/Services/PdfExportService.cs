using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BPMS.Modules.Reports.Services;

public class PdfExportService
{
    public byte[] ExportTableToPdf<T>(IReadOnlyList<T> data, string title, string[] columns, Func<T, string?[]> rowSelector)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c =>
                {
                    c.Column(col =>
                    {
                        col.Item().Text(title).FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });

                page.Content().Element(c =>
                {
                    if (data.Count == 0)
                    {
                        c.Text("No data available").FontSize(12).FontColor(Colors.Grey.Medium);
                        return;
                    }

                    c.Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            foreach (var _ in columns)
                                cols.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            foreach (var col in columns)
                                header.Cell().Element(cellStyle).Text(col).Bold();
                        });

                        foreach (var item in data)
                        {
                            var rowValues = rowSelector(item);
                            foreach (var val in rowValues)
                                table.Cell().Element(cellStyle).Text(val ?? string.Empty);
                        }

                        static IContainer cellStyle(IContainer container) =>
                            container.Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8));
                    text.Span("Page ");
                    text.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }
}