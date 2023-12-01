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
    public class JobRequestManagerTests
    {
        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            var configuration = TestHelpers.InitConfiguration();
            SharedSettings.PopulateSharedSettings(configuration);
        }

        [TestMethod()]
        public void GetOrAddJobRequestTest_NewJob()
        {
            JobRequestManager.ResetQueue();
            var expectedKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            Assert.IsTrue(expectedKey == CurrentJobRequest.JobKey);
        }

        [TestMethod()]
        public void GetOrAddJobRequestTest_ExistingJob()
        {
            JobRequestManager.ResetQueue();
            var expectedKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            var SameJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            Assert.IsTrue(expectedKey == CurrentJobRequest.JobKey && CurrentJobRequest == SameJobRequest);
        }

        [TestMethod()]
        public void GetOrAddJobRequestTest_PurgeAndGetNew()
        {
            JobRequestManager.ResetQueue();
            var expectedKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            CurrentJobRequest.Status.CanRemove = true;
            CurrentJobRequest.Data.DataLength = 1;
            var secondKey = Guid.NewGuid().ToString();
            var NewJobRequest = JobRequestManager.GetOrAddJobRequest(secondKey);
            var OriginalJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            Assert.IsTrue(expectedKey == OriginalJobRequest.JobKey && OriginalJobRequest.Data.DataLength != 1);
        }

        [TestMethod()]
        public void GetOrAddJobRequestTest_PurgeAndGetExisting()
        {
            JobRequestManager.ResetQueue();
            var expectedKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            CurrentJobRequest.Data.DataLength = 1;
            var secondKey = Guid.NewGuid().ToString();
            var NewJobRequest = JobRequestManager.GetOrAddJobRequest(secondKey);
            var OriginalJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            Assert.IsTrue(expectedKey == OriginalJobRequest.JobKey && OriginalJobRequest.Data.DataLength == 1);
        }

        [TestMethod()]
        public void GetExistingJobRequestTest_DoesNotExist()
        {
            JobRequestManager.ResetQueue();
            var expectedKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetExistingJobRequest(expectedKey);
            Assert.IsTrue(CurrentJobRequest == null);
        }

        [TestMethod()]
        public void GetExistingJobRequestTest_Exists()
        {
            JobRequestManager.ResetQueue();
            var expectedKey = Guid.NewGuid().ToString();
            var FirstJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            var SameJobRequest = JobRequestManager.GetExistingJobRequest(expectedKey);
            Assert.IsTrue(SameJobRequest != null);
        }

        [TestMethod()]
        public void GetExistingJobRequestTest_PurgeAndGetExisting()
        {
            JobRequestManager.ResetQueue();
            var expectedKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            CurrentJobRequest.Data.DataLength = 1;
            var secondKey = Guid.NewGuid().ToString();
            var NewJobRequest = JobRequestManager.GetOrAddJobRequest(secondKey);
            var OriginalJobRequest = JobRequestManager.GetExistingJobRequest(expectedKey);
            Assert.IsTrue(expectedKey == OriginalJobRequest.JobKey && OriginalJobRequest.Data.DataLength == 1);
        }

        [TestMethod()]
        public void GetExistingJobRequestTest_PurgeExistingAndGetNoRequest()
        {
            JobRequestManager.ResetQueue();
            var expectedKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            CurrentJobRequest.Status.CanRemove = true;
            CurrentJobRequest.Data.DataLength = 1;
            var secondKey = Guid.NewGuid().ToString();
            var NewJobRequest = JobRequestManager.GetOrAddJobRequest(secondKey);
            var OriginalJobRequest = JobRequestManager.GetExistingJobRequest(expectedKey);
            Assert.IsTrue(OriginalJobRequest == null);
        }

        [TestMethod()]
        public void PurgeJobsTest_HasJobsToPurge()
        {
            JobRequestManager.ResetQueue();
            var expectedKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            CurrentJobRequest.Status.CanRemove = true;
            CurrentJobRequest.Data.DataLength = 1;
            var currentCount = JobRequestManager.GetCurrentJobCount();
            JobRequestManager.PurgeJobs();
            var postPurgeCount = JobRequestManager.GetCurrentJobCount();
            Assert.IsTrue(postPurgeCount == 0 && currentCount == 1);
        }

        [TestMethod()]
        public void PurgeJobsTest_NoJobsToPurge()
        {
            JobRequestManager.ResetQueue();
            var expectedKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(expectedKey);
            var currentCount = JobRequestManager.GetCurrentJobCount();
            JobRequestManager.PurgeJobs();
            var postPurgeCount = JobRequestManager.GetCurrentJobCount();
            Assert.IsTrue(postPurgeCount == 1 && currentCount == 1);
        }
    }
}