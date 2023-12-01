using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HiddenGems.Business
{
    public static class AnalyzerPreFeaturizer
    {
        public static dynamic GetPreFeaturizer(MLContext mlContext, string labelColumn, List<string> textColumns, List<string> numericColumns, DataKind dataKind)
        {
            return PreFeaturizerV1(mlContext, labelColumn, textColumns, numericColumns, dataKind);
        }

        private static dynamic PreFeaturizerV1(MLContext mlContext, string labelColumn, List<string> textColumns, List<string> numericColumns, DataKind dataKind)
        {
            var numericColumnPairs = numericColumns.Select(x => new InputOutputColumnPair("NUM" + x, x)).ToArray();

            if (textColumns.Count > 0 && numericColumnPairs.Length > 0)
            {
                var options = new TextFeaturizingEstimator.Options();
                var combinedColumns = numericColumnPairs.Select(x => x.OutputColumnName).Union(new List<string>() { "TextFeatures" }).ToArray();

                var initializer = mlContext.Transforms.Conversion.ConvertType("Label", labelColumn, dataKind)
                .Append(mlContext.Transforms.Text.FeaturizeText("TextFeatures", options, textColumns.ToArray()))
                .Append(mlContext.Transforms.Conversion.ConvertType(numericColumnPairs))
                .Append(mlContext.Transforms.Concatenate("Features", combinedColumns))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(mlContext.Transforms.SelectColumns(new string[] { "Label", "Features" }));
                return initializer;
            }
            else if (numericColumnPairs.Length > 0)
            {
                var initializer = mlContext.Transforms.Conversion.ConvertType("Label", labelColumn, dataKind)
                .Append(mlContext.Transforms.Conversion.ConvertType(numericColumnPairs))
                .Append(mlContext.Transforms.Concatenate("Features", numericColumnPairs.Select(x => x.OutputColumnName).ToArray()))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(mlContext.Transforms.SelectColumns(new string[] { "Label", "Features" }));
                return initializer;
            }
            else
            {
                throw new ArgumentException("No valid data was found to process");
            }
        }
        private static dynamic PreFeaturizerV0(MLContext mlContext, int labelColumnNumber, List<string> textColumns, List<string> numericColumns, DataKind dataKind)
        {
            var combinedColumns = textColumns.ToList();
            combinedColumns.AddRange(numericColumns);
            var optionsV0 = new TextFeaturizingEstimator.Options
            {
                KeepNumbers = true,
                WordFeatureExtractor = null,
                CharFeatureExtractor = null
            };

            var initializerV0 = mlContext.Transforms.Conversion.ConvertType("Label", "Column" + labelColumnNumber, dataKind)
                .Append(mlContext.Transforms.Text.FeaturizeText("Features", optionsV0, combinedColumns.ToArray()))
                .AppendCacheCheckpoint(mlContext);
            return initializerV0;
        }
    }
}
