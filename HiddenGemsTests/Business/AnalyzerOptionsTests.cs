using Microsoft.VisualStudio.TestTools.UnitTesting;
using HiddenGems.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiddenGemsTests;
using HiddenGems.Common;
using System.IO;

namespace HiddenGems.Business.Tests
{
    [TestClass()]
    public class TrainerOptionsTests
    {
        public class TrainerOptionsTestRecord
        {
            public string Column1 { get; set; }
            public string Column2 { get; set; }
            public string Column3 { get; set; }
            public string Column4 { get; set; }
            public string Column5 { get; set; }
        }


        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            var configuration = TestHelpers.InitConfiguration();
            SharedSettings.PopulateSharedSettings(configuration);
        }

        [TestMethod()]
        public void TrainerOptionsTest_NotEnoughTrainingData()
        {
            using FileStream sampleDataStream = new("wwwroot\\samples\\IsItOrIsntIt.csv", FileMode.Open, FileAccess.Read);
            var incomingFileName = sampleDataStream.Name;
            var CurrentJobRequest = JobRequestManager.GetOrAddJobRequest(Guid.NewGuid().ToString());
            JobControllerHelpers.UploadData(sampleDataStream, incomingFileName, CurrentJobRequest);
            JobControllerHelpers.SubmitRequest(
                CurrentJobRequest,
                new List<string>() { CurrentJobRequest.Data.EligibleColumns[0].Item1 },
                true
                );

            CurrentJobRequest.Data.DataToProcess = new List<TrainerOptionsTestRecord>() {
                new TrainerOptionsTestRecord { Column1 = "", Column2 = "test", Column3 = "", Column4 = "", Column5 = "" },
                new TrainerOptionsTestRecord { Column1 = "", Column2 = "test", Column3 = "", Column4 = "", Column5 = "" },
                new TrainerOptionsTestRecord { Column1 = "", Column2 = "test", Column3 = "", Column4 = "", Column5 = "" },
                new TrainerOptionsTestRecord { Column1 = "", Column2 = "test", Column3 = "", Column4 = "", Column5 = "" },
            };

            var helpersType = typeof(TestHelpers);
            var processMethod = helpersType.GetMethod(nameof(TestHelpers.GetTrainerOptions));
            var genericProcessMethod = processMethod.MakeGenericMethod(CurrentJobRequest.Data.DataToProcess[0].GetType());
            var options = genericProcessMethod.Invoke(null, new object[] {
                CurrentJobRequest.Data.DataToProcess[0],
                CurrentJobRequest,
                AlgorithmType.BinaryClassification,
                "Column1"
            });

            Assert.IsTrue(options.TrainData.Count == 0);
        }

        [TestMethod()]
        public void IsValidTrainingDataTest_IsFillerValue_BinaryClassification()
        {
            var result = TrainerOptions<TrainerOptionsTestRecord>.IsValidTrainingData(
                new TrainerOptionsTestRecord { Column1 = " ", Column2 = "test" },
                "Column1",
                AlgorithmType.BinaryClassification);
            Assert.IsTrue(!result);
        }

        [TestMethod()]
        public void IsValidTrainingDataTest_IsFillerValue_Regression()
        {
            var result = TrainerOptions<TrainerOptionsTestRecord>.IsValidTrainingData(
                new TrainerOptionsTestRecord { Column1 = " ", Column2 = "test" },
                "Column1",
                AlgorithmType.Regression);
            Assert.IsTrue(!result);
        }

        [TestMethod()]
        public void IsValidTrainingDataTest_GoodBinaryClassificationData_1()
        {
            var result = TrainerOptions<TrainerOptionsTestRecord>.IsValidTrainingData(
                new TrainerOptionsTestRecord { Column1 = "1", Column2 = "test" },
                "Column1",
                AlgorithmType.BinaryClassification);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsValidTrainingDataTest_GoodBinaryClassificationData_0()
        {
            var result = TrainerOptions<TrainerOptionsTestRecord>.IsValidTrainingData(
                new TrainerOptionsTestRecord { Column1 = "0", Column2 = "test" },
                "Column1",
                AlgorithmType.BinaryClassification);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsValidTrainingDataTest_GoodBinaryClassificationData_True()
        {
            var result = TrainerOptions<TrainerOptionsTestRecord>.IsValidTrainingData(
                new TrainerOptionsTestRecord { Column1 = "True", Column2 = "test" },
                "Column1",
                AlgorithmType.BinaryClassification);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsValidTrainingDataTest_GoodBinaryClassificationData_Yes()
        {
            var result = TrainerOptions<TrainerOptionsTestRecord>.IsValidTrainingData(
                new TrainerOptionsTestRecord { Column1 = "True", Column2 = "test" },
                "Column1",
                AlgorithmType.BinaryClassification);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsValidTrainingDataTest_BadBinaryClassificationData_Decimal()
        {
            var result = TrainerOptions<TrainerOptionsTestRecord>.IsValidTrainingData(
                new TrainerOptionsTestRecord { Column1 = "1.45", Column2 = "test" },
                "Column1",
                AlgorithmType.BinaryClassification);
            Assert.IsTrue(!result);
        }

        [TestMethod()]
        public void IsValidTrainingDataTest_BadBinaryClassificationData_Text()
        {
            var result = TrainerOptions<TrainerOptionsTestRecord>.IsValidTrainingData(
                new TrainerOptionsTestRecord { Column1 = "test", Column2 = "test" },
                "Column1",
                AlgorithmType.BinaryClassification);
            Assert.IsTrue(!result);
        }

        [TestMethod()]
        public void IsValidTrainingDataTest_GoodRegressionData_Decimal()
        {
            var result = TrainerOptions<TrainerOptionsTestRecord>.IsValidTrainingData(
                new TrainerOptionsTestRecord { Column1 = "1.45", Column2 = "test" },
                "Column1",
                AlgorithmType.Regression);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsValidTrainingDataTest_BadRegressionData_True()
        {
            var result = TrainerOptions<TrainerOptionsTestRecord>.IsValidTrainingData(
                new TrainerOptionsTestRecord { Column1 = "True", Column2 = "test" },
                "Column1",
                AlgorithmType.Regression);
            Assert.IsTrue(!result);
        }
    }
}