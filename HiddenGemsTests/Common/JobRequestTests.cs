using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using HiddenGemsTests;

namespace HiddenGems.Common.Tests
{
    [TestClass()]
    public class JobRequestTests
    {
        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            var configuration = TestHelpers.InitConfiguration();
            SharedSettings.PopulateSharedSettings(configuration);
        }

        [TestMethod()]
        public void JobRequestTest_HasErrorYes()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            CurrentJobRequest.Status.Error = ErrorCode.NoWorkDone;
            Assert.IsTrue(CurrentJobRequest.HasError);
        }

        [TestMethod()]
        public void JobRequestTest_HasErrorNo()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            CurrentJobRequest.Status.Error = ErrorCode.NoError;
            Assert.IsTrue(!CurrentJobRequest.HasError);
        }

        [TestMethod()]
        public void JobRequestTest_ResultExistsYes()
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
            Assert.IsTrue(CurrentJobRequest.ResultExists);
        }

        [TestMethod()]
        public void JobRequestTest_ResultExistsNo()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            Assert.IsTrue(!CurrentJobRequest.ResultExists);
        }


        [TestMethod()]
        public void JobRequestTest_MakeJobDataOnGetter()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            Assert.IsTrue(CurrentJobRequest.Data.ParentJobKey == CurrentJobRequest.JobKey);
        }

        [TestMethod()]
        public void JobRequestTest_MakeJobStatusOnGetter()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            Assert.IsTrue(CurrentJobRequest.Status.ParentJobKey == CurrentJobRequest.JobKey);
        }

        [TestMethod()]
        public void ResetTest()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            CurrentJobRequest.Status.JobCompleted = true;
            CurrentJobRequest.Data.DataLength = 1;
            CurrentJobRequest.Reset();
            Assert.IsTrue(CurrentJobRequest.Status.JobCompleted != true
                && CurrentJobRequest.Data.DataLength != 1);
        }
    }
}