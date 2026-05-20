namespace PRN232.LMS.Services.Helpers;

public static class QueryParser
{
    public static IReadOnlyList<string> SplitCommaSeparated(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    public static HashSet<string> ParseToSet(string? value)
        => SplitCommaSeparated(value)
            .Select(item => item.ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
