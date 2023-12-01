using HiddenGems.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HiddenGems.Business
{
    public class EvaluatorOptions<T>
    {
        public T RecordToEvaluate { get; set; }
        public AlgorithmType SelectedAlgorithm { get; set; }
        public string ColumnToAnalyze { get; set; }
        public byte[] ExistingDataPrepModel { get; set; } = Array.Empty<byte>();
        public byte[] ExistingTrainerModel { get; set; } = Array.Empty<byte>();

        public EvaluatorOptions(
            byte[] existingDataPrepModel,
            byte[] existingTrainerModel,
            AlgorithmType selectedAlgorithm,
            string columnToAnalyze,
            dynamic recordToEvaluate)
        {
            SelectedAlgorithm = selectedAlgorithm;
            ColumnToAnalyze = columnToAnalyze;
            ExistingDataPrepModel = existingDataPrepModel;
            ExistingTrainerModel = existingTrainerModel;
            RecordToEvaluate = recordToEvaluate;
        }
    }
}
