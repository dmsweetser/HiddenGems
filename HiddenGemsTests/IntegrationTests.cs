using Microsoft.VisualStudio.TestTools.UnitTesting;
using HiddenGems.Common;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace HiddenGemsTests
{
    [TestClass()]
    public class IntegrationTests
    {
        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            var configuration = TestHelpers.InitConfiguration();
            SharedSettings.PopulateSharedSettings(configuration);
        }

        [TestMethod()]
        public async Task Analyze_BinaryClassificationData()
        {
            var totalCount = 3;
            var correctCount = 0;
            for (int i = 0; i < totalCount; i++)
            {
                var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
                await TestHelpers.RunAnalysisForFirstColumn("wwwroot\\samples\\IsItOrIsntIt.csv", CurrentJobRequest);
                if (CurrentJobRequest.Results.Any(x => x.SelectedAlgorithm == AlgorithmType.BinaryClassification
                        && x.OriginalColumnsWithWeights.OrderByDescending(y => y.Item2)
                        .First().Item1 == "CarAge"))
                {
                    correctCount += 1;
                }
            }
            float correctProportion = (float)correctCount / totalCount;
            Assert.IsTrue(correctProportion >= .7);
        }

        [TestMethod()]
        public async Task Analyze_BinaryClassificationData_QuotedColumnNames()
        {
            var totalCount = 3;
            var correctCount = 0;
            for (int i = 0; i < totalCount; i++)
            {
                var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
                await TestHelpers.RunAnalysisForFirstColumn("wwwroot\\samples\\IsItOrIsntIt_QuotedColumnNames.csv", CurrentJobRequest);
                if (CurrentJobRequest.Results.Any(x => x.SelectedAlgorithm == AlgorithmType.BinaryClassification
                        && x.OriginalColumnsWithWeights.OrderByDescending(y => y.Item2)
                        .First().Item1 == "CarAge"))
                {
                    correctCount += 1;
                }
            }
            float correctProportion = (float)correctCount / totalCount;
            Assert.IsTrue(correctProportion >= .7);
        }

        [TestMethod()]
        public async Task Analyze_RegressionData()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            await TestHelpers.RunAnalysisForFirstColumn("wwwroot\\samples\\WhatWillTheValueBe.csv", CurrentJobRequest);
            Assert.IsTrue(CurrentJobRequest.Results.Any(x => x.SelectedAlgorithm == AlgorithmType.Regression
            && x.OriginalColumnsWithWeights.OrderByDescending(y => y.Item2).First().Item1 == "KellyBlueBookRating"));
        }

        [TestMethod()]
        public async Task Analyze_UsedCarData()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            await TestHelpers.RunAnalysisForFirstColumn("wwwroot\\samples\\UsedCarsSample.csv", CurrentJobRequest);
            Assert.IsTrue(CurrentJobRequest.Results.All(x => !x.HasError || x.ErrorMessage == ErrorCode.NoWorkDone.GetDescription()));
        }

        [TestMethod()]
        public async Task Evaluate_BinaryClassificationData_False()
        {
            var jobKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(jobKey);
            await TestHelpers.RunAnalysisForFirstColumn("wwwroot\\samples\\IsItOrIsntIt.csv", CurrentJobRequest);
            var recordToAnalyze = new Dictionary<string, string>()
            {
                ["ShouldIBuyTheCar"] = "",
                ["CarAge"] = "100",
                ["TotalPreviousRepairs"] = "",
                ["KellyBlueBlookRating"] = "",
                ["CrashRating"] = ""
            };

            var result = JobControllerHelpers.Evaluate(jobKey, recordToAnalyze, new MockLogger());
            Assert.IsTrue(result.Item1 == "False");
        }

        [TestMethod()]
        public async Task Evaluate_BinaryClassificationData_True()
        {
            var jobKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(jobKey);
            await TestHelpers.RunAnalysisForFirstColumn("wwwroot\\samples\\IsItOrIsntIt.csv", CurrentJobRequest);
            var recordToAnalyze = new Dictionary<string, string>()
            {
                ["ShouldIBuyTheCar"] = "",
                ["CarAge"] = "1",
                ["TotalPreviousRepairs"] = "",
                ["KellyBlueBlookRating"] = "",
                ["CrashRating"] = ""
            };

            var result = JobControllerHelpers.Evaluate(jobKey, recordToAnalyze, new MockLogger());
            Assert.IsTrue(result.Item1 == "True");
        }

        [TestMethod()]
        public async Task Evaluate_RegressionData_12000()
        {
            var jobKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(jobKey);
            await TestHelpers.RunAnalysisForFirstColumn("wwwroot\\samples\\WhatWillTheValueBe.csv", CurrentJobRequest);
            var recordToAnalyze = new Dictionary<string, string>()
            {
                ["CurrentValue"] = "",
                ["CarAge"] = "5",
                ["TotalPreviousRepairs"] = "1500",
                ["KellyBlueBlookRating"] = "5",
                ["CrashRating"] = "4"
            };

            var result = JobControllerHelpers.Evaluate(jobKey, recordToAnalyze, new MockLogger());
            Assert.IsTrue(Math.Abs(float.Parse(result.Item2) - 12000) < Math.Abs(float.Parse(result.Item2) - 3000));
        }

        [TestMethod()]
        public async Task Evaluate_RegressionData_3000()
        {
            var jobKey = Guid.NewGuid().ToString();
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(jobKey);
            await TestHelpers.RunAnalysisForFirstColumn("wwwroot\\samples\\WhatWillTheValueBe.csv", CurrentJobRequest);
            var recordToAnalyze = new Dictionary<string, string>()
            {
                ["CurrentValue"] = "",
                ["CarAge"] = "25",
                ["TotalPreviousRepairs"] = "10000",
                ["KellyBlueBlookRating"] = "1",
                ["CrashRating"] = "3"
            };

            var result = JobControllerHelpers.Evaluate(jobKey, recordToAnalyze, new MockLogger());
            Assert.IsTrue(Math.Abs(float.Parse(result.Item2) - 12000) > Math.Abs(float.Parse(result.Item2) - 3000));
        }

        [TestMethod()]
        public async Task Workflow_CancelJob()
        {
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            await TestHelpers.RunAnalysisForFirstColumn("wwwroot\\samples\\UsedCarsSample.csv", CurrentJobRequest);
            JobControllerHelpers.CancelJob(CurrentJobRequest.JobKey, new MockLogger());
            Assert.IsTrue(JobRequestManager.GetExistingJobRequest(CurrentJobRequest.JobKey) == null);
        }
    }
}
