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
    public class DataRecordBuilderTests
    {
        public class DataRecordBuilderTestRecord
        {
            public string Column1 { get; set; }
            public string Column2 { get; set; }
        }

        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            var configuration = TestHelpers.InitConfiguration();
            SharedSettings.PopulateSharedSettings(configuration);
        }

        [TestMethod()]
        public void BuildDataRecordTest_OneColumn()
        {
            var builtType = DataRecordBuilder.BuildDataRecord(1);
            Assert.IsTrue(builtType.GetProperties().Where(x => x.Name.StartsWith("Column")).ToList().Count == 1);
        }

        [TestMethod()]
        public void BuildDataRecordTest_FiveColumns()
        {
            var builtType = DataRecordBuilder.BuildDataRecord(5);
            Assert.IsTrue(builtType.GetProperties().Where(x => x.Name.StartsWith("Column")).ToList().Count == 5);
        }

        [TestMethod()]
        public void ScrubValuesTest_FillerValue()
        {
            var test = new DataRecordBuilderTestRecord
            {
                Column1 = "",
                Column2 = "test"
            };
            var scrubbedTest = DataRecordBuilder.ScrubValues(test);
            Assert.IsTrue(scrubbedTest.Column1 == DataRecordBuilder.FillerValue);
        }

        [TestMethod()]
        public void ScrubValuesTest_FillerValue_FromSharedSettings()
        {
            var test = new DataRecordBuilderTestRecord
            {
                Column1 = SharedSettings.FillerValues[0],
                Column2 = SharedSettings.FillerValues[1]
            };
            var scrubbedTest = DataRecordBuilder.ScrubValues(test);
            Assert.IsTrue(
                scrubbedTest.Column1 == DataRecordBuilder.FillerValue
                && scrubbedTest.Column2 == DataRecordBuilder.FillerValue);
        }

        [TestMethod()]
        public void ScrubValuesTest_YesNoValues()
        {
            var test = new DataRecordBuilderTestRecord
            {
                Column1 = "Yes",
                Column2 = "No"
            };
            var scrubbedTest = DataRecordBuilder.ScrubValues(test);
            Assert.IsTrue(
                scrubbedTest.Column1 == "True"
                && scrubbedTest.Column2 == "False");
        }

        [TestMethod()]
        public void ScrubValuesTest_YesNoValuesLowercase()
        {
            var test = new DataRecordBuilderTestRecord
            {
                Column1 = "yes",
                Column2 = "no"
            };
            var scrubbedTest = DataRecordBuilder.ScrubValues(test);
            Assert.IsTrue(
                scrubbedTest.Column1 == "True"
                && scrubbedTest.Column2 == "False");
        }

        [TestMethod()]
        public void ScrubValuesTest_YesNoValuesUppercase()
        {
            var test = new DataRecordBuilderTestRecord
            {
                Column1 = "YES",
                Column2 = "NO"
            };
            var scrubbedTest = DataRecordBuilder.ScrubValues(test);
            Assert.IsTrue(
                scrubbedTest.Column1 == "True"
                && scrubbedTest.Column2 == "False");
        }

        [TestMethod()]
        public void ScrubValuesTest_TrueFalseValues()
        {
            var test = new DataRecordBuilderTestRecord
            {
                Column1 = "True",
                Column2 = "False"
            };
            var scrubbedTest = DataRecordBuilder.ScrubValues(test);
            Assert.IsTrue(
                scrubbedTest.Column1 == "True"
                && scrubbedTest.Column2 == "False");
        }

        [TestMethod()]
        public void ScrubValuesTest_TrueFalseValuesLowercase()
        {
            var test = new DataRecordBuilderTestRecord
            {
                Column1 = "true",
                Column2 = "false"
            };
            var scrubbedTest = DataRecordBuilder.ScrubValues(test);
            Assert.IsTrue(
                scrubbedTest.Column1 == "True"
                && scrubbedTest.Column2 == "False");
        }

        [TestMethod()]
        public void ScrubValuesTest_TrueFalseValuesUppercase()
        {
            var test = new DataRecordBuilderTestRecord
            {
                Column1 = "TRUE",
                Column2 = "FALSE"
            };
            var scrubbedTest = DataRecordBuilder.ScrubValues(test);
            Assert.IsTrue(
                scrubbedTest.Column1 == "True"
                && scrubbedTest.Column2 == "False");
        }
    }
}