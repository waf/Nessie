using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services;
using Nessie.Services.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nessie.Tests.Integration
{
    [TestClass]
    public class FileGeneratorTests
    {
        private FileGenerator generator;

        [TestInitialize]
        public void Initialize()
        {
            generator = new FileGenerator(new TemplateProcessor(), new MarkdownProcessor());
        }

        [TestMethod]
        public void GenerateFile_Markdown_OutputsHtml()
        {
            var output = generator.GenerateFile(".",
                new FileLocation("directory/index.md"),
                "Hello",
                Array.Empty<string>(),
                new Dictionary<string, IBuffer<Hash>>()
            );

            Assert.AreEqual(@"directory\index.html", output.Name.FullyQualifiedName);
            Assert.AreEqual("<p>Hello</p>\r\n", output.Output);
        }

        [TestMethod]
        public void GenerateFile_JsonFile_ProcessesJson()
        {
            var employees = new[] { "Sarah", "Mike", "Joe", "Zippy the clown" }
                .Select(employee => Hash.FromAnonymousObject(new
                {
                    name = employee
                }))
                .Memoize();
            var output = generator.GenerateFile(".",
                new FileLocation(@"directory\profiles.json"),
                @"{ 'profiles': [ '{{ employees | map: 'name' | join: ""', '"" }}' ] }",
                Array.Empty<string>(),
                new Dictionary<string, IBuffer<Hash>>
                {
                    { "employees", employees }
                }
            );

            Assert.AreEqual(@"directory\profiles.json", output.Name.FullyQualifiedName);
            Assert.AreEqual("{ 'profiles': [ 'Sarah', 'Mike', 'Joe', 'Zippy the clown' ] }", output.Output);
        }
    }
}
