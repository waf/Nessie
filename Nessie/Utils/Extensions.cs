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
    }
}
