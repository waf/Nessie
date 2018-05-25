using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessie.Services.Processors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nessie.Tests.Converters
{
    /// <summary>
    /// Sanity checks to make sure DotLiquid is working. We don't need to do
    /// much testing of the underlying library.
    /// </summary>
    [TestClass]
    public class TemplateProcessorTests
    {
        [TestMethod]
        public void Convert_WithTemplate_RendersString()
        {
            var converter = new TemplateProcessor();

            const string template = "{% if array contains 'yes' %}yep{% endif %}";
            var input = new Dictionary<string, object>
            {
                { "array", new[] { "yes", "no", "maybe", "so"} }
            };

            string result = converter.Convert(".", template, Hash.FromDictionary(input), out Hash output);

            Assert.AreEqual("yep", result);
        }

        [TestMethod]
        public void Convert_WithTemplateOutput_CapturesOutput()
        {
            var converter = new TemplateProcessor();

            const string template = "{% assign name = 'Fido'  %}{% capture description %} A good boy! {% endcapture %}";

            converter.Convert(".", template, new Hash(), out Hash output);

            Assert.AreEqual("Fido", output["name"]);
            Assert.AreEqual(" A good boy! ", output["description"]);
        }
    }
}
