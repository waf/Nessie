using DotLiquid;
using MarkdownDeep;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nessie.Services
{
    using FileReader = Func<string, string>;
    using FileWriter = Action<string, string>;
    class DirectoryGenerator
    {
        private readonly static Regex layout = new Regex(@"_(?<category>.+)_.*");
        private readonly FileReader ReadFile;
        private readonly FileWriter WriteFile;
        private readonly FileGenerator fileGenerator;

        public DirectoryGenerator(
            // poor-man's dependency injection for mocking purposes
            FileReader readFile = null, FileWriter writeFile = null, FileGenerator fileGenerator = null)
        {
            this.ReadFile = readFile ?? File.ReadAllText;
            this.WriteFile = writeFile ?? File.WriteAllText;
            this.fileGenerator = fileGenerator ?? new FileGenerator();
        }

        public void Generate(string root, IList<string> inputFiles, string outputRoot)
        {
            string autoContent = ReadFile(Path.Combine(root, "_auto.html"));

            /* this next statement is a bit tricky, it takes advantage of laziness and a recursive variable definition.
               we want all template variables to be available to all templates when they're rendering.

               given files _post_a.md, _post_b.md, _product_x.md, it will generate a dictionary:
                    post => _post_a.md's variables, _post_b.md's variables
                    product => _product_x's variables
            */
            Dictionary<string, IBuffer<Hash>> allTemplateVariables = null;
            allTemplateVariables = inputFiles
                .Select(file => new FileLocation(file))
                .Where(file => file.Extension == ".md")
                .GroupBy(file => ReadCategoryFromFileName(file)) // this gets the template variable keys available for the templates
                .ToDictionary(
                    fileGroup => fileGroup.Key,
                    fileGroup => fileGroup
                                    .Select(file => fileGenerator.GenerateFile(file, ReadFile(file.FullyQualifiedName), autoContent, allTemplateVariables))
                                    .Do(outputFile => WriteFileAndDirectories(outputRoot, outputFile.Name, outputFile.Output))
                                    .Select(context => context.Variables)
                                    .Memoize());

            // evaluate the lazy values in the dictionary, to generate all the files.
            foreach (var item in allTemplateVariables)
            {
                Console.WriteLine("Evaluating " + item.Key);
                item.Value.ToList();
            }
        }

        private void WriteFileAndDirectories(string outputRoot, FileLocation file, string output)
        {
            string fullDirectory = Path.Combine(outputRoot, file.Directory);
            string fullPath = Path.Combine(outputRoot, file.FullyQualifiedName);
            Directory.CreateDirectory(fullDirectory);
            WriteFile(fullPath, output);
        }

        /// <summary>
        /// Given a filename of the pattern _foo_xyz, return "foo"
        /// Or empty string if the filename is some other pattern.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string ReadCategoryFromFileName(FileLocation file)
        {
            return layout.Match(file.FileNameWithoutExtension).Groups["category"].Value;
        }
    }
}