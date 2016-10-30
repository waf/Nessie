using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services;
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
        private ProjectGenerator generator;
        private IDictionary<string, string> filesToRead = null;
        private readonly IDictionary<string, string> writtenFiles = new Dictionary<string, string>();

        [TestInitialize]
        public void Setup()
        {
            this.generator = new ProjectGenerator(
                readFile: name => filesToRead[name],
                writeFile: (name, contents) => writtenFiles[name] = contents);
        }

        [TestMethod]
        public void Generator_WithMultipleTemplate_AppliesAllTemplates()
        {
            filesToRead = new Dictionary<string, string>()
            {
                {"_template_.html", "t1 {{body}} t1" },
                {"index.md", "I'm the *root* index file" },
                {"blog/_template_.html", "{% capture body %}t2 {{ body }} t2{% endcapture %}" },
                {"blog/index.md", "{% capture body %}my posts:\r\n{% for item in post %}\r\n- {{ item.title }}{% endfor %}\r\n{% endcapture %}" },
                {"blog/_template_post.html", "{% capture body %}t3 {{ body }} t3{% endcapture %}" },
                {"blog/_post_first.md", "{% assign title = 'Title 1' %}{% capture body %}\r\n content one \r\n{% endcapture %}" },
                {"blog/_post_second.md", "{% assign nessie-url-prefix = 'foo\\bar' %}{% assign title = 'Title 2' %}{% capture body %} content two {% endcapture %}" },
            };

            generator.Generate("", filesToRead.Select(kvp => kvp.Key).ToList(), "_output");

            Assert.AreEqual("<p>I'm the <em>root</em> index file</p>\r\n", writtenFiles["_output\\index.html"]);
            Assert.AreEqual("t1 t2 <p>my posts:</p>\r\n<ul>\r\n<li>Title 1</li>\r\n<li>Title 2</li>\r\n</ul> t2 t1", writtenFiles["_output\\blog\\index.html"]);
            Assert.AreEqual("t1 t2 t3 <p>content one</p> t3 t2 t1", writtenFiles["_output\\blog\\first.html"]);
            Assert.AreEqual("t1 t2 t3 content two t3 t2 t1", writtenFiles["_output\\foo\\bar\\second.html"]);
            Assert.AreEqual(4, writtenFiles.Count());
        }

        [TestMethod]
        public void Generator_ItemsHaveNoTemplate_ItemsNotRendered()
        {
            filesToRead = new Dictionary<string, string>()
            {
                {"_template_.html", "t1 {{body}} t1" },
                {"index.md", "I'm the *root* index file" },
                {"blog/_template_.html", "{% capture body %}t2 {{ body }} t2{% endcapture %}" },
                {"blog/index.md", "{% capture body %}my posts:\r\n{% for item in post %}\r\n- {{ item.title }}{% endfor %}\r\n{% endcapture %}" },
                {"blog/_post_first.md", "{% assign title = 'Title 1' %}{% capture body %}\r\n content one \r\n{% endcapture %}" },
                {"blog/_post_second.md", "{% assign nessie-url-prefix = 'foo\\bar' %}{% assign title = 'Title 2' %}{% capture body %} content two {% endcapture %}" },
            };

            generator.Generate("", filesToRead.Select(kvp => kvp.Key).ToList(), "_output");

            Assert.AreEqual("<p>I'm the <em>root</em> index file</p>\r\n", writtenFiles["_output\\index.html"]);
            Assert.AreEqual("t1 t2 <p>my posts:</p>\r\n<ul>\r\n<li>Title 1</li>\r\n<li>Title 2</li>\r\n</ul> t2 t1", writtenFiles["_output\\blog\\index.html"]);
            Assert.AreEqual(2, writtenFiles.Count());
        }
    }
}
