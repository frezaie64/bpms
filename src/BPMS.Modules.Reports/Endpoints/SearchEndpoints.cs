using BPMS.Modules.Reports.Models;
using BPMS.Modules.Reports.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.Reports.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization();

        group.MapGet("/search", async (IReportsModule module, string q, string? format, ExcelExportService excel, PdfExportService pdf, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.BadRequest("Query parameter 'q' is required.");

            var data = await module.SearchAsync(q, ct);

            if (format == "excel")
            {
                var bytes = excel.ExportToExcel(data.Results, "SearchResults");
                return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "search_results.xlsx");
            }
            if (format == "pdf")
            {
                var columns = new[] { "Type", "Title", "Description", "Status" };
                var bytes = pdf.ExportTableToPdf(data.Results, $"Search Results for '{q}'", columns,
                    d => new string?[] { d.Type, d.Title, d.Description, d.Status });
                return Results.File(bytes, "application/pdf", "search_results.pdf");
            }

            return Results.Ok(data);
        })
        .WithName("Search")
        .WithTags("Search");
    }
}