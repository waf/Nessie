using DotLiquid.FileSystems;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid.Exceptions;
using DotLiquid;

namespace Nessie.Services
{
    /// <summary>
    /// This implements an abstract file system which retrieves template files.
    /// File names are automatically prefixed with '_partial_'
    /// For security reasons, template paths are only allowed to contain letters, numbers, and underscore.
    ///
    /// Example:
    ///
    /// file_system = Liquid::LocalFileSystem.new("/some/path")
    ///
    /// file_system.full_path("mypartial") # => "/some/path/_partial_mypartial"
    /// file_system.full_path("dir/mypartial.html") # => "/some/path/dir/_partial_mypartial.html"
    /// </summary>
    class NessieLiquidFileSystem : IFileSystem
    {
        public string Root { get; set; }

        public NessieLiquidFileSystem(string root)
        {
            Root = root;
        }

        public string ReadTemplateFile(Context context, string templateName)
        {
            string templatePath = (string) context[templateName];
            string fullPath = FullPath(templatePath);
            if (!File.Exists(fullPath))
                throw new FileSystemException("Template not found", templatePath);
            return File.ReadAllText(fullPath);
        }

        public string FullPath(string templatePath)
        {
            if (templatePath == null || !Regex.IsMatch(templatePath, @"^[^.\/][a-zA-Z0-9_\/]+\.?[a-zA-Z0-9_]*$"))
            {
                throw new FileSystemException("Illegal template path", templatePath);
            }

            string fullPath = templatePath.Contains("/")
                ? Path.Combine(Path.Combine(Root, Path.GetDirectoryName(templatePath)), string.Format("_partial_{0}", Path.GetFileName(templatePath)))
                : Path.Combine(Root, string.Format("_partial_{0}", templatePath));

            string escapedPath = Root.Replace(@"\", @"\\").Replace("(", @"\(").Replace(")", @"\)");
            if (!Regex.IsMatch(Path.GetFullPath(fullPath), string.Format("^{0}", escapedPath)))
            {
                throw new FileSystemException("Illegal template full path", Path.GetFullPath(fullPath));
            }

            return fullPath;
        }
    }
}
