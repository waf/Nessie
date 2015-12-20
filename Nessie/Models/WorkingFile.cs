using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie
{
    /// <summary>
    /// File manipulation is pretty important for a static site generator.
    /// To avoid a lot of path/filename string-joining/string-splitting, the code works with this representation of a file.
    /// </summary>
    class FileLocation
    {
        public FileLocation(string directory, string fileNameWithoutExtension, string extension)
        {
            this.Directory = directory;
            this.FileNameWithoutExtension = fileNameWithoutExtension;
            this.Extension = extension;
            this.FullyQualifiedName = this.Directory + Path.DirectorySeparatorChar + this.FileNameWithoutExtension + "." + this.Extension;
        }

        public FileLocation(string fullFilePath)
        {
            this.Directory = Path.GetDirectoryName(fullFilePath);
            this.FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilePath);
            this.Extension = Path.GetExtension(fullFilePath);
            this.FullyQualifiedName = fullFilePath;
        }

        public string Directory { get; private set; }
        public string Extension { get; private set; }
        public string FileNameWithoutExtension { get; private set; }
        public string FullyQualifiedName { get; private set; }
    }
}
