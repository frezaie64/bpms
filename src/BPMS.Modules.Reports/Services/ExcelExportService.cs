using ClosedXML.Excel;

namespace BPMS.Modules.Reports.Services;

public class ExcelExportService
{
    public byte[] ExportToExcel<T>(IReadOnlyList<T> data, string sheetName = "Report")
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        if (data.Count == 0)
        {
            worksheet.Cell(1, 1).Value = "No data available";
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        var properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).ToList();

        for (int col = 0; col < properties.Count; col++)
        {
            worksheet.Cell(1, col + 1).Value = properties[col].Name;
            worksheet.Cell(1, col + 1).Style.Font.Bold = true;
        }

        for (int row = 0; row < data.Count; row++)
        {
            for (int col = 0; col < properties.Count; col++)
            {
                var value = properties[col].GetValue(data[row]);

                if (value == null)
                    worksheet.Cell(row + 2, col + 1).Value = Blank.Value;
                else if (value is DateTime dt)
                    worksheet.Cell(row + 2, col + 1).Value = dt.ToString("yyyy-MM-dd HH:mm:ss");
                else
                    worksheet.Cell(row + 2, col + 1).Value = value.ToString();
            }
        }

        worksheet.Columns().AdjustToContents();

        var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
    }
}