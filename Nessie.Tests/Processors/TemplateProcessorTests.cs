using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services.Processors;
using Nessie.Tests.Integration;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nessie.Tests.Converters
{
    /// <summary>
    /// Sanity checks to make sure DotLiquid is working. We don't need to do
    /// much testing of the underlying library.
    /// </summary>
    [TestClass]
    public class TemplateProcessorTests
    {
        private FakeFileSystem fs;
        private TemplateProcessor converter;

        [TestInitialize]
        public void Initialize()
        {
            fs = new FakeFileSystem();
            converter = new TemplateProcessor(fs.FileOperation);
        }

        [TestMethod]
        public void Convert_WithTemplate_RendersString()
        {
            const string template = "{% if array contains 'yes' %}yep{% endif %}";
            var input = new Dictionary<string, object>
            {
                { "array", new[] { "yes", "no", "maybe", "so"} }
            };

            string result = converter.Convert(FakeFileSystem.Root, template, input.ToImmutableDictionary(), out var output);

            Assert.AreEqual("yep", result);
        }

        [TestMethod]
        public void Convert_WithTemplateOutput_CapturesOutput()
        {
            const string template = "{% assign name = 'Fido'  %}{% capture description %} A good boy! {% endcapture %}";

            converter.Convert(FakeFileSystem.Root, template, ImmutableDictionary.Create<string, object>(), out var output);

            Assert.AreEqual("Fido", output["name"]);
            Assert.AreEqual(" A good boy! ", output["description"]);
        }

        [TestMethod]
        public void Convert_WithPartial_RendersString()
        {
            fs.InputFiles = new Dictionary<string, string>()
            {
                { $@"{FakeFileSystem.Root}\partials\_partial_interrupting_cow.md", "mooooooooooooooo" }
            };

            var result = converter.Convert(
                FakeFileSystem.Root,
                @"A cow {% include 'partials\interrupting_cow.md' %} says:",
                ImmutableDictionary.Create<string, object>());

            Assert.AreEqual("A cow mooooooooooooooo says:", result);
        }
    }
}
