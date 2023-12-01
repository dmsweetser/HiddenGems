using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HiddenGems.Common
{
    public class JobStatus
    {
        public string ParentJobKey { get; set; }
        public DateTime ExecutionStart { get; set; }
        public int EstimatedDuration { get; set; }
        public string StatusMessage { get; set; }
        public ErrorCode Error { get; set; } = ErrorCode.NoError;
        public double PercentCompleted => GetPercentCompleted();
        public bool IsRunning { get; set; }
        public bool JobCompleted { get; set; }
        public double UploadPercentCompleted { get; set; }
        public bool UploadCompleted { get; set; }

        [JsonIgnore]
        public bool CanRemove { get; set; }

        [JsonIgnore]
        public CancellationTokenSource JobCancellationTokenSource { get; set; }

        public JobStatus(string parentJobKey)
        {
            ParentJobKey = parentJobKey;
            EstimatedDuration = 0;
            ExecutionStart = DateTime.Now;
            CanRemove = false;
            JobCancellationTokenSource = new CancellationTokenSource();
        }
        public int CalculateEstimatedDuration(long dataLength, double requestedDuration, int numberOfIterations)
        {
            var currentLengthInKilobytes = double.Parse(dataLength.ToString()) / 1024;
            var estimatedDuration = currentLengthInKilobytes * requestedDuration * numberOfIterations;
            var roundedEstimatedDuration = Math.Round(estimatedDuration, 0);
            return Convert.ToInt32(roundedEstimatedDuration > SharedSettings.MinimumEstimatedDuration
                ? roundedEstimatedDuration
                : SharedSettings.MinimumEstimatedDuration);
        }

        public double GetPercentCompleted()
        {
            if (EstimatedDuration == 0)
            {
                return 0;
            }

            var currentPercent = (DateTime.Now - ExecutionStart).TotalSeconds / EstimatedDuration;

            if (currentPercent > .8)
            {
                JobCancellationTokenSource.Cancel();
            }

            return currentPercent > 1 ? 1 : currentPercent;
        }

        public static ErrorCode GetErrorCode(string errorText)
        {
            ErrorCode returnedErrorCode = ErrorCode.GenericError;

            foreach (ErrorCode code in (ErrorCode[])Enum.GetValues(typeof(ErrorCode)))
            {
                if (errorText == code.GetDescription())
                {
                    returnedErrorCode = code;
                }
            }

            return returnedErrorCode;
        }

        public static string GetFriendlyErrorMessage(string errorMessage, JobStatus jobStatus)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
            {
                return errorMessage;
            }

            if (jobStatus.Error == ErrorCode.NoWorkDone)
            {
                return "It looks like the file you uploaded didn't have any work for me to do. Please try again.";
            }
            else if (jobStatus.Error == ErrorCode.NoValidResult)
            {
                return "The file was analyzed but no valid results could be found. Please make sure you have at least one column that is numeric or true/false and try again.";
            } else if (jobStatus.Error == ErrorCode.InsufficientData)
            {
                return "It looks like your data didn't have enough rows that could be used for training, or that your file is missing headers for each column.";
            }
            else
            {
                return jobStatus.Error.GetDescription();
            }
        }
    }
}
