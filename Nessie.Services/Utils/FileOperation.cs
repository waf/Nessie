using System;
using System.IO;

namespace Nessie.Services.Utils
{
    /// <summary>
    /// Abstraction of file and directory operations, mostly to make other classes more unit testable.
    /// </summary>
    public class FileOperation
    {
        private readonly ReadFileFunction readFile;
        private readonly WriteFileFunction writeFile;
        private readonly FileExistsFunction fileExists;
        private readonly CreateDirectoryFunction createDirectory;
        private readonly FileCopyFunction fileCopy;

        public delegate string ReadFileFunction(string path);
        public delegate void WriteFileFunction(string path, string text);
        public delegate bool FileExistsFunction(string path);
        public delegate void CreateDirectoryFunction(string path);
        public delegate void FileCopyFunction(string sourceFileName, string destFileName, bool overwrite);

        // this constructor is used during actual runtime
        public FileOperation()
            : this(File.ReadAllText,
                   File.WriteAllText,
                   File.Exists,
                   CreateDirectory,
                   File.Copy)
        {
        }

        // this constructor can be used during unit tests so they don't touch the filesystem
        public FileOperation(
            ReadFileFunction readFile,
            WriteFileFunction writeFile,
            FileExistsFunction fileExists,
            CreateDirectoryFunction createDirectory,
            FileCopyFunction fileCopy)
        {
            this.readFile = readFile;
            this.writeFile = writeFile;
            this.fileExists = fileExists;
            this.createDirectory = createDirectory;
            this.fileCopy = fileCopy;
        }

        public string ReadFile(string path) => readFile(path);

        public void WriteFile(string path, string text) => writeFile(path, text);

        public bool FileExists(string fullPath) => fileExists(fullPath);

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
