using Microsoft.VisualStudio.TestTools.UnitTesting;
using HiddenGems.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiddenGemsTests;
using System.IO;

namespace HiddenGems.Common.Tests
{
    [TestClass()]
    public class JobControllerHelpersTests
    {
        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            var configuration = TestHelpers.InitConfiguration();
            SharedSettings.PopulateSharedSettings(configuration);
        }

        [TestMethod()]
        public void UploadDataTest_NullRequest()
        {
            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt.csv", FileMode.Open, FileAccess.Read);
            var incomingFileName = sampleDataStream.Name;
            var result = JobControllerHelpers.UploadData(sampleDataStream, incomingFileName, null);
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void UploadDataTest_EmptyData()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            var result = JobControllerHelpers.UploadData(new MemoryStream(Array.Empty<byte>()), "Test", CurrentJobRequest);
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void UploadDataTest_NullData()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            var result = JobControllerHelpers.UploadData(null, "Test", CurrentJobRequest);
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void UploadDataTest_GoodData()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt.csv", FileMode.Open, FileAccess.Read);
            var incomingFileName = sampleDataStream.Name;
            var result = JobControllerHelpers.UploadData(sampleDataStream, incomingFileName, CurrentJobRequest);
            Assert.IsTrue(result
                && CurrentJobRequest.ReportIdentifier == incomingFileName
                && CurrentJobRequest.Data.DataToProcess != null
                && CurrentJobRequest.Data.OriginalColumnNames.Count == 5
                && CurrentJobRequest.Data.EligibleColumns.Count == 5);
        }

        [TestMethod()]
        public void UploadDataTest_NoHeaders()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());

            var testData =
@"1,1,1500,5,5
0,100,,5,5
1,2,1499,,4
0,99,1499,4,
1,3,1498,3,3
0,98,1498,3,3
1,4,1497,2,2
0,97,1497,2,2
1,1,1500,5,5
0,100,1500,5,5";

            byte[] testBytes = Encoding.ASCII.GetBytes(testData);
            MemoryStream testStream = new(testBytes);
            var incomingFileName = "Test";
            var result = JobControllerHelpers.UploadData(testStream, incomingFileName, CurrentJobRequest);
            Assert.IsTrue(result
                && CurrentJobRequest.ReportIdentifier == incomingFileName
                && CurrentJobRequest.Data.DataToProcess != null
                && CurrentJobRequest.Data.OriginalColumnNames.Count == 5
                && CurrentJobRequest.Data.EligibleColumns.Count == 5);
        }

        [TestMethod()]
        public void UploadDataTest_MissingColumnName()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());

            var testData =
@"ShouldIBuyTheCar,,TotalPreviousRepairs,KellyBlueBookRating,CrashRating
1,,1500,5,5
0,100,,5,5
1,2,1499,,4
0,99,1499,4,
1,3,1498,3,3
0,98,1498,3,3
1,4,1497,2,2
0,97,1497,2,2
1,1,1500,5,5
0,100,1500,5,5";

            byte[] testBytes = Encoding.ASCII.GetBytes(testData);
            MemoryStream testStream = new(testBytes);
            var incomingFileName = "Test";
            var result = JobControllerHelpers.UploadData(testStream, incomingFileName, CurrentJobRequest);
            Assert.IsTrue(result
                && CurrentJobRequest.ReportIdentifier == incomingFileName
                && CurrentJobRequest.Data.DataToProcess != null
                && CurrentJobRequest.Data.OriginalColumnNames.Count == 5
                && CurrentJobRequest.Data.EligibleColumns.Count == 5);
        }

        [TestMethod()]
        public void UploadDataTest_EmptyLineBeforeHeader()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());

            var testData =
@"
ShouldIBuyTheCar,CarAge,TotalPreviousRepairs,KellyBlueBookRating,CrashRating
1,,1500,5,5
0,100,,5,5
1,2,1499,,4
0,99,1499,4,
1,3,1498,3,3
0,98,1498,3,3
1,4,1497,2,2
0,97,1497,2,2
1,1,1500,5,5
0,100,1500,5,5";

            byte[] testBytes = Encoding.ASCII.GetBytes(testData);
            MemoryStream testStream = new(testBytes);
            var incomingFileName = "Test";
            var result = JobControllerHelpers.UploadData(testStream, incomingFileName, CurrentJobRequest);
            Assert.IsTrue(result
                && CurrentJobRequest.ReportIdentifier == incomingFileName
                && CurrentJobRequest.Data.DataToProcess != null
                && CurrentJobRequest.Data.OriginalColumnNames.Count == 5
                && CurrentJobRequest.Data.EligibleColumns.Count == 5);
        }

        [TestMethod()]
        public void UploadDataTest_EmptyLineInMiddleOfData()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());

            var testData =
@"ShouldIBuyTheCar,CarAge,TotalPreviousRepairs,KellyBlueBookRating,CrashRating
1,,1500,5,5
0,100,,5,5
1,2,1499,,4
0,99,1499,4,

1,3,1498,3,3
0,98,1498,3,3
1,4,1497,2,2
0,97,1497,2,2
1,1,1500,5,5
0,100,1500,5,5";

            byte[] testBytes = Encoding.ASCII.GetBytes(testData);
            MemoryStream testStream = new(testBytes);
            var incomingFileName = "Test";
            var result = JobControllerHelpers.UploadData(testStream, incomingFileName, CurrentJobRequest);
            Assert.IsTrue(result
                && CurrentJobRequest.ReportIdentifier == incomingFileName
                && CurrentJobRequest.Data.DataToProcess != null
                && CurrentJobRequest.Data.OriginalColumnNames.Count == 5
                && CurrentJobRequest.Data.EligibleColumns.Count == 5);
        }

        [TestMethod()]
        public void UploadDataTest_FirstLineEmptyNoHeader()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());

            var testData =
@"
1,,1500,5,5
0,100,,5,5
1,2,1499,,4
0,99,1499,4,
1,3,1498,3,3
0,98,1498,3,3
1,4,1497,2,2
0,97,1497,2,2
1,1,1500,5,5
0,100,1500,5,5";

            byte[] testBytes = Encoding.ASCII.GetBytes(testData);
            MemoryStream testStream = new(testBytes);
            var incomingFileName = "Test";
            var dataLength = testBytes.Length;
            var result = JobControllerHelpers.UploadData(testStream, incomingFileName, CurrentJobRequest);
            Assert.IsTrue(result
                && CurrentJobRequest.ReportIdentifier == incomingFileName
                && CurrentJobRequest.Data.DataToProcess != null
                && CurrentJobRequest.Data.OriginalColumnNames.Count == 5
                && CurrentJobRequest.Data.EligibleColumns.Count == 5);
        }

        [TestMethod()]
        public void SubmitRequestTest_NullRequest()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt.csv", FileMode.Open, FileAccess.Read);
            var incomingFileName = sampleDataStream.Name;
            JobControllerHelpers.UploadData(sampleDataStream, incomingFileName, CurrentJobRequest);
            var result = JobControllerHelpers.SubmitRequest(null, new List<string>() { CurrentJobRequest.Data.EligibleColumns[0].Item1 });
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void SubmitRequestTest_GoodRequest()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());

            CurrentJobRequest.Results.Add(new JobResult(
                CurrentJobRequest.JobKey,
                "Column1",
                AlgorithmType.BinaryClassification,
                15,
                new List<string> { "Column1", "Column2", "Column3" },
                "",
                new byte[1],
                new byte[1],
                "Metrics",
                new float[] { .07f, .01f, .05f }
                ));
            CurrentJobRequest.Status.ParentJobKey = null;

            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt.csv", FileMode.Open, FileAccess.Read);
            var incomingFileName = sampleDataStream.Name;
            JobControllerHelpers.UploadData(sampleDataStream, incomingFileName, CurrentJobRequest);
            var result = JobControllerHelpers.SubmitRequest(CurrentJobRequest, new List<string>() { CurrentJobRequest.Data.EligibleColumns[0].Item1 });
            Assert.IsTrue(result
                && CurrentJobRequest.Status.ParentJobKey == CurrentJobRequest.JobKey
                && CurrentJobRequest.Results.Count == 0
                && CurrentJobRequest.Data.SelectedColumnToAnalyze.Item1 == "Column1"
                && CurrentJobRequest.Status.EstimatedDuration == SharedSettings.MinimumEstimatedDuration);
        }

        [TestMethod()]
        public void SubmitRequestTest_ExceptionFromNullData()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());

            CurrentJobRequest.Results.Add(new JobResult(
                CurrentJobRequest.JobKey,
                "Column1",
                AlgorithmType.BinaryClassification,
                15,
                new List<string> { "Column1", "Column2", "Column3" },
                "",
                new byte[1],
                new byte[1],
                "Metrics",
                new float[] { .07f, .01f, .05f }
                ));
            CurrentJobRequest.Status.ParentJobKey = null;

            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt.csv", FileMode.Open, FileAccess.Read);
            var incomingFileName = sampleDataStream.Name;
            JobControllerHelpers.UploadData(sampleDataStream, incomingFileName, CurrentJobRequest);

            CurrentJobRequest.Data = null;

            var result = JobControllerHelpers.SubmitRequest(CurrentJobRequest, new List<string>() { "Column1" });
            Assert.IsTrue(!result
                && CurrentJobRequest.Status.Error == ErrorCode.GenericError
                && CurrentJobRequest.Status.StatusMessage == JobStatus.GetFriendlyErrorMessage("", CurrentJobRequest.Status)
                && CurrentJobRequest.Status.CanRemove);
        }

        [TestMethod()]
        public void CheckStatusTest_JobDoesNotExist()
        {
            JobRequestManager.ResetQueue();
            var returnedStatus = JobControllerHelpers.CheckStatus("asdf", new MockLogger());
            Assert.IsTrue(returnedStatus.StatusMessage == "Waiting for status...");
        }

        [TestMethod()]
        public void CheckStatusTest_GoodRequest()
        {
            JobRequestManager.ResetQueue();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            var returnedStatus = JobControllerHelpers.CheckStatus(CurrentJobRequest.JobKey, new MockLogger());
            Assert.IsTrue(returnedStatus == CurrentJobRequest.Status);
        }

        [TestMethod()]
        public void CancelJobTest_NullRequest()
        {
            JobRequestManager.ResetQueue();
            var result = JobControllerHelpers.CancelJob("asdf", new MockLogger());
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void CancelJobTest_GoodRequest()
        {
            JobRequestManager.ResetQueue();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            var result = JobControllerHelpers.CancelJob(CurrentJobRequest.JobKey, new MockLogger());
            var getRequest = JobRequestManager.GetExistingJobRequest(CurrentJobRequest.JobKey);
            Assert.IsTrue(result
                && getRequest == null);
        }

        [TestMethod()]
        public void GetResultTest_HasResults()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());

            CurrentJobRequest.Results.Add(new JobResult(
                CurrentJobRequest.JobKey,
                "Column1",
                AlgorithmType.BinaryClassification,
                15,
                new List<string> { "Column1", "Column2", "Column3" },
                "",
                new byte[1],
                new byte[1],
                "{\"Metrics\":\"1\"}",
                new float[] { .07f, .01f, .05f }
                ));

            var result = JobControllerHelpers.GetResult(CurrentJobRequest.JobKey, new MockLogger());
            Assert.IsTrue(result.ColumnWeights.Length == 2 && !string.IsNullOrWhiteSpace(result.RawResult));
        }

        [TestMethod()]
        public void GetResultTest_NoResults()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());

            var result = JobControllerHelpers.GetResult(CurrentJobRequest.JobKey, new MockLogger());

            Assert.IsTrue(result.ColumnWeights.Length == 0 && string.IsNullOrWhiteSpace(result.RawResult));
        }
    }
}