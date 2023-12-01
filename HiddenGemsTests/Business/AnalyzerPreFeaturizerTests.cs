using Microsoft.VisualStudio.TestTools.UnitTesting;
using HiddenGems.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiddenGems.Common;
using HiddenGemsTests;

namespace HiddenGems.Business.Tests
{
    [TestClass()]
    public class AnalyzerPreFeaturizerTests
    {
        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            var configuration = TestHelpers.InitConfiguration();
            SharedSettings.PopulateSharedSettings(configuration);
        }

        [TestMethod()]
        public void GetPreFeaturizerTest_NoValidData()
        {
            try
            {
                var preFeaturizer = AnalyzerPreFeaturizer.GetPreFeaturizer(
                new Microsoft.ML.MLContext(),
                "Column1",
                new List<string> { "Column0" },
                new List<string> { },
                Microsoft.ML.Data.DataKind.Boolean);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message == "No valid data was found to process");
            }
        }

        [TestMethod()]
        public void GetPreFeaturizerTest_ValidData()
        {
            try
            {
                var preFeaturizer = AnalyzerPreFeaturizer.GetPreFeaturizer(
                new Microsoft.ML.MLContext(),
                "Column1",
                new List<string> { "Column0" },
                new List<string> { "Column1" },
                Microsoft.ML.Data.DataKind.Boolean);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message == "No valid data was found to process");
            }
        }
    }
}