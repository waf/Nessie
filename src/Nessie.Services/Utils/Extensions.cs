using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie
{
    static class Extensions
    {
        public static string RenderFromStringDictionary<T>(this Template template, IDictionary<string, T> templateValues)
        {
            // hack to get around lack of safe covariance in IDictionary
            var objectDictionary = templateValues.ToDictionary(k => k.Key, k => (object)k.Value);
            return template.Render(Hash.FromDictionary(objectDictionary));
        }

        public static Hash AsTemplateValues<T>(this IDictionary<string, T> templateValues)
        {
            // hack to get around lack of safe covariance in IDictionary
            var objectDictionary = templateValues.ToDictionary(k => k.Key, k => (object)k.Value);
            return Hash.FromDictionary(objectDictionary);
        }

        public static bool TryGetVariable(this Hash hash, string key, out string value)
        {
            object result;
            var success = hash.TryGetValue(key, out result);
            value = success ? (string)result : null;
            return success;
        }
    }
}
