using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nessie.Services.Utils
{
    public class FileOperation
    {
        private readonly ReadFileFunction readFile;
        private readonly WriteFileFunction writeFile;
        private readonly CreateDirectoryFunction createDirectory;
        private readonly FileCopyFunction fileCopy;

        public delegate string ReadFileFunction(string path);
        public delegate void WriteFileFunction(string path, string text);
        public delegate void CreateDirectoryFunction(string path);
        public delegate void FileCopyFunction(string sourceFileName, string destFileName, bool overwrite);

        public FileOperation()
            : this(File.ReadAllText,
                   File.WriteAllText,
                   CreateDirectory,
                   File.Copy)
        {
        }

        public FileOperation(
            ReadFileFunction readFile,
            WriteFileFunction writeFile,
            CreateDirectoryFunction createDirectory,
            FileCopyFunction fileCopy)
        {
            this.readFile = readFile;
            this.writeFile = writeFile;
            this.createDirectory = createDirectory;
            this.fileCopy = fileCopy;
        }

        public string ReadFile(string path) => readFile(path);

        public void WriteFile(string path, string text) => writeFile(path, text);

        public void CopyFiles(string inputRoot, string outputRoot, FileLocation[] files)
        {
            foreach (var file in files)
            {
                string outputFileRelativeToInputRoot = MakeFileRelativeToPath(file, inputRoot);
                string outputLocation = Path.Combine(outputRoot, outputFileRelativeToInputRoot);
                Console.WriteLine($"Copying {outputFileRelativeToInputRoot}");
                createDirectory(outputLocation);
                fileCopy(file.FullyQualifiedName, outputLocation, true);
            }
        }

        public void WriteFileAndDirectories(string outputRoot, string inputRoot, FileLocation file, string output)
        {
            if(string.IsNullOrWhiteSpace(output))
            {
                return;
            }
            string outputFileRelativeToInputRoot = MakeFileRelativeToPath(file, inputRoot);
            string outputLocation = Path.Combine(outputRoot, outputFileRelativeToInputRoot);
            createDirectory(outputLocation);
            Console.WriteLine("Generating " + outputFileRelativeToInputRoot);
            WriteFile(outputLocation, output);
        }

        private static string MakeFileRelativeToPath(FileLocation file, string path)
        {
            return file.FullyQualifiedName.StartsWith(path)
                ? file.FullyQualifiedName.Substring(path.Length).TrimStart(Path.DirectorySeparatorChar)
                : file.FullyQualifiedName;
        }

        private static void CreateDirectory(string directory) =>
            Directory.CreateDirectory(Path.GetDirectoryName(directory));
    }
}
