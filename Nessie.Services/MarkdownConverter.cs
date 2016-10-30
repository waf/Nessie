using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nessie.Services
{
    /// <summary>
    /// Markdown converter, using MarkdownDeep
    /// </summary>
    public class MarkdownConverter
    {
        private static string PandocLocation;

        static MarkdownConverter()
        {
            PandocLocation =
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    @"lib\pandoc.exe"
                );
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
            p.StandardInput.Close();
            p.WaitForExit(2000);

            return p.StandardOutput.ReadToEnd();
        }
    }
}
