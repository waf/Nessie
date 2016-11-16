using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nessie.Services.Converters
{
    /// <summary>
    /// Markdown converter, using Pandoc
    /// </summary>
    public class MarkdownConverter
    {
        private static string PandocLocation;

        static MarkdownConverter()
        {
            string currentDirectory =
                Path.GetDirectoryName(
                    typeof(MarkdownConverter).GetTypeInfo().Assembly.Location
                );
            PandocLocation = Path.Combine(currentDirectory, @"lib\pandoc.exe");
        }

        public string Convert(string source)
        {
            string args = "-f markdown -t html";
            Process p = new Process
            {
                StartInfo = new ProcessStartInfo(PandocLocation, args)
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            p.Start();

            p.StandardInput.Write(source);
            p.StandardInput.Dispose();
            p.WaitForExit(2000);

            return p.StandardOutput.ReadToEnd();
        }
    }
}
