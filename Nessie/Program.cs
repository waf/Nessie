using Nessie.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie
{
    class Program
    {
        static void Main(string[] args)
        {
            string root = "example";
            var files = Directory
                .GetFiles(root, "*", SearchOption.AllDirectories)
                .ToList();

            var generator = new ProjectGenerator();
            generator.Generate(root, files, "_output2");
            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
