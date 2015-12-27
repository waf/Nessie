using MarkdownDeep;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie.Services
{
    /// <summary>
    /// Markdown converter, using MarkdownDeep
    /// </summary>
    public class MarkdownConverter
    {
        private static Markdown Markdown = new Markdown
        {
            ExtraMode = true,
            SafeMode = false,
            AutoHeadingIDs = true,
            MarkdownInHtml = true,
        };

        public string Convert(string input)
        {
            string html = Markdown.Transform(input).Trim();
            return html;
        }
    }
}
