using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services;
using Nessie.Services.Processors;

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
        public void Convert_BulletedList_OutputsCorrectHtml()
        {
            var markdown = new MarkdownProcessor();
            var result = markdown.Convert("- I'm a list item.\n- I'm another one.");
            Assert.AreEqual(
                "<ul>\r\n<li>I'm a list item.</li>\r\n<li>I'm another one.</li>\r\n</ul>\r\n",
                result);
        }
    }
}
