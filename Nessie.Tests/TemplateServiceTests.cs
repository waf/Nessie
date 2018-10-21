using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services;
using System.Linq;

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
                    new FileLocation("C:/_template_.html"),
                    new FileLocation("C:/albums/_template_.html"),
                    new FileLocation("C:/albums/_template_dog.html"),
                    new FileLocation("C:/albums/_template_cat.html"),
                    new FileLocation("C:/albums/dogs/_template_dog.html"),
                },
                new FileLocation("C:/albums/dogs/_dog_fido.md")
            );

            CollectionAssert.AreEqual(
                new[]
                {
                    new FileLocation("C:/albums/dogs/_template_dog.html"),
                    new FileLocation("C:/albums/_template_dog.html"),
                    new FileLocation("C:/albums/_template_.html"),
                    new FileLocation("C:/_template_.html"),
                },
                templates.ToArray());
        }

        [TestMethod]
        public void GetApplicableTemplates_NestedTemplatesWithoutCategory_AppliesInOrder()
        {
            var templates = this.service.GetApplicableTemplates(
                new[]
                {
                    new FileLocation("C:/_template_.html"),
                    new FileLocation("C:/albums/_template_.html"),
                    new FileLocation("C:/albums/_template_dog.html"),
                    new FileLocation("C:/albums/_template_cat.html"),
                    new FileLocation("C:/albums/dogs/_template_dog.html"),
                },
                new FileLocation("C:/albums/dog.md")
            );

            Assert.IsTrue(
                new[]
                {
                    new FileLocation("C:/albums/_template_.html"),
                    new FileLocation("C:/_template_.html"),
                }
                .SequenceEqual(templates));
        }

        [TestMethod]
        public void GetApplicableTemplates_NotMarkdown_DoesNotApplyTemplates()
        {
            var templates = this.service.GetApplicableTemplates(
                new[]
                {
                    new FileLocation("C:/_template_.html"),
                    new FileLocation("C:/albums/_template_.html"),
                    new FileLocation("C:/albums/_template_dog.html"),
                    new FileLocation("C:/albums/_template_cat.html"),
                    new FileLocation("C:/albums/dogs/_template_dog.html"),
                },
                new FileLocation("C:/albums/dogs/_dog_fido.png")
            );

            Assert.AreEqual(0, templates.Count);
        }
    }
}
