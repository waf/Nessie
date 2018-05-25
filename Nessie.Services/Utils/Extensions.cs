using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie
{
    public static class Extensions
    {
        public static string RenderFromStringDictionary<T>(this Template template, IDictionary<string, T> templateValuesDictionary)
        {
            var hash = templateValuesDictionary.AsTemplateValues();
            return template.Render(hash);
        }

        public static Hash AsTemplateValues<T>(this IDictionary<string, T> templateValues)
        {
            // hack to get around lack of safe covariance in IDictionary
            var objectDictionary = templateValues.ToDictionary(k => k.Key, k => (object)k.Value);
            return Hash.FromDictionary(objectDictionary);
        }

        public static bool TryGetVariable(this Hash hash, string key, out string value)
        {
            var success = hash.TryGetValue(key, out object result);
            value = success ? (string)result : null;
            return success;
        }
    }
}
