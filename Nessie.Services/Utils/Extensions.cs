using DotLiquid;
using System.Collections.Immutable;
using System.IO;

namespace Nessie
{
    public static class Extensions
    {
        public static ImmutableDictionary<string, object> AsTemplateValues<T>(this ImmutableDictionary<string, T> templateValues)
        {
            // hack to get around lack of safe covariance in IDictionary
            var objectDictionary = templateValues.ToImmutableDictionary(k => k.Key, k => (object)k.Value);
            return objectDictionary;
        }

        public static bool TryGetVariable(this Hash hash, string key, out string value)
        {
            var success = hash.TryGetValue(key, out object result);
            value = success ? (string)result : null;
            return success;
        }

        public static ImmutableDictionary<TKey, TValue> TrySetItem<TKey, TValue>(this ImmutableDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if(!dict.ContainsKey(key))
            {
                return dict.SetItem(key, value);
            }
            return dict;
        }

        public static string NormalizeDirectorySeparators(this string path) =>
            path.Replace('\\', '/');
    }
}
