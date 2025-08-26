namespace BookWooks.OrderApi.Infrastructure.AiServices;
public static class JsonHelpers
{
    public static bool TryGetNonEmptyArrayProperty(JsonElement json, string propertyName, out JsonElement arrayElement)
    {
        arrayElement = default;
        if (json.TryGetProperty(propertyName, out var prop) &&
            prop.ValueKind == JsonValueKind.Array &&
            prop.GetArrayLength() > 0)
        {
            arrayElement = prop;
            return true;
        }
        return false;
    }
}
