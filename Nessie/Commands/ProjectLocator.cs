using System.IO;
using System.Linq;

namespace Nessie.Commands
{
    public static class ProjectLocator
    {
        public const string OutputDirectory = "_output";

        public static string FindProjectDirectory() =>
            FindProjectDirectory(Path.GetFullPath("."));

        public static string FindProjectDirectory(string currentPath)
        {
            string currentPathIteration = currentPath;
            do
            {
                if (DirectoryContainsOutputDirectory(currentPathIteration))
                {
                    return currentPathIteration;
                }
                currentPathIteration = Directory.GetParent(currentPathIteration)?.FullName;
            } while (currentPathIteration != null);

            // no path found, return the original
            return currentPath;
        }

        private static bool DirectoryContainsOutputDirectory(string currentPathIteration) =>
            Directory
                .GetDirectories(currentPathIteration)
                .Any(fullpath => Path.GetFileName(fullpath) == OutputDirectory);
    }
}
