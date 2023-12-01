using HiddenGems.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace HiddenGems.Business
{
    public class TrainerOptions<T>
    {
        public IEnumerable<T> TrainData { get; set; }
        public AlgorithmType SelectedAlgorithm { get; set; }
        public List<string> OriginalColumnNames { get; set; }
        public List<string> TextColumns { get; set; }
        public List<string> NumericColumns { get; set; }
        public string ColumnToAnalyze { get; set; }
        public string OriginalNameOfColumnToAnalyze { get; set; }
        public int MaxAnalysisTimeInSeconds { get; set; }
        public CancellationToken JobCancellationToken { get; set; }

        public TrainerOptions(
            JobRequest currentJobRequest,
            AlgorithmType selectedAlgorithm,
            string columnToAnalyze)
        {
            SelectedAlgorithm = selectedAlgorithm;
            OriginalColumnNames = currentJobRequest.Data.OriginalColumnNames;
            ColumnToAnalyze = columnToAnalyze;
            OriginalNameOfColumnToAnalyze = currentJobRequest.Data.SelectedColumnToAnalyze.Item2;

            JobCancellationToken = currentJobRequest.Status.JobCancellationTokenSource.Token;
            
            TrainData = ((IEnumerable<T>)currentJobRequest.Data.DataToProcess)
                .Where(x => IsValidTrainingData(x, columnToAnalyze, selectedAlgorithm));

            TextColumns = new List<string>();
            NumericColumns = new List<string>();
            for (int i = 1; i <= currentJobRequest.Data.OriginalColumnNames.Count; i++)
            {
                if ("Column" + i == currentJobRequest.Data.SelectedColumnToAnalyze.Item1 || OriginalColumnNames[i - 1] == "SYSTEM_SCORE")
                {
                    continue;
                }

                if (((IEnumerable<T>)currentJobRequest.Data.DataToProcess)
                    .Any(x => Regex.IsMatch(typeof(T).GetProperty("Column" + i).GetValue(x)?.ToString()?.Trim(), @"[^\d\.\-\+]+")))
                {
                    TextColumns.Add("Column" + i);
                }
                else
                {
                    NumericColumns.Add("Column" + i);
                }
            }

            //If the training data comprises less than 5% of the total records, skip the analysis
            //This is intended to avoid situations where a random value happens to be valid but the remainder is not
            if (!TrainData.Skip((int)Math.Round(currentJobRequest.Data.DataToProcess.Count * 0.05, 0)).Any())
            {
                TrainData = new List<T>();
            }

            MaxAnalysisTimeInSeconds = currentJobRequest.Status.EstimatedDuration;
        }

        public static bool IsValidTrainingData(T recordToEvaluate, string columnToAnalyze, AlgorithmType selectedAlgorithm)
        {
            if ((string)recordToEvaluate.GetType().GetProperty(columnToAnalyze).GetValue(recordToEvaluate) == DataRecordBuilder.FillerValue)
            {
                return false;
            }

            var currentLabelValue = (string)recordToEvaluate.GetType().GetProperty(columnToAnalyze).GetValue(recordToEvaluate);

            if (selectedAlgorithm == AlgorithmType.BinaryClassification)
            {
                return bool.TryParse(currentLabelValue, out _)
                            || currentLabelValue.Trim() == "1"
                            || currentLabelValue.Trim() == "0";
            }
            else if (selectedAlgorithm == AlgorithmType.Regression)
            {
                var isFloat = float.TryParse((string)recordToEvaluate.GetType().GetProperty(columnToAnalyze).GetValue(recordToEvaluate), out _);
                return isFloat;
            }

            return false;
        }
    }
}
