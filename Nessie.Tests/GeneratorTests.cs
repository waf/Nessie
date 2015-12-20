using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nessie.Tests
{
    [TestClass]
    public class GeneratorTests
    {
        private DirectoryGenerator generator;
        private IDictionary<string, string> filesToRead = null;
        private readonly IDictionary<string, string> writtenFiles = new Dictionary<string, string>();

        [TestInitialize]
        public void Setup()
        {
            this.generator = new DirectoryGenerator(
                readFile: name => filesToRead[name],
                writeFile: (name, contents) => writtenFiles[name] = contents);
        }

        [TestMethod]
        public void Generator_WithAutoContent()
        {
            filesToRead = new Dictionary<string, string>()
            {
                {"_auto.html", "prefix {{body}} suffix" },
                {"index.md", "I'm the *root* index file" },
                {"blog/index.md", "{% capture body %}my posts:\r\n{% for item in post %}\r\n- {{ item.title }}{% endfor %}\r\n{% endcapture %}" },
                {"blog/_post_first.md", "{% assign title = 'Title 1' %}{% capture body %} content one {% endcapture %}" },
                {"blog/_post_second.md", "{% assign title = 'Title 2' %}{% capture body %} content two {% endcapture %}" },
            };

            generator.Generate("", filesToRead.Select(kvp => kvp.Key).ToList());

            Assert.AreEqual("<p>I'm the <em>root</em> index file</p>", writtenFiles["_output\\index.html"]);
            Assert.AreEqual("prefix <p>my posts:</p>\n<ul>\n<li>Title 1</li>\n<li>Title 2</li>\n</ul> suffix", writtenFiles["_output\\blog\\index.html"]);
            Assert.AreEqual("prefix <p>content one </p> suffix", writtenFiles["_output\\blog\\_post_first.html"]);
            Assert.AreEqual("prefix <p>content two </p> suffix", writtenFiles["_output\\blog\\_post_second.html"]);
            Assert.AreEqual(4, writtenFiles.Count());
            return;
        }
    }
}
