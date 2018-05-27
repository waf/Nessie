using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services;
using Nessie.Services.Processors;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Nessie.Tests.Integration
{
    [TestClass]
    public class FileGeneratorTests
    {
        private FileGenerator generator;

        [TestInitialize]
        public void Initialize()
        {
            var fs = new FakeFileSystem();
            generator = new FileGenerator(new TemplateProcessor(fs.FileOperation), new MarkdownProcessor());
        }

        [TestMethod]
        public void GenerateFile_Markdown_OutputsHtml()
        {
            var output = generator.GenerateFile(".",
                new FileLocation("directory/index.md"),
                "Hello",
                Array.Empty<string>(),
                new Dictionary<string, IBuffer<ImmutableDictionary<string, object>>>().ToImmutableDictionary()
            );

            Assert.AreEqual(@"directory\index.html", output.Name.FullyQualifiedName);
            Assert.AreEqual("<p>Hello</p>\r\n", output.Output);
        }

        [TestMethod]
        public void GenerateFile_JsonFile_ProcessesJson()
        {
            var employees = new[] { "Sarah", "Mike", "Joe", "Zippy the clown" }
                .Select(employee => new Dictionary<string, object>
                {
                    { "name", employee }
                }.ToImmutableDictionary())
                .Memoize();
            var output = generator.GenerateFile(".",
                new FileLocation(@"directory\profiles.json"),
                @"{ 'profiles': [ '{{ employees | map: 'name' | join: ""', '"" }}' ] }",
                Array.Empty<string>(),
                new Dictionary<string, IBuffer<ImmutableDictionary<string, object>>>
                {
                    { "employees", employees }
                }.ToImmutableDictionary()
            );

            Assert.AreEqual(@"directory\profiles.json", output.Name.FullyQualifiedName);
            Assert.AreEqual("{ 'profiles': [ 'Sarah', 'Mike', 'Joe', 'Zippy the clown' ] }", output.Output);
        }
    }
}
