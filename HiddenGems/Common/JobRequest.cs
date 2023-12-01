using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HiddenGems.Common
{
    public class JobRequest
    {
        public string JobKey { get; set; } = "";
        public string ReportIdentifier { get; set; }

        private JobData _data { get; set; }
        public JobData Data
        {
            get
            {
                if (_data == null)
                {
                    _data = new JobData(JobKey);
                }
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        private JobStatus _status { get; set; }
        public JobStatus Status
        {
            get
            {
                if (_status == null)
                {
                    _status = new JobStatus(JobKey);
                }
                _ = _status.PercentCompleted;
                return _status;
            }
            set
            {
                _status = value;
            }
        }

        public List<JobResult> Results { get; set; }
        public bool HasError => Results.Count > 0 && Results.All(x => x.HasError)
            || Results.Count == 0 && Status.Error != ErrorCode.NoError;
        public bool ResultExists => Results.Count > 0;

        public JobRequest(string jobKey)
        {
            JobKey = jobKey;
            Data = new JobData(jobKey);
            Status = new JobStatus(jobKey);
            Results = new List<JobResult>();
        }

        public void Reset()
        {
            Data = new JobData(JobKey);
            Status = new JobStatus(JobKey);
            Results = new List<JobResult>();
        }

        public Dictionary<string, float> GetAggregatedJobResults()
        {
            var weightCollection = new Dictionary<string, float>();
            foreach (var result in Results)
            {
                if (string.IsNullOrWhiteSpace(result.SerializedMetrics)) continue;
                float weightMultiplier = 1;
                var deserializedResults = JObject.Parse(result.SerializedMetrics);
                var areaUnderPrecisionRecallCurve = deserializedResults.Descendants()
                    .Where(x => x is JObject && x["AreaUnderPrecisionRecallCurve"] != null)
                    .Select(y => y["AreaUnderPrecisionRecallCurve"])
                    .FirstOrDefault();
                var rSquared = deserializedResults.Descendants()
                    .Where(x => x is JObject && x["RSquared"] != null)
                    .Select(y => y["RSquared"])
                    .FirstOrDefault();
                if (areaUnderPrecisionRecallCurve != null)
                {
                    weightMultiplier = areaUnderPrecisionRecallCurve.Value<float>();
                }
                else if (rSquared != null)
                {
                    weightMultiplier = rSquared.Value<float>();
                }

                if (weightMultiplier <= 0) continue;
                if (weightMultiplier > 1.8) weightMultiplier = 1.8F;

                try
                {
                    foreach (var providedColumn in result.OriginalColumnsWithWeights)
                    {
                        if (providedColumn.Item1 == result.LabelColumn)
                        {
                            continue;
                        }

                        if (weightCollection.ContainsKey(providedColumn.Item1))
                        {
                            weightCollection[providedColumn.Item1] += providedColumn.Item2 * weightMultiplier;
                        }
                        else if (!weightCollection.ContainsKey(providedColumn.Item1))
                        {
                            weightCollection.Add(providedColumn.Item1, providedColumn.Item2 * weightMultiplier);
                        }
                    }
                }
                catch (Exception)
                {
                    //Do nothing, but do not include the result
                }
            }

            return weightCollection;
        }
    }
}
