using HiddenGems.Business;
using HiddenGems.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static HiddenGems.Common.JobRequestManager;


namespace HiddenGems.Common
{
    public static class JobControllerHelpers
    {
        public static bool UploadDataUrl(string dataUrl, JobRequest currentJobRequest)
        {
            currentJobRequest.Status.UploadCompleted = false;
            currentJobRequest.Status.UploadPercentCompleted = 0;

            currentJobRequest.Status.StatusMessage = "Downloading your data from the provided URL...";
            var dataStream = AnalyzerHelpers.RetrieveCsvFromUrl(dataUrl).Result;
            return UploadData(dataStream, dataUrl, currentJobRequest);
        }

        public static bool UploadData(Stream dataStream, string incomingFileName, JobRequest currentJobRequest)
        {
            if (currentJobRequest == null || dataStream == null)
            {
                return false;
            }
            else
            {
                currentJobRequest.Reset();
            }

            currentJobRequest.Status.UploadCompleted = false;
            currentJobRequest.Status.UploadPercentCompleted = 0;

            currentJobRequest.Status.StatusMessage = "Preparing your data...";
            currentJobRequest.Status.UploadPercentCompleted = .1;

            dynamic recordsToAnalyze = AnalyzerHelpers.ProcessDataStreamFromCsv(dataStream, out List<string> propertyNames);

            for (int i = 0; i < propertyNames.Count; i++)
            {
                propertyNames[i] = propertyNames[i].Trim();
            }

            if (recordsToAnalyze.Count == 0)
            {
                currentJobRequest.Status.Error = ErrorCode.InsufficientData;
                currentJobRequest.Status.StatusMessage = JobStatus.GetFriendlyErrorMessage("", currentJobRequest.Status);
                currentJobRequest.Status.CanRemove = true;
                return false;
            }

            currentJobRequest.Status.StatusMessage = "Categorizing your data...";
            currentJobRequest.Status.UploadPercentCompleted = .5;

            recordsToAnalyze = AnalyzerHelpers.ScrubTrainingData(recordsToAnalyze);

            AnalyzerHelpers.CategorizeColumns(recordsToAnalyze, propertyNames, -1,
                currentJobRequest,
                out List<Tuple<string, string>> eligibleColumns);

            AnalyzerHelpers.PopulateSampleRecord(currentJobRequest, propertyNames, recordsToAnalyze[0]);

            currentJobRequest.Status.StatusMessage = "Finalizing upload...";
            currentJobRequest.Status.UploadPercentCompleted = .99;

            currentJobRequest.ReportIdentifier = incomingFileName;

            currentJobRequest.Data.DataToProcess = recordsToAnalyze;
            currentJobRequest.Data.DataLength = recordsToAnalyze.Count;
            currentJobRequest.Data.OriginalColumnNames = propertyNames;
            currentJobRequest.Data.EligibleColumns = eligibleColumns;

            currentJobRequest.Status.StatusMessage = "";

            currentJobRequest.Status.UploadCompleted = true;
            currentJobRequest.Status.UploadPercentCompleted = 1;

            return true;
        }

        public static bool SubmitRequest(JobRequest currentJobRequest, List<string> selectedColumns, bool skipAnalysis = false)
        {
            try
            {
                if (currentJobRequest == null)
                {
                    return false;
                }
                else
                {
                    currentJobRequest.Status = new JobStatus(currentJobRequest.JobKey);
                    currentJobRequest.Results = new List<JobResult>();
                }


                currentJobRequest.Status.UploadCompleted = false;
                currentJobRequest.Status.UploadPercentCompleted = 0;
                currentJobRequest.Status.StatusMessage = "Starting analysis...";

                CancellationToken ct = currentJobRequest.Status.JobCancellationTokenSource.Token;

                currentJobRequest.Data.SelectedColumnToAnalyze = currentJobRequest.Data.EligibleColumns
                    .Where(x => selectedColumns.Contains(x.Item1))
                    .FirstOrDefault();

                currentJobRequest.Status.EstimatedDuration =
                    currentJobRequest.Status.CalculateEstimatedDuration(
                        currentJobRequest.Data.DataLength,
                        SharedSettings.EstimationMultiplier,
                        SharedSettings.NumberOfIterations);

                if (skipAnalysis)
                {
                    return true;
                }

                var analyzerType = typeof(AnalyzerHelpers);
                var processMethod = analyzerType.GetMethod(nameof(AnalyzerHelpers.ProcessTrainingRequest));
                var genericProcessMethod = processMethod.MakeGenericMethod(currentJobRequest.Data.DataToProcess[0].GetType());
                genericProcessMethod.Invoke(null, new object[] { currentJobRequest });
                return true;
            }
            catch (Exception ex)
            {
                currentJobRequest.Status.Error = JobStatus.GetErrorCode(ex.Message);
                currentJobRequest.Status.StatusMessage = JobStatus.GetFriendlyErrorMessage(ex.Message, currentJobRequest.Status);
                currentJobRequest.Status.CanRemove = true;
                return false;
            }
        }

        public static JobResultDTO GetResult(string jobKey, ILogger logger)
        {
            try
            {
                var existingJob = GetExistingJobRequest(jobKey);
                if (existingJob == null)
                {
                    return new JobResultDTO();
                }

                var columnWeights = existingJob.GetAggregatedJobResults();
                var rawResult = new StringBuilder();
                foreach (var result in existingJob.Results)
                {
                    if (string.IsNullOrWhiteSpace(result.SerializedMetrics)) continue;
                    rawResult.Append(result.GetSummary());
                    rawResult.AppendLine();
                }

                var orderedWeights = columnWeights.OrderByDescending(x => x.Value).ToArray();

                return new JobResultDTO()
                {
                    ColumnWeights = orderedWeights,
                    RawResult = rawResult.ToString(),
                    SerializedSampleRecord = existingJob.Data.SerializedSampleRecord
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new JobResultDTO();
            }
        }

        public static Tuple<string, string> Evaluate(string jobKey, Dictionary<string, string> recordToAnalyze, ILogger logger)
        {
            try
            {
                var existingJob = GetExistingJobRequest(jobKey);
                if (existingJob == null)
                {
                    return new Tuple<string,string>("Error", "0");
                }

                var recordHeader = "\"" + string.Join("\",\"", recordToAnalyze.Keys.Select(x => x.Replace("\"","")).ToList()) + "\"";
                var recordValues = "\"" + string.Join("\",\"", recordToAnalyze.Values) + "\"";
                var recordString = recordHeader + Environment.NewLine + recordValues;

                var recordBytes = Encoding.ASCII.GetBytes(recordString);
                var recordStream = new MemoryStream(recordBytes);

                dynamic recordsToAnalyze = AnalyzerHelpers.ProcessDataStreamFromCsv(recordStream, out List<string> propertyNames);
                recordsToAnalyze = AnalyzerHelpers.ScrubTrainingData(recordsToAnalyze);

                var analyzerType = typeof(AnalyzerHelpers);
                var processMethod = analyzerType.GetMethod(nameof(AnalyzerHelpers.ProcessEvaluationRequest));
                var genericProcessMethod = processMethod.MakeGenericMethod(recordsToAnalyze[0].GetType());
                var result = genericProcessMethod.Invoke(null, new object[] { existingJob, recordsToAnalyze[0] });
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new Tuple<string, string>("", "0");
            }
        }

        public static JobStatus CheckStatus(string jobKey, ILogger logger)
        {
            try
            {
                var existingJob = GetExistingJobRequest(jobKey);
                if (existingJob == null)
                {
                    return new JobStatus(null)
                    {
                        StatusMessage = "Waiting for status..."
                    };
                }

                return existingJob.Status;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new JobStatus(null)
                {
                    StatusMessage = "Something went wrong with your request"
                };
            }
        }

        public static bool CancelJob(string jobKey, ILogger logger)
        {
            try
            {
                var existingJob = GetExistingJobRequest(jobKey);

                if (existingJob == null)
                {
                    return false;
                }

                existingJob.Status.StatusMessage = "Requesting analysis cancellation...";

                existingJob.Status.CanRemove = true;
                existingJob.Status.JobCancellationTokenSource.Cancel();
                PurgeJobs();

                existingJob.Status.StatusMessage = "Analysis cancelled!";

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public static bool ResendEmail(string emailAddress)
        {
            return LicenseManager.ResendActivationCode(emailAddress);
        }

        public static bool Activate(string activationKey)
        {
            LicenseManager.PutLicense(activationKey);
            return LicenseManager.IsLicenseValid(true);
        }
    }
}
