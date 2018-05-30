using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Nessie
{
    /// <summary>
    /// File manipulation is important for a static site generator.
    /// To avoid a lot of ad-hoc path string joining/splitting, the code works with this representation of a file.
    /// </summary>
    public class FileLocation
    {
        private readonly static Regex layout = new Regex(@"_(?<category>.+)_.*");

        public FileLocation(string directory, string fileNameWithoutExtension, string extension)
        {
            this.Directory = directory;
            this.FileNameWithoutExtension = fileNameWithoutExtension;
            this.Extension = extension;
            this.FullyQualifiedName = Path.Combine(this.Directory, this.FileNameWithoutExtension + this.Extension);
            this.Category = layout.Match(FileNameWithoutExtension).Groups["category"].Value;
        }

        public FileLocation(string fullFilePath)
        {
            this.Directory = Path.GetDirectoryName(fullFilePath);
            this.FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilePath).TrimEnd('.');
            this.Extension = Path.GetExtension(fullFilePath);
            this.FullyQualifiedName = fullFilePath;
            this.Category = layout.Match(FileNameWithoutExtension).Groups["category"].Value;
        }

        public string Directory { get; }

        public string FullyQualifiedName { get; }

        public string FileNameWithoutExtension { get; }

        /// <summary>
        /// File extension, with the leading period.
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// The prefixed category of a file.
        /// e.g. The filename "_foo_bar.txt" has a category of "foo"
        /// </summary>
        public string Category { get; }

        public override string ToString()
        {
            return $"{FullyQualifiedName} ({Category})";
        }

        public override bool Equals(object obj)
        {
            return obj is FileLocation location
                   && Directory == location.Directory
                   && FileNameWithoutExtension == location.FileNameWithoutExtension
                   && Extension == location.Extension
                   && FullyQualifiedName == location.FullyQualifiedName
                   && Category == location.Category;
        }

        public override int GetHashCode()
        {
            var hashCode = 2039212844;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Directory);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileNameWithoutExtension);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Extension);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullyQualifiedName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Category);
            return hashCode;
        }
    }
}
