using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services;
using Nessie.Services.Processors;
using Nessie.Tests.Utilities;
using System;

namespace Nessie.Tests.Processors
{
    /// <summary>
    /// Sanity checks to make sure markdown is working. We don't need to do
    /// much testing of the underlying markdown library.
    /// </summary>
    [TestClass]
    public class MarkdownProcessorTests
    {
        [TestMethod]
        public void Convert_GivenMarkdown_OutputsHtml()
        {
            var processor = new MarkdownProcessor("pandoc");

            var result = processor.Convert("# Hello World", Settings.Default);

            AssertHelper.AreEqualIgnoringNewLines("<h1 id=\"hello-world\">Hello World</h1>\r\n", result);
        }

        [TestMethod]
        public void Convert_BulletedList_OutputsCorrectHtml()
        {
            var markdown = new MarkdownProcessor();
            var result = markdown.Convert("- I'm a list item.\n- I'm another one.", Settings.Default);
            AssertHelper.AreEqualIgnoringNewLines(
                "<ul>\r\n<li>I’m a list item.</li>\r\n<li>I’m another one.</li>\r\n</ul>\r\n",
                result);
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
