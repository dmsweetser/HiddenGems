using HiddenGems.Common;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HiddenGems.Business
{
    public static partial class Analyzer
    {
        public static TrainerResult RunExperiment<T>(TrainerOptions<T> trainerOptions)
            where T : class
        {
            switch (trainerOptions.SelectedAlgorithm)
            {
                case AlgorithmType.BinaryClassification:
                    return ExecuteBinaryClassification(trainerOptions); ;
                case AlgorithmType.Regression:
                    return ExecuteRegression(trainerOptions);
                default:
                    return new TrainerResult("Selected algorithm not found");
            }
        }

        private static TrainerResult ExecuteBinaryClassification<T>(TrainerOptions<T> trainerOptions)
            where T : class
        {
            var mlContext = new MLContext();
            var trainDataView = mlContext.Data.LoadFromEnumerable(trainerOptions.TrainData);
            IDataView initializedData;
            dynamic initializer;
            dynamic dataPrepTransformer;
            BinaryClassificationExperiment experiment;
            ExperimentResult<BinaryClassificationMetrics> result;
            dynamic model;
            IReadOnlyList<Single> weights;

            try
            {
                initializer = AnalyzerPreFeaturizer.GetPreFeaturizer(
                    mlContext,
                    trainerOptions.ColumnToAnalyze,
                    trainerOptions.TextColumns,
                    trainerOptions.NumericColumns,
                    DataKind.Boolean
                    );
                
                dataPrepTransformer = initializer.Fit(trainDataView);
                initializedData = dataPrepTransformer.Transform(trainDataView);

                var settings = new BinaryExperimentSettings
                {
                    MaxExperimentTimeInSeconds = (uint)trainerOptions.MaxAnalysisTimeInSeconds,
                    CancellationToken = trainerOptions.JobCancellationToken,
                    CacheBeforeTrainer = CacheBeforeTrainer.On
                };

                settings.Trainers.Clear();
                settings.Trainers.Add(BinaryClassificationTrainer.AveragedPerceptron);
                settings.Trainers.Add(BinaryClassificationTrainer.LinearSvm);

                experiment = mlContext.Auto().CreateBinaryClassificationExperiment(settings);
                result = experiment.Execute(initializedData);

                if (result.BestRun == null)
                {
                    return new TrainerResult(result.RunDetails.FirstOrDefault()?.Exception.Message);
                }

                model = result.BestRun.Model;
                weights = model.LastTransformer.Model.Weights;

                using (var dataPrepMs = new MemoryStream())
                using (var trainerMs = new MemoryStream())
                {
                    mlContext.Model.Save(dataPrepTransformer, trainDataView.Schema, dataPrepMs);
                    mlContext.Model.Save(result.BestRun.Model, initializedData.Schema, trainerMs);
                    return new TrainerResult(trainerMs.ToArray(), dataPrepMs.ToArray(), weights.ToArray(), JsonConvert.SerializeObject(new { result.BestRun.TrainerName, result.BestRun.ValidationMetrics }, Formatting.Indented));
                }
            }
            finally
            {
                mlContext = null;
                trainDataView = null;
                initializedData = null;
                initializer = null;
                experiment = null;
                model = null;
            }
        }

        private static TrainerResult ExecuteRegression<T>(TrainerOptions<T> trainerOptions)
            where T : class
        {
            var mlContext = new MLContext();
            var trainDataView = mlContext.Data.LoadFromEnumerable(trainerOptions.TrainData);
            IDataView initializedData;
            dynamic initializer;
            dynamic dataPrepTransformer;
            RegressionExperiment experiment;
            ExperimentResult<RegressionMetrics> result;
            dynamic model;
            IReadOnlyList<Single> weights;

            try
            {
                initializer = AnalyzerPreFeaturizer.GetPreFeaturizer(
                    mlContext,
                    trainerOptions.ColumnToAnalyze,
                    trainerOptions.TextColumns,
                    trainerOptions.NumericColumns,
                    DataKind.Single
                    );

                dataPrepTransformer = initializer.Fit(trainDataView);
                initializedData = dataPrepTransformer.Transform(trainDataView);

                var settings = new RegressionExperimentSettings
                {
                    MaxExperimentTimeInSeconds = (uint)trainerOptions.MaxAnalysisTimeInSeconds,
                    CancellationToken = trainerOptions.JobCancellationToken,
                    CacheBeforeTrainer = CacheBeforeTrainer.On
                };

                settings.Trainers.Clear();
                settings.Trainers.Add(RegressionTrainer.StochasticDualCoordinateAscent);
                settings.Trainers.Add(RegressionTrainer.OnlineGradientDescent);
                settings.Trainers.Add(RegressionTrainer.LbfgsPoissonRegression);
                settings.Trainers.Add(RegressionTrainer.Ols);

                experiment = mlContext.Auto().CreateRegressionExperiment(settings);
                result = experiment.Execute(initializedData);
                if (result.BestRun == null)
                {
                    return new TrainerResult(result.RunDetails.FirstOrDefault()?.Exception.Message);
                }

                model = result.BestRun.Model;
                weights = model.LastTransformer.Model.Weights;

                using (var dataPrepMs = new MemoryStream())
                using (var trainerMs = new MemoryStream())
                {
                    mlContext.Model.Save(dataPrepTransformer, trainDataView.Schema, dataPrepMs);
                    mlContext.Model.Save(result.BestRun.Model, initializedData.Schema, trainerMs);
                    return new TrainerResult(trainerMs.ToArray(), dataPrepMs.ToArray(), weights.ToArray(), JsonConvert.SerializeObject(new { result.BestRun.TrainerName, result.BestRun.ValidationMetrics }, Formatting.Indented));
                }
            }
            finally
            {
                mlContext = null;
                trainDataView = null;
                initializedData = null;
                initializer = null;
                experiment = null;
                model = null;
            }
        }
    }
}
