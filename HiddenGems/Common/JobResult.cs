using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HiddenGems.Common
{
    public class JobResult
    {
        public string ParentJobKey { get; set; }
        public string LabelColumn { get; set; }
        public AlgorithmType SelectedAlgorithm { get; set; }
        public int TotalAnalysisDurationInSeconds { get; set; }
        public List<Tuple<string, float>> OriginalColumnsWithWeights { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
        public byte[] GeneratedDataPrepModel { get; set; }
        public byte[] GeneratedTrainerModel { get; set; }
        public string SerializedMetrics { get; set; }

        public JobResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
        public JobResult(
            string parentJobKey,
            string labelColumn,
            AlgorithmType algorithmType,
            int totalAnalysisDurationInSeconds,
            List<string> originalColumns,
            string errorMessage,
            byte[] generatedDataPrepModel,
            byte[] generatedTrainerModel,
            string serializedMetrics,
            float[] weights
            )
        {
            ParentJobKey = parentJobKey;
            LabelColumn = labelColumn;
            SelectedAlgorithm = algorithmType;
            TotalAnalysisDurationInSeconds = totalAnalysisDurationInSeconds;
            OriginalColumnsWithWeights = originalColumns.Select(x => new Tuple<string, float>(x, 0)).ToList();
            ErrorMessage = errorMessage;
            GeneratedDataPrepModel = generatedDataPrepModel;
            GeneratedTrainerModel = generatedTrainerModel;
            SerializedMetrics = serializedMetrics;
            AssignAndNormalizeWeights(weights);
        }

        private void AssignAndNormalizeWeights(float[] weights)
        {
            if (weights == null || weights.Length == 0)
            {
                return;
            }

            for (int i = 0; i < weights.Length; i++)
            {
                if (i == OriginalColumnsWithWeights.Count)
                {
                    break;
                }

                if (OriginalColumnsWithWeights[i].Item1 == LabelColumn)
                {
                    continue;
                }

                OriginalColumnsWithWeights[i] = new Tuple<string, float>(OriginalColumnsWithWeights[i].Item1, weights[i]);
            }

            OriginalColumnsWithWeights = OriginalColumnsWithWeights.OrderByDescending(x => Math.Abs(x.Item2)).ToList();
            var maxValue = OriginalColumnsWithWeights.Max(x => Math.Abs(x.Item2));
            var normalizationRatio = maxValue > 0 ? 1 / maxValue : 1;

            OriginalColumnsWithWeights = OriginalColumnsWithWeights.Select(x =>
            {
                return new Tuple<string, float>(x.Item1, Math.Abs(x.Item2) * normalizationRatio);
            }).ToList();
        }

        public string GetSummary()
        {
            try
            {
                var summary = new StringBuilder();
                summary.Append("<br/>________________________________<br/><br/><strong>Analyzed Column: </strong>" + LabelColumn);
                summary.Append("<br/><strong>Total Analysis Duration:</strong> " + TimeSpan.FromSeconds(TotalAnalysisDurationInSeconds).ToString(@"mm\:ss") + "");
                summary.Append("<br/><br/><strong>Algorithm Class:</strong> " + SelectedAlgorithm.GetDescription());
                summary.Append("<br/><br/><strong>Summary:</strong>&nbsp;This analysis indicates that the column <strong>" + OriginalColumnsWithWeights[0].Item1 + "</strong> contributed most to the value of <strong>" + LabelColumn + "</strong>.");
                summary.Append("<br/><br/><strong>Normalized Column Weights:</strong><br/>");

                foreach (var providedColumn in OriginalColumnsWithWeights)
                {
                    if (providedColumn.Item1 == LabelColumn)
                    {
                        continue;
                    }
                    summary.Append($"<br/><strong>Column Name:</strong> {providedColumn.Item1}<br/><strong>Weight:</strong> {providedColumn.Item2}");
                }

                if (!string.IsNullOrWhiteSpace(SerializedMetrics))
                {
                    summary.Append("<br/><br/><strong>Raw Metrics:</strong><br/>");
                    summary.Append("<br/>" + SerializedMetrics + "<br/>");
                }

                return summary.ToString();
            }
            catch (Exception)
            {
                return "Error when generating summary";
            }
        }

        public static string GetHelpText(AlgorithmType selectedAlgorithm)
        {

            switch (selectedAlgorithm)
            {
                case AlgorithmType.BinaryClassification:
                    return @"
Metrics Explained:

Area Under Roc Curve [Value between 0.0 and 1.0, higher is better]: 
If the model is given a value that should be true and one that should be false, this indicates how likely the model is to tell which is which. It is more useful to you if you have an even split between your values that should be true and those that should be false.

Accuracy [Value between 0.0 and 1.0, higher is better]: 
This indicates how often the model will make a correct prediction.

Positive Precision [Value between 0.0 and 1.0, higher is better]: 
This indicates how well the model can avoid false positive results.

Positive Recall [Value between 0.0 and 1.0, higher is better]: 
This indicates how well the model can predict a value that should actually be true.

Negative Precision [Values between 0.0 and 1.0, higher is better]: 
This indicates how well the model can avoid false negative results.

Negative Recall [Value between 0.0 and 1.0, higher is better]: 
This indicates how well the model can predict a value that should be actually negative.

F1 Score [Value between 0.0 and 1.0, higher is better]: 
This is a calculated quality of the model that takes into consideration precision and recall.

Area Under Precision Recall Curve [Value between 0.0 and 1.0, higher is better]: 
This is a calculated value that takes into consideration precision and recall. It is more useful than the Area Under Roc Curve when you don't have an even split between values that should be true and values that should be false.

Confusion Matrix [Values vary]: 
This breaks down the precision and recall of your model based on the class (true or false, in this case), and also tells you a count of times the model correctly predicated a value of true, incorrectly predicted a value of true, correctly predicted a value of false, and incorrectly predicted a value of false.

Number Of Classes [Value should be 2]: 
This indicates that the model is attempting to predict for two different values, which it translates internally to true and false.
";
                case AlgorithmType.Regression:
                    return @"
Metrics Explained:

Mean Absolute Error [Value varies, smaller is better]: 
This indicates, on average, the difference between the value the model predicted and what the value should have been.

Mean Squared Error [Value varies, smaller is better]: 
This indicates, on average, the squared difference between the value the model predicted and what the value should have been. This value is not in the same measure as the values in your actual data, so it can be hard to evaluate. Keep in mind the scientific notation - this may be very small!

Root Mean Squared Error [Value varies, smaller is better]: 
This is the square root of the Mean Squared Error above, and indicates the difference between the value the model predicted and what the value should have been. This value is in the same measure as the values in your actual data, so it may be more helpful when thinking about how well the model performed.

Loss Function [Value varies, smaller is better]: 
A calculated value that indicates how well the model performed. Keep in mind the scientific notation - this may be very small!

R Squared [Value between 0.0 and 1.0, higher is better]: 
This indicates how accurately the model can predict a result based on the data you gave it to train.
";
                default:
                    return "";
            }
        }
    }
}
