using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services;
using Nessie.Services.Processors;
using Nessie.Tests.Integration;
using System.Collections.Generic;
using System.Linq;

namespace Nessie.Tests
{
    [TestClass]
    public class GeneratorTests
    {
        private FakeFileSystem fs;
        private ProjectGenerator generator;

        [TestInitialize]
        public void Setup()
        {
            fs = new FakeFileSystem();

            this.generator = new ProjectGenerator(
                fileio: fs.FileOperation,
                fileGenerator: new FileGenerator(new TemplateProcessor(fs.FileOperation), new MarkdownProcessor()),
                templateService: new TemplateService());
        }

        [TestMethod]
        public void Generator_WithMultipleTemplate_AppliesAllTemplates()
        {
            fs.InputFiles = new Dictionary<string, string>()
            {
                {"_template_.html", "t1 {{body}} t1" },
                {"index.md", "I'm the *root* index file" },
                {"blog/_template_.html", "{% capture body %}t2 {{ body }} t2{% endcapture %}" },
                {"blog/index.md", "{% capture body %}my posts:\r\n{% for item in post %}\r\n- {{ item.title }}{% endfor %}\r\n{% endcapture %}" },
                {"blog/_template_post.html", "{% capture body %}t3 {{ body }} t3{% endcapture %}" },
                {"blog/_post_first.md", "{% assign title = 'Title 1' %}{% capture body %}\r\n content one \r\n{% endcapture %}" },
                {"blog/_post_second.md", "{% assign nessie-url-prefix = 'foo/bar' %}{% assign title = 'Title 2' %}{% capture body %} content two {% endcapture %}" },
            };

            generator.Generate(FakeFileSystem.Root, "_output", fs.InputFiles.Select(kvp => kvp.Key).ToList());

            AreEqualIgnoringNewLines("<p>I’m the <em>root</em> index file</p>\r\n", fs.OutputFiles["_output/index.html"]);
            AreEqualIgnoringNewLines("t1 t2<p>my posts:</p>\r\n<ul>\r\n<li>Title 1</li>\r\n<li>Title 2</li>\r\n</ul><p>t2 t1</p>", fs.OutputFiles["_output/blog/index.html"]);
            AreEqualIgnoringNewLines("t1 t2 t3<p>content one</p><p>t3 t2 t1</p>", fs.OutputFiles["_output/blog/first.html"]);
            AreEqualIgnoringNewLines("<p>t1 t2 t3 content two t3 t2 t1</p>", fs.OutputFiles["_output/foo/bar/second.html"]);
            Assert.AreEqual(4, fs.OutputFiles.Count);
        }

        private static void AreEqualIgnoringNewLines(string expected, string actual)
        {
            expected = expected.Replace("\r", "").Replace("\n", "");
            actual = actual.Replace("\r", "").Replace("\n", "");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Generator_ItemsHaveNoTemplate_ItemsNotRendered()
        {
            fs.InputFiles = new Dictionary<string, string>()
            {
                {"_template_.html", "t1 {{body}} t1" },
                {"index.md", "I'm the *root* index file" },
                {"blog/_template_.html", "{% capture body %}t2 {{ body }} t2{% endcapture %}" },
                {"blog/index.md", "{% capture body %}my posts:\r\n{% for item in post %}\r\n- {{ item.title }}{% endfor %}\r\n{% endcapture %}" },
                {"blog/_post_first.md", "{% assign title = 'Title 1' %}{% capture body %}\r\n content one \r\n{% endcapture %}" },
                {"blog/_post_second.md", "{% assign title = 'Title 2' %}{% capture body %} content two {% endcapture %}" },
            };

            generator.Generate(FakeFileSystem.Root, "_output", fs.InputFiles.Select(kvp => kvp.Key).ToList());

            AreEqualIgnoringNewLines("<p>I’m the <em>root</em> index file</p>\r\n", fs.OutputFiles["_output/index.html"]);
            AreEqualIgnoringNewLines("t1 t2<p>my posts:</p>\r\n<ul>\r\n<li>Title 1</li>\r\n<li>Title 2</li>\r\n</ul><p>t2 t1</p>", fs.OutputFiles["_output/blog/index.html"]);
            Assert.AreEqual(2, fs.OutputFiles.Count);
        }
    }
}
