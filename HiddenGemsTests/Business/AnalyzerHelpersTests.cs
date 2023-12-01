using Microsoft.VisualStudio.TestTools.UnitTesting;
using HiddenGems.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HiddenGems.Runtime;
using HiddenGems.Common;
using HiddenGemsTests;

namespace HiddenGems.Business.Tests
{
    [TestClass()]
    public class AnalyzerHelpersTests
    {
        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            var configuration = TestHelpers.InitConfiguration();
            SharedSettings.PopulateSharedSettings(configuration);
        }

        [TestMethod()]
        public async Task RetrieveCsvFromUrlTest_GoodGoogleSheetsUrl()
        {
            Stream resultStream = await AnalyzerHelpers.RetrieveCsvFromUrl("https://docs.google.com/spreadsheets/d/1FxPEkkSnZk-itc27wliHgk1O6hSWqBmfogHi9yWejNQ/edit?usp=sharing");
            var resultText = await new StreamReader(resultStream).ReadToEndAsync();
            Assert.IsTrue(resultText.Length > 0);
        }

        [TestMethod()]
        public async Task RetrieveCsvFromUrlTest_BadGoogleSheetsUrl()
        {
            Stream resultStream = await AnalyzerHelpers.RetrieveCsvFromUrl("https://docs.google.com/ZZZZZ/spreadsheets/d/1FxPEkkSnZk-itc27wliHgk1O6hSWqBmfogHi9yWejNQ/edit?usp=sharingZZZ");
            var resultText = await new StreamReader(resultStream).ReadToEndAsync();
            Assert.IsTrue(string.IsNullOrWhiteSpace(resultText));
        }

        [TestMethod()]
        public async Task RetrieveCsvFromUrlTest_GoodFileUrlCSV()
        {
            Stream resultStream = await AnalyzerHelpers.RetrieveCsvFromUrl("https://fiveloavestwofish.blob.core.windows.net/hiddengems/IsItOrIsntIt.csv");
            var resultText = await new StreamReader(resultStream).ReadToEndAsync();
            Assert.IsTrue(resultText.Length > 0);
        }

        [TestMethod()]
        public async Task RetrieveCsvFromUrlTest_GoodFileUrlTSV()
        {
            Stream resultStream = await AnalyzerHelpers.RetrieveCsvFromUrl("https://fiveloavestwofish.blob.core.windows.net/hiddengems/IsItOrIsntIt_TabDelimited.csv");
            var resultText = await new StreamReader(resultStream).ReadToEndAsync();
            Assert.IsTrue(resultText.Length > 0);
        }

        [TestMethod()]
        public void GetPropertyNamesForDataRecordTest_LabelIsFirstColumn()
        {
            var propertyNames = AnalyzerHelpers.GetPropertyNamesForDataRecord(1, 10);
            Assert.IsTrue(propertyNames[0] == "Column2" && propertyNames.Length == 9);
        }

        [TestMethod()]
        public void GetPropertyNamesForDataRecordTest_NoEligibleColumns()
        {
            var propertyNames = AnalyzerHelpers.GetPropertyNamesForDataRecord(1, 1);
            Assert.IsTrue(propertyNames.Length == 0);
        }

        [TestMethod()]
        public void ProcessDataStreamFromCsvTest_IsItOrIsntIt()
        {
            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt.csv", FileMode.Open, FileAccess.Read);
            var resultData = AnalyzerHelpers.ProcessDataStreamFromCsv(sampleDataStream, out var propertyNames);
            Assert.IsTrue(propertyNames.Count == 5
                && resultData[0] is IHgDataRecord
                && resultData.Count == 202);
        }

        [TestMethod()]
        public void ProcessDataStreamFromCsvTest_QuotedColumnNames()
        {
            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt_QuotedColumnNames.csv", FileMode.Open, FileAccess.Read);
            var resultData = AnalyzerHelpers.ProcessDataStreamFromCsv(sampleDataStream, out var propertyNames);
            Assert.IsTrue(propertyNames.Count == 5
                && resultData[0] is IHgDataRecord
                && resultData.Count == 202);
        }

        [TestMethod()]
        public void ProcessDataStreamFromCsvTest_TabDelimited()
        {
            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt_TabDelimited.csv", FileMode.Open, FileAccess.Read);
            var resultData = AnalyzerHelpers.ProcessDataStreamFromCsv(sampleDataStream, out var propertyNames);
            Assert.IsTrue(propertyNames.Count == 5
                && resultData[0] is IHgDataRecord
                && resultData.Count == 202);
        }

        [TestMethod()]
        public void CategorizeColumnsTest_IsItOrIsntIt()
        {
            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt.csv", FileMode.Open, FileAccess.Read);
            var resultData = AnalyzerHelpers.ProcessDataStreamFromCsv(sampleDataStream, out var propertyNames);
            AnalyzerHelpers.CategorizeColumns(
                resultData,
                propertyNames,
                1,
                new JobRequest(Guid.NewGuid().ToString()),
                out List<Tuple<string, string>> eligibleColumns);
            Assert.IsTrue(eligibleColumns.Count == 4);
        }

        [TestMethod()]
        public void CategorizeColumnsTest_QuotedColumnNames()
        {
            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt_QuotedColumnNames.csv", FileMode.Open, FileAccess.Read);
            var resultData = AnalyzerHelpers.ProcessDataStreamFromCsv(sampleDataStream, out var propertyNames);
            AnalyzerHelpers.CategorizeColumns(
                resultData,
                propertyNames,
                1,
                new JobRequest(Guid.NewGuid().ToString()),
                out List<Tuple<string, string>> eligibleColumns);
            Assert.IsTrue(eligibleColumns.Count == 4);
        }

        [TestMethod()]
        public void CategorizeColumnsTest_IsItOrIsntIt_YesNo()
        {
            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt_YesNo.csv", FileMode.Open, FileAccess.Read);
            var resultData = AnalyzerHelpers.ProcessDataStreamFromCsv(sampleDataStream, out var propertyNames);
            AnalyzerHelpers.CategorizeColumns(
                resultData,
                propertyNames,
                1,
                new JobRequest(Guid.NewGuid().ToString()),
                out List<Tuple<string, string>> eligibleColumns);
            Assert.IsTrue(eligibleColumns.Count == 4);
        }

        [TestMethod()]
        public void CategorizeColumnsTest_UsedCarSample()
        {
            using FileStream sampleDataStream = new("wwwroot\\samples\\UsedCarsSample.csv", FileMode.Open, FileAccess.Read);
            var resultData = AnalyzerHelpers.ProcessDataStreamFromCsv(sampleDataStream, out var propertyNames);
            AnalyzerHelpers.CategorizeColumns(
                resultData,
                propertyNames,
                1,
                new JobRequest(Guid.NewGuid().ToString()),
                out List<Tuple<string, string>> eligibleColumns);
            Assert.IsTrue(eligibleColumns.Count == 6);
        }

        [TestMethod()]
        public void GenerateFilesTest_HasContent()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            CurrentJobRequest.Results.Add(new JobResult(
                CurrentJobRequest.JobKey,
                "Column1",
                AlgorithmType.BinaryClassification,
                15,
                new List<string> { "Column1" },
                "",
                new byte[1],
                new byte[1],
                "Metrics",
                new float[1] { 1.0f }
                ));
            var generatedFiles = AnalyzerHelpers.GenerateFiles(CurrentJobRequest.Results);
            Assert.IsTrue(generatedFiles.Length > 0);
        }

        [TestMethod()]
        public void GenerateFilesTest_NoContent()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            var generatedFiles = AnalyzerHelpers.GenerateFiles(CurrentJobRequest.Results);
            Assert.IsTrue(generatedFiles.Length == 0);
        }
    }
}