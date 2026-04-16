using System.Text.Json;

namespace CleanArchitecture.Infrastructure.Options;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        IncludeFields = true // Useful if your events use fields instead of properties
    };
}
