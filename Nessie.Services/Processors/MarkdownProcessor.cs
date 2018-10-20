using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Nessie.Services.Processors
{
    /// <summary>
    /// Markdown processor, using Pandoc
    /// </summary>
    public class MarkdownProcessor
    {
        private readonly string executable;

        public MarkdownProcessor(string executable = "pandoc")
        {
            this.executable = executable;
        }

        public string Convert(string source, ImmutableDictionary<string, object> environment)
        {
            var args = environment[Settings.PandocSettings].ToString();
            Process p = new Process
            {
                StartInfo = new ProcessStartInfo(executable, args)
                {
                    StandardOutputEncoding = Encoding.UTF8,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            try
            {
                p.Start();
            }
            catch (Exception ex) when (ex.Message == "The system cannot find the file specified")
            {
                throw new ErrorMessageException(
                    $"Could not find {executable} on the path. Is it installed?" + Environment.NewLine + InstallationHelp
                );
            }

            p.StandardInput.Write(source);
            p.StandardInput.Dispose();
            p.WaitForExit(2000);

            return p.StandardOutput.ReadToEnd();
        }

        private string InstallationHelp =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"Try running `choco install {executable}`" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"Try running `brew install {executable}`" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? $"Try installing the `{executable}` package" :
            null;
    }
}
