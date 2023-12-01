using HiddenGems.Common;
using System.Collections.Generic;

namespace HiddenGems.Business
{
    public class EvaluatorResult
    {
        public AlgorithmType SelectedAlgorithm { get; set; }
        public string EvaluatedValue { get; set; } = "";
        public float EvaluatedScore { get; set; } = 0;
        public string ErrorMessage { get; set; } = "";

        public EvaluatorResult(AlgorithmType selectedAlgorithm, string evaluatedValue, float evaluatedScore)
        {
            SelectedAlgorithm = selectedAlgorithm;
            EvaluatedValue = evaluatedValue;
            EvaluatedScore = evaluatedScore;
            ErrorMessage = "";
        }

        public EvaluatorResult(AlgorithmType selectedAlgorithm, string evaluatedValue, float evaluatedScore, string errorMessage)
        {
            SelectedAlgorithm = selectedAlgorithm;
            EvaluatedValue = evaluatedValue;
            EvaluatedScore = evaluatedScore;
            ErrorMessage = errorMessage;
        }

        public EvaluatorResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
