using HiddenGems.Common;
using System;
using HiddenGems.Runtime;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Globalization;

namespace HiddenGems.Business
{
    public static partial class AnalyzerHelpers
    {
        public static async void ProcessTrainingRequest<T>(JobRequest currentJobRequest)
            where T : class, IHgDataRecord, new()
        {
            var analysisTasks = new List<Task>();

            for (int i = 0; i < SharedSettings.NumberOfIterations; i++)
            {
                analysisTasks.Add(Task.Run(() => ProcessRequestForAlgorithm<T>(currentJobRequest, AlgorithmType.BinaryClassification)));
                analysisTasks.Add(Task.Run(() => ProcessRequestForAlgorithm<T>(currentJobRequest, AlgorithmType.Regression)));
            }

            await Task.WhenAll(analysisTasks);

            currentJobRequest.Status.StatusMessage = "Wrapping up...";
            currentJobRequest.Status.IsRunning = false;
            currentJobRequest.Status.JobCompleted = true;
        }

        public static Tuple<string, string> ProcessEvaluationRequest<T>(
            JobRequest currentJobRequest, 
            dynamic recordToAnalyze)
            where T : class, IHgDataRecord, new()
        {
            try
            {
                var evaluationResults = new List<EvaluatorResult>();

                foreach (var result in currentJobRequest.Results)
                {
                    if (result.GeneratedDataPrepModel.Length == 0 && result.GeneratedTrainerModel.Length == 0)
                    {
                        continue;
                    }

                    var evaluatorOptions = new EvaluatorOptions<T>(
                        result.GeneratedDataPrepModel,
                        result.GeneratedTrainerModel,
                        result.SelectedAlgorithm,
                        result.LabelColumn,
                        recordToAnalyze);
                    
                    evaluationResults.Add(Analyzer.Evaluate(evaluatorOptions));
                    evaluatorOptions = null;
                }

                float finalScore = 0;
                string finalLabel = "";
                if (evaluationResults.All(x => x.SelectedAlgorithm == AlgorithmType.BinaryClassification))
                {
                    finalLabel = (evaluationResults.Select(x => x.EvaluatedScore).Sum() > 0).ToString();

                    var sortedScores = evaluationResults.Select(x => x.EvaluatedScore).OrderByDescending(x => Math.Abs(x)).ToList();
                    for (int i = 0; i < sortedScores.Count; i++)
                    {
                        if (float.IsInfinity(sortedScores[i]))
                        {
                            sortedScores[i] = float.MaxValue;
                        } else if (float.IsNaN(sortedScores[i]))
                        {
                            sortedScores[i] = float.MinValue;
                        }
                    }
                    var maxValue = sortedScores.Max(x => Math.Abs(x));
                    var normalizationRatio = maxValue > 0 ? 1 / maxValue : 1;
                    sortedScores = sortedScores.Select(x => Math.Abs(x) * normalizationRatio).ToList();
                    finalScore = sortedScores.Sum(x => x) / sortedScores.Count;
                    if (finalScore >= .999)
                    {
                        finalScore = .9999f;
                    }
                    return new Tuple<string, string>(finalLabel, finalScore.ToString("P", CultureInfo.InvariantCulture));
                } else
                {
                    finalLabel = "";

                    finalScore = evaluationResults.Sum(x => x.EvaluatedScore) / evaluationResults.Count;
                    return new Tuple<string, string>(finalLabel, finalScore.ToString());
                }
                
            }
            catch (Exception) { }

            return new Tuple<string, string>("", "0");
        }

        private static void ProcessRequestForAlgorithm<T>(JobRequest currentJobRequest, AlgorithmType selectedAlgorithm)
            where T : class, IHgDataRecord, new()
        {
            try
            {
                var originalColumnName = currentJobRequest.Data.SelectedColumnToAnalyze.Item2;
                currentJobRequest.Status.StatusMessage = $"Building request for {originalColumnName}";

                var trainerOptions = new TrainerOptions<T>(
                    currentJobRequest,
                    selectedAlgorithm,
                    currentJobRequest.Data.SelectedColumnToAnalyze.Item1
                    );

                //Sets when the analysis began as close to the actual analysis to avoid early termination
                currentJobRequest.Status.ExecutionStart =
                    currentJobRequest.Status.ExecutionStart == DateTime.MaxValue ?
                    DateTime.Now :
                    currentJobRequest.Status.ExecutionStart;

                currentJobRequest.Results.Add(RunAnalyzer(trainerOptions, currentJobRequest));

                trainerOptions = null;
            }
            catch (Exception) { }
        }

        private static JobResult RunAnalyzer<T>(
            TrainerOptions<T> trainerOptions,
            JobRequest currentJobRequest
            ) where T : class, IHgDataRecord, new()
        {
            ErrorCode error;
            var generatedDataPrepModel = Array.Empty<byte>();
            var generatedTrainerModel = Array.Empty<byte>();
            var serializedMetrics = "";
            var weights = Array.Empty<float>();

            if (!trainerOptions.TrainData.Any())
            {
                error = ErrorCode.NoWorkDone;
            }
            else
            {
                currentJobRequest.Status.StatusMessage = $"Executing analysis for {trainerOptions.OriginalNameOfColumnToAnalyze}";
                Train(trainerOptions, out error, out generatedDataPrepModel, out generatedTrainerModel, out serializedMetrics, out weights);
            }

            return new JobResult(
                currentJobRequest.JobKey,
                currentJobRequest.Data.SelectedColumnToAnalyze.Item2,
                trainerOptions.SelectedAlgorithm,
                (DateTime.Now - currentJobRequest.Status.ExecutionStart).Seconds,
                trainerOptions.OriginalColumnNames,
                error.GetDescription(),
                generatedDataPrepModel,
                generatedTrainerModel,
                serializedMetrics,
                weights);
        }

        private static void Train<T>(
            TrainerOptions<T> trainerOptions,
            out ErrorCode error,
            out byte[] generatedDataPrepModel,
            out byte[] generatedTrainerModel,
            out string serializedMetrics,
            out float[] weights)
            where T : class
        {
            var result = Analyzer.RunExperiment(trainerOptions);
            error = ErrorCode.NoError;

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                error = JobStatus.GetErrorCode(result.ErrorMessage);
            }

            if (result.DataPrepModel.Length == 0 || result.TrainerModel.Length == 0)
            {
                error = ErrorCode.NoValidResult;
            }

            generatedDataPrepModel = result.DataPrepModel;
            generatedTrainerModel = result.TrainerModel;
            serializedMetrics = result.RawMetrics;
            weights = result.Weights;
        }
    }
}
