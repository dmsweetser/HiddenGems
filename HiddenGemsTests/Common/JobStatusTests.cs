using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HiddenGemsTests;
using Microsoft.Extensions.Hosting;

namespace HiddenGems.Common.Tests
{
    [TestClass()]
    public class JobStatusTests
    {
        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            var configuration = TestHelpers.InitConfiguration();
            SharedSettings.PopulateSharedSettings(configuration);
        }

        [TestMethod()]
        public void JobStatusTest_GetParentJob()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            Assert.IsTrue(CurrentJobRequest.Status.ParentJobKey == CurrentJobRequest.JobKey);
        }

        [TestMethod()]
        public void JobStatusTest_GetInitialExecutionStart()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            Assert.IsTrue(CurrentJobRequest.Status.ExecutionStart.Date == DateTime.Now.Date);
        }

        [TestMethod()]
        public void JobStatusTest_GetInitialErrorCode()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            Assert.IsTrue(CurrentJobRequest.Status.Error == ErrorCode.NoError);
        }

        [TestMethod()]
        public void JobStatusTest_GetPercentCompletedWhenZero()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            CurrentJobRequest.Status.EstimatedDuration = 0;
            Assert.IsTrue(CurrentJobRequest.Status.GetPercentCompleted() == 0);
        }

        [TestMethod()]
        public void CalculateEstimatedDurationTest_LessThanMinimum()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            var estimatedDuration = CurrentJobRequest.Status.CalculateEstimatedDuration(
                1, 
                SharedSettings.EstimationMultiplier,
                SharedSettings.NumberOfIterations);
            Assert.IsTrue(estimatedDuration == SharedSettings.MinimumEstimatedDuration);
        }

        [TestMethod()]
        public void CalculateEstimatedDurationTest_MoreThanMinimum()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            var estimatedDuration = CurrentJobRequest.Status.CalculateEstimatedDuration(
                int.MaxValue,
                SharedSettings.EstimationMultiplier,
                SharedSettings.NumberOfIterations);
            Assert.IsTrue(estimatedDuration > SharedSettings.MinimumEstimatedDuration);
        }

        [TestMethod()]
        public void CalculateEstimatedDurationTest_DataLengthOfZero()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            var estimatedDuration = CurrentJobRequest.Status.CalculateEstimatedDuration(
                0,
                SharedSettings.EstimationMultiplier,
                SharedSettings.NumberOfIterations);
            Assert.IsTrue(estimatedDuration == SharedSettings.MinimumEstimatedDuration);
        }

        [TestMethod()]
        public void CalculateEstimatedDurationTest_NoColumnsToAnalyze()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            var estimatedDuration = CurrentJobRequest.Status.CalculateEstimatedDuration(
                1,
                SharedSettings.EstimationMultiplier,
                SharedSettings.NumberOfIterations);
            Assert.IsTrue(estimatedDuration == SharedSettings.MinimumEstimatedDuration);
        }

        [TestMethod()]
        public async Task GetPercentCompletedTest_PercentGreaterThanOne()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            CurrentJobRequest.Status.EstimatedDuration = 1;
            await Task.Delay(1100);
            var percentCompleted = CurrentJobRequest.Status.GetPercentCompleted();
            Assert.IsTrue(percentCompleted == 1);
        }

        [TestMethod()]
        public void GetPercentCompletedTest_EstimatedDurationOfZero()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            CurrentJobRequest.Status.EstimatedDuration = 0;
            var percentCompleted = CurrentJobRequest.Status.GetPercentCompleted();
            Assert.IsTrue(percentCompleted == 0);
        }

        [TestMethod()]
        public void MatchErrorCodeTest_NoError()
        {
            var match = JobStatus.GetErrorCode("");
            Assert.IsTrue(match == ErrorCode.NoError);
        }

        [TestMethod()]
        public void MatchErrorCodeTest_HasError()
        {
            var match = JobStatus.GetErrorCode("No work was done");
            Assert.IsTrue(match == ErrorCode.NoWorkDone);
        }

        [TestMethod()]
        public void GetFriendlyErrorMessageTest_IsDevEnvironment()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Development);
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            var message = JobStatus.GetFriendlyErrorMessage("No work was done", CurrentJobRequest.Status);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Development);
            Assert.IsTrue(message == "No work was done");
        }

        [TestMethod()]
        public void GetFriendlyErrorMessageTest_IsNoWorkDoneNonDevEnvironment()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Production);
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            CurrentJobRequest.Status.Error = ErrorCode.NoWorkDone;
            var message = JobStatus.GetFriendlyErrorMessage("No work was done", CurrentJobRequest.Status);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Development);
            Assert.IsTrue(message == "It looks like the file you uploaded didn't have any work for me to do. Please try again.");
        }

        [TestMethod()]
        public void GetFriendlyErrorMessageTest_IsUnhandledErrorCode()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Production);
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            CurrentJobRequest.Status.Error = ErrorCode.GenericError;
            var message = JobStatus.GetFriendlyErrorMessage("Test message", CurrentJobRequest.Status);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Development);
            Assert.IsTrue(message == ErrorCode.GenericError.GetDescription());
        }
    }
}