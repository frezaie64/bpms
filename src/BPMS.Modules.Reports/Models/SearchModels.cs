namespace BPMS.Modules.Reports.Models;

public record SearchResponse(
    List<SearchResultItem> Results,
    int TotalCount
);

public record SearchResultItem(
    string Type,
    Guid Id,
    string Title,
    string? Description,
    string? Status
);