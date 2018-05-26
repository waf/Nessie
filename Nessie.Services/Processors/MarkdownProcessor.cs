using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Nessie.Services.Processors
{
    /// <summary>
    /// Markdown processor, using Pandoc
    /// </summary>
    public class MarkdownProcessor
    {
        private static readonly string PandocLocation;

        static MarkdownProcessor()
        {
            string currentDirectory =
                Path.GetDirectoryName(
                    typeof(MarkdownProcessor).GetTypeInfo().Assembly.Location
                );
            PandocLocation = Path.Combine(currentDirectory, @"lib\pandoc.exe");
        }

        public string Convert(string source)
        {
            const string args = "-f markdown -t html";
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
