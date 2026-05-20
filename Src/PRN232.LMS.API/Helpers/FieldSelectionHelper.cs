using System.Reflection;

namespace PRN232.LMS.API.Helpers;

public static class FieldSelectionHelper
{
    public static bool TryValidateFields<T>(string? fields, out string[] invalidFields)
    {
        invalidFields = [];
        var requestedFields = ParseFields(fields);

        if (requestedFields.Count == 0)
        {
            return true;
        }

        var availableFields = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        invalidFields = requestedFields
            .Where(field => !availableFields.Contains(field))
            .ToArray();

        return invalidFields.Length == 0;
    }

    public static IReadOnlyList<object> ShapeCollection<T>(IEnumerable<T> items, string? fields)
    {
        var requestedFields = ParseFields(fields);

        if (requestedFields.Count == 0)
        {
            return items.Cast<object>().ToList();
        }

        var properties = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

        return items
            .Select(item => ShapeObject(item, requestedFields, properties))
            .ToList();
    }

    private static Dictionary<string, object?> ShapeObject<T>(
        T item,
        IReadOnlyList<string> requestedFields,
        IReadOnlyDictionary<string, PropertyInfo> properties)
    {
        var shaped = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in requestedFields)
        {
            var property = properties[field];
            shaped[property.Name] = property.GetValue(item);
        }

        return shaped;
    }

    private static IReadOnlyList<string> ParseFields(string? fields)
        => string.IsNullOrWhiteSpace(fields)
            ? []
            : fields
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
}
