using HiddenGems.Common;
using HiddenGems.Runtime;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HiddenGems.Business
{
    public static partial class Analyzer
    {
        public static EvaluatorResult Evaluate<T>(EvaluatorOptions<T> evaluatorOptions)
            where T : class, IHgDataRecord, new()
        {
            var mlContext = new MLContext();
            var testDataView = mlContext.Data.LoadFromEnumerable(new List<T>() { evaluatorOptions.RecordToEvaluate });

            using (var dataPrepMs = new MemoryStream(evaluatorOptions.ExistingDataPrepModel))
            using (var trainerMs = new MemoryStream(evaluatorOptions.ExistingTrainerModel))
            {
                IDataView initializedData;
                dynamic loadedDataPrepModel;
                dynamic loadedTrainerModel;
                IReadOnlyList<Single> weights = new float[0];
                dynamic predictions;

                try
                {
                    loadedDataPrepModel = mlContext.Model.Load(dataPrepMs, out var inputDataPrepSchema);
                    loadedTrainerModel = mlContext.Model.Load(trainerMs, out var inputTrainerSchema);
                    weights = loadedTrainerModel.LastTransformer.Model.Weights;
                    initializedData = loadedDataPrepModel.Transform(testDataView);
                    predictions = loadedTrainerModel.Transform(initializedData);
                    GetResults(predictions, out List<string> predictedLabels, out List<float> scores);
                    return new EvaluatorResult(evaluatorOptions.SelectedAlgorithm, 
                        predictedLabels.Count > 0 ? predictedLabels[0] : "", 
                        scores.Count > 0 ? scores[0] : 0, "");
                }
                catch (Exception)
                {
                    return new EvaluatorResult(evaluatorOptions.SelectedAlgorithm, "", 0, "NOTICE: Failed to predict using generated model");
                }
                finally
                {
                    mlContext = null;
                    testDataView = null;
                    initializedData = null;
                    loadedDataPrepModel = null;
                    loadedTrainerModel = null;
                    weights = null;
                    predictions = null;
                }
            }
        }

        private static void GetResults(IDataView predictions, out List<string> predictedLabels, out List<float> scores)
        {
            predictedLabels = new List<string>();
            scores = new List<float>();

            if (predictions.Schema.GetColumnOrNull("PredictedLabel") != null)
            {
                predictedLabels = predictions.GetColumn<bool>("PredictedLabel")
                        .ToArray()
                        .Select(x => x.ToString())
                        .ToList();
            }

            if (predictions.Schema.GetColumnOrNull("Score") != null)
            {
                scores = predictions.GetColumn<float>("Score")
                        .ToArray()
                        .Select(x => x)
                        .ToList();
            }            
        }
    }
}
