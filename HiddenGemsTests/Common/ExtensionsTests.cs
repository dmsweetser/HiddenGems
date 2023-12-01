using Microsoft.VisualStudio.TestTools.UnitTesting;
using HiddenGems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiddenGemsTests;

namespace HiddenGems.Common.Tests
{
    [TestClass()]
    public class ExtensionsTests
    {
        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            var configuration = TestHelpers.InitConfiguration();
            SharedSettings.PopulateSharedSettings(configuration);
        }

        [TestMethod()]
        public void GetDescriptionTest_AlgorithmType()
        {
            var description = AlgorithmType.BinaryClassification.GetDescription();
            Assert.IsTrue(description == "Is it or isn't it? [Binary Classification]");
        }

        [TestMethod()]
        public void GetDescriptionTest_ErrorCode()
        {
            var description = ErrorCode.GenericError.GetDescription();
            Assert.IsTrue(description == "Something went wrong!");
        }
    }
}