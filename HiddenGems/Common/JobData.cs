using System;
using System.Collections.Generic;

namespace HiddenGems.Common
{
    public class JobData
    {
        public string ParentJobKey { get; set; }
        public dynamic DataToProcess { get; set; }
        public long DataLength { get; set; }
        public List<string> OriginalColumnNames { get; set; }
        public byte[] ExistingDataPrepModel { get; set; } = Array.Empty<byte>();
        public byte[] ExistingTrainerModel { get; set; } = Array.Empty<byte>();
        public List<Tuple<string, string>> EligibleColumns { get; set; }
        public Tuple<string, string> SelectedColumnToAnalyze { get; set; }
        public string SerializedSampleRecord { get; set; }
        public JobData(string parentJobKey)
        {
            ParentJobKey = parentJobKey;
        }
    }
}
