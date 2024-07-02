using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text.Tagging;
using PowerShellTools.Classification;

namespace PowerShellTools.Test
{
    [TestClass]
    public class RegionAndBraceMatchingServiceTest
    {
        private RegionAndBraceMatchingService _service;
        private Dictionary<int, int> _startBraces;
        private Dictionary<int, int> _endBraces;
        private List<TagInformation<IOutliningRegionTag>> _regionTags;

        [TestInitialize]
        public void Init()
        {
            _service = new RegionAndBraceMatchingService();
        }

        [TestMethod]
        public void ShouldMatchMultiLineComment()
        {
            var script = "<#\r\nComment\r\n#>";

            Token[] tokens;
            ParseError[] errors;
            Parser.ParseInput(script, out tokens, out errors);

            _service.GetRegionsAndBraceMatchingInformation(script, 0, tokens, out _startBraces, out _endBraces, out _regionTags);

            Assert.AreEqual(script.IndexOf("<#") + 2, _regionTags.FirstOrDefault().Start);
            Assert.AreEqual(script.IndexOf("#>") - 2, _regionTags.FirstOrDefault().Length);
            Assert.AreEqual("...", _regionTags.FirstOrDefault().Tag.CollapsedForm);
        }

        [TestMethod]
        public void ShouldMatchRegion()
        {
            var script = "#region MyRegion\r\n#endregion";

            Token[] tokens;
            ParseError[] errors;
            Parser.ParseInput(script, out tokens, out errors);

            _service.GetRegionsAndBraceMatchingInformation(script, 0, tokens, out _startBraces, out _endBraces, out _regionTags);

            Assert.AreEqual(script.IndexOf("#region"), _regionTags.FirstOrDefault().Start);
            Assert.AreEqual(script.IndexOf("#endregion") + "#endregion".Length, _regionTags.FirstOrDefault().Length);
            Assert.AreEqual("#region MyRegion...", _regionTags.FirstOrDefault().Tag.CollapsedForm);
        }

        [TestMethod]
        public void ShouldMatchCurlyBraces()
        {
            var script = "{\r\n}";

            Token[] tokens;
            ParseError[] errors;
            Parser.ParseInput(script, out tokens, out errors);

            _service.GetRegionsAndBraceMatchingInformation(script, 0, tokens, out _startBraces, out _endBraces, out _regionTags);

            Assert.AreEqual(script.IndexOf("{") + 1, _regionTags.FirstOrDefault().Start);
            Assert.AreEqual(script.IndexOf("}") - 1, _regionTags.FirstOrDefault().Length);
            Assert.AreEqual("...", _regionTags.FirstOrDefault().Tag.CollapsedForm);
        }

        [TestMethod]
        public void ShouldMatchSquareBraces()
        {
            var script = "[\r\n]";

            Token[] tokens;
            ParseError[] errors;
            Parser.ParseInput(script, out tokens, out errors);

            _service.GetRegionsAndBraceMatchingInformation(script, 0, tokens, out _startBraces, out _endBraces, out _regionTags);

            Assert.AreEqual(script.IndexOf("[") + 1, _regionTags.FirstOrDefault().Start);
            Assert.AreEqual(script.IndexOf("]") - 1, _regionTags.FirstOrDefault().Length);
            Assert.AreEqual("...", _regionTags.FirstOrDefault().Tag.CollapsedForm);
        }

        [TestMethod]
        public void ShouldMatchParenthesis()
        {
            var script = "(\r\n)";

            Token[] tokens;
            ParseError[] errors;
            Parser.ParseInput(script, out tokens, out errors);

            _service.GetRegionsAndBraceMatchingInformation(script, 0, tokens, out _startBraces, out _endBraces, out _regionTags);

            Assert.AreEqual(script.IndexOf("(") + 1, _regionTags.FirstOrDefault().Start);
            Assert.AreEqual(script.IndexOf(")") - 1, _regionTags.FirstOrDefault().Length);
            Assert.AreEqual("...", _regionTags.FirstOrDefault().Tag.CollapsedForm);
        }

        [TestMethod]
        public void ShouldCollapseFunctionCorrectly()
        {
            var script = "function Test \r\n{\t\r\nGet-Process\r\n}";

            Token[] tokens;
            ParseError[] errors;
            Parser.ParseInput(script, out tokens, out errors);

            _service.GetRegionsAndBraceMatchingInformation(script, 0, tokens, out _startBraces, out _endBraces, out _regionTags);

            Assert.AreEqual(script.IndexOf("{") + 1, _regionTags.FirstOrDefault().Start);
            Assert.AreEqual(16, _regionTags.FirstOrDefault().Length);
            Assert.AreEqual("...", _regionTags.FirstOrDefault().Tag.CollapsedForm);
        }
    }
}
