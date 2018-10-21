using DotLiquid.FileSystems;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using DotLiquid.Exceptions;
using DotLiquid;
using Nessie.Services.Utils;

namespace Nessie.Services.Processors
{
    /// <summary>
    /// Required integration point for DotLiquid library.
    /// 
    /// This implements an abstract file system which retrieves template files.
    /// File names are automatically prefixed with '_partial_'
    /// For security reasons, template paths are only allowed to contain letters, numbers, and underscore.
    ///
    /// Example (from original ruby implementation):
    ///
    /// file_system = Liquid::LocalFileSystem.new("/some/path")
    ///
    /// file_system.full_path("mypartial") # => "/some/path/_partial_mypartial"
    /// file_system.full_path("dir/mypartial.html") # => "/some/path/dir/_partial_mypartial.html"
    /// </summary>
    public class NessieLiquidFileSystem : IFileSystem
    {
        private readonly string root;
        private readonly FileOperation fileio;
        private readonly static Regex TemplatePathValidator = new Regex(@"^[^.\/\\][a-zA-Z0-9_\/\\]+\.?[a-zA-Z0-9_]*$");
        private const string PartialPrefix = "_partial_";

        public NessieLiquidFileSystem(FileOperation fileio, string root)
        {
            this.fileio = fileio;
            this.root = root.NormalizeDirectorySeparators();
        }

        public string ReadTemplateFile(Context context, string templateName)
        {
            string templatePath = (string) context[templateName];

            string fullPath = FullPath(templatePath);
            if (!fileio.FileExists(fullPath))
                throw new FileSystemException("Template not found", templatePath);
            return fileio.ReadFile(fullPath);
        }

        private string FullPath(string templatePath)
        {
            if (templatePath == null || !TemplatePathValidator.IsMatch(templatePath))
            {
                throw new FileSystemException("Illegal template path", templatePath);
            }
            templatePath = templatePath.NormalizeDirectorySeparators();

            string fullPath = templatePath.Contains('/')
                ? Path.Combine(Path.Combine(root, Path.GetDirectoryName(templatePath)), PartialPrefix + Path.GetFileName(templatePath))
                : Path.Combine(root, PartialPrefix + templatePath);

            string escapedPath = root.Replace(@"\", @"\\").Replace("(", @"\(").Replace(")", @"\)");
            if (!Regex.IsMatch(fullPath, string.Format("^{0}", escapedPath)))
            {
                throw new FileSystemException("Illegal template full path", Path.GetFullPath(fullPath));
            }

            return fullPath.NormalizeDirectorySeparators();
        }
    }
}
