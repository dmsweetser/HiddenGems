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
    public class JobResultTests
    {
        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            var configuration = TestHelpers.InitConfiguration();
            SharedSettings.PopulateSharedSettings(configuration);
        }

        [TestMethod()]
        public void JobResultTest_GoodWeights()
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
            Assert.IsTrue(CurrentJobRequest.Results[0].OriginalColumnsWithWeights[0].Item2 == 1
                && CurrentJobRequest.Results[0].OriginalColumnsWithWeights[1].Item2 > 0
                && CurrentJobRequest.Results[0].OriginalColumnsWithWeights[2].Item2 == 0);
        }

        [TestMethod()]
        public void JobResultTest_NullWeights()
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
                null
                ));
            Assert.IsTrue(CurrentJobRequest.Results[0].OriginalColumnsWithWeights[0].Item2 == 0
                && CurrentJobRequest.Results[0].OriginalColumnsWithWeights[1].Item2 == 0
                && CurrentJobRequest.Results[0].OriginalColumnsWithWeights[2].Item2 == 0);
        }

        [TestMethod()]
        public void JobResultTest_EmptyWeights()
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
                new float[0]
                ));
            Assert.IsTrue(CurrentJobRequest.Results[0].OriginalColumnsWithWeights[0].Item2 == 0
                && CurrentJobRequest.Results[0].OriginalColumnsWithWeights[1].Item2 == 0
                && CurrentJobRequest.Results[0].OriginalColumnsWithWeights[2].Item2 == 0);
        }

        [TestMethod()]
        public void GetSummaryTest_GoodResult()
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
            Assert.IsTrue(!string.IsNullOrWhiteSpace(CurrentJobRequest.Results[0].GetSummary()));
        }

        [TestMethod()]
        public void GetHelpTextTest_BinaryClassification()
        {
            var binaryHelpText = JobResult.GetHelpText(AlgorithmType.BinaryClassification);
            Assert.IsTrue(binaryHelpText.Contains("Area Under Roc Curve"));
        }

        [TestMethod()]
        public void GetHelpTextTest_Regression()
        {
            var regressionHelpText = JobResult.GetHelpText(AlgorithmType.Regression);
            Assert.IsTrue(regressionHelpText.Contains("Mean Absolute Error"));
        }
    }
}