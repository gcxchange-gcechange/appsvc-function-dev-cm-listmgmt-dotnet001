using Microsoft.Extensions.Configuration;

namespace appsvc_function_dev_cm_listmgmt_dotnet001
{
    public static class PropertyAliasMapper
    {
        private static Dictionary<string, string> _aliases = new();

        public static void LoadAliases(IConfiguration config)
        {
            var allKeys = config.AsEnumerable()
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value,
                    StringComparer.OrdinalIgnoreCase
                );

            _aliases = typeof(JobOpportunity)
                .GetProperties()
                .ToDictionary(
                    prop => prop.Name,
                    prop =>
                    {
                        var key = $"{prop.Name}_Alias";
                        return allKeys.TryGetValue(key, out var value) ? value : prop.Name;
                    });
        }

        public static string GetAlias(string propertyName)
        {
            if (_aliases.TryGetValue(propertyName, out var alias))
            {
                return alias;
            }
            else
            {
                throw new ArgumentException($"Missing {propertyName}_Alias setting in app configuration!", propertyName);
            }
        }
    }
}
