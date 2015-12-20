using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie;

namespace Nessie.Tests
{
    [TestClass]
    public class MarkdownTests
    {
        [TestMethod]
        public void Markdown()
        {
            var markdown = new MarkdownConverter();
            var result = markdown.Convert("- I'm a list item.\n- I'm another one.");
            Assert.AreEqual(
                "<ul>\n<li>I'm a list item.</li>\n<li>I'm another one.</li>\n</ul>\n",
                result);
        }
    }
}
