using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nessie.Tests
{
    [TestClass]
    public class TemplateServiceTests
    {
        private TemplateService service;

        [TestInitialize]
        public void Initialize()
        {
            this.service = new TemplateService();
        }

        [TestMethod]
        public void GetApplicableTemplates_NestedTemplatesForCategory_AppliesInOrder()
        {
            var templates = this.service.GetApplicableTemplates(
                new[]
                {
                    new FileLocation("_template_.html"),
                    new FileLocation("albums/_template_.html"),
                    new FileLocation("albums/_template_dog.html"),
                    new FileLocation("albums/_template_cat.html"),
                    new FileLocation("albums/dogs/_template_dog.html"),
                },
                new FileLocation("albums/dogs/_dog_fido.md")
            );

            Assert.IsTrue(
                new[]
                {
                    new FileLocation("albums/dogs/_template_dog.html"),
                    new FileLocation("albums/_template_dog.html"),
                    new FileLocation("albums/_template_.html"),
                    new FileLocation("_template_.html"),
                }
                .SequenceEqual(templates));
        }

        [TestMethod]
        public void GetApplicableTemplates_NestedTemplatesWithoutCategory_AppliesInOrder()
        {
            var templates = this.service.GetApplicableTemplates(
                new[]
                {
                    new FileLocation("_template_.html"),
                    new FileLocation("albums/_template_.html"),
                    new FileLocation("albums/_template_dog.html"),
                    new FileLocation("albums/_template_cat.html"),
                    new FileLocation("albums/dogs/_template_dog.html"),
                },
                new FileLocation("albums/dog.md")
            );

            Assert.IsTrue(
                new[]
                {
                    new FileLocation("albums/_template_.html"),
                    new FileLocation("_template_.html"),
                }
                .SequenceEqual(templates));
        }

        [TestMethod]
        public void GetApplicableTemplates_NotMarkdown_DoesNotApplyTemplates()
        {
            var templates = this.service.GetApplicableTemplates(
                new[]
                {
                    new FileLocation("_template_.html"),
                    new FileLocation("albums/_template_.html"),
                    new FileLocation("albums/_template_dog.html"),
                    new FileLocation("albums/_template_cat.html"),
                    new FileLocation("albums/dogs/_template_dog.html"),
                },
                new FileLocation("albums/dogs/_dog_fido.png")
            );

            Assert.AreEqual(0, templates.Count);
        }
    }
}
