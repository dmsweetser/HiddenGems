using HiddenGems.Business;
using HiddenGems.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HiddenGemsTests
{
    public static class TestHelpers
    {
        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            return config;
        }

        public static Task RunAnalysisForFirstColumn(string fileName, JobRequest currentJobRequest)
        {
            return Task.Run(async () =>
            {
                using FileStream sampleDataStream = new(fileName, FileMode.Open, FileAccess.Read);
                var incomingFileName = sampleDataStream.Name;
                JobControllerHelpers.UploadData(sampleDataStream, incomingFileName, currentJobRequest);
                JobControllerHelpers.SubmitRequest(currentJobRequest, new List<string>() { currentJobRequest.Data.EligibleColumns[0].Item1 });
                while (!JobControllerHelpers.CheckStatus(currentJobRequest.JobKey, new MockLogger()).JobCompleted)
                {
                    //Wait until analysis completes
                    await Task.Delay(5000);
                }
                return;
            });
        }

        public static Task RunAnalysisForAllColumns(string fileName, JobRequest currentJobRequest)
        {
            return Task.Run(async () =>
            {
                using (FileStream sampleDataStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    var incomingFileName = sampleDataStream.Name;
                    var jobKey = Guid.NewGuid().ToString();
                    JobControllerHelpers.UploadData(sampleDataStream, incomingFileName, currentJobRequest);
                    JobControllerHelpers.SubmitRequest(currentJobRequest, currentJobRequest.Data.EligibleColumns.Select(x => x.Item1).ToList());
                    while (!JobControllerHelpers.CheckStatus(currentJobRequest.JobKey, new MockLogger()).JobCompleted)
                    {
                        //Wait until analysis completes
                        await Task.Delay(5000);
                    }
                    return;
                }
            });
        }

        public static TrainerOptions<T> GetTrainerOptions<T>(
                T sampleRecord,
                JobRequest currentJobRequest,
                AlgorithmType selectedAlgorithm,
                string columnToAnalyze
        )
        {
            return new TrainerOptions<T>(currentJobRequest, selectedAlgorithm, columnToAnalyze);
        }
    }
}
