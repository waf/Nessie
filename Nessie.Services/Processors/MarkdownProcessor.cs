using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Nessie.Services.Processors
{
    /// <summary>
    /// Markdown processor, using Pandoc
    /// </summary>
    public class MarkdownProcessor
    {
        private static readonly string PandocLocation =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"lib\pandoc.exe");

        public string Convert(string source, ImmutableDictionary<string, object> environment)
        {
            var args = environment[Settings.MarkdownSettings].ToString();
            Process p = new Process
            {
                StartInfo = new ProcessStartInfo(PandocLocation, args)
                {
                    StandardOutputEncoding = Encoding.UTF8,
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
