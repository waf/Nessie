using DotLiquid;
using Nessie.Services.Processors;
using Nessie.Services.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nessie.Tests.Integration
{
    class FakeFileSystem
    {
        public const string Root = @"Z:\FakeFileSystem\Project";

        public IDictionary<string, string> InputFiles { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> OutputFiles { get; set; } = new Dictionary<string, string>();
        public FileOperation FileOperation { get; }

        public FakeFileSystem()
        {
            FileOperation = new FileOperation(
                readFile: file => InputFiles[file],
                writeFile: (file, content) => OutputFiles[file] = content,
                fileExists: file => InputFiles.ContainsKey(file),
                createDirectory: dir => { },
                fileCopy: (source, dest, overwrite) => { });

            // Unfortunately DotLiquid uses a mutable static field for its reference to the root directory.
            // It's fine for non-test scenarios because the root won't change, but it makes unit testing
            // with different roots a bit of a pain. So we define the static field centrally so all unit tests
            // use the same root.
            Template.FileSystem = new NessieLiquidFileSystem(FileOperation, Root);
        }
    }
}
