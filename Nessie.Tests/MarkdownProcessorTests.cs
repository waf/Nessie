using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services;
using Nessie.Services.Processors;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Nessie.Tests
{
    [TestClass]
    public class MarkdownProcessorTests
    {
        [TestMethod]
        public void Convert_GivenMarkdown_OutputsHtml()
        {
            var processor = new MarkdownProcessor("pandoc");

            var result = processor.Convert("# Hello World", Settings.Default);

            Assert.AreEqual("<h1 id=\"hello-world\">Hello World</h1>\r\n", result);
        }

        [TestMethod]
        public void Convert_ExecutableNotFound_ThrowsNiceError()
        {
            string executable = Guid.NewGuid().ToString();
            var processor = new MarkdownProcessor(executable);

            var ex = Assert.ThrowsException<ErrorMessageException>(() =>
                processor.Convert("# Hello World", Settings.Default)
            );

            StringAssert.Contains(ex.Message, $"Could not find {executable} on the path. Is it installed?");
        }
    }
}
