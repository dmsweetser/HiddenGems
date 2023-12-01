using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HiddenGems.Common
{
    public class JobResultDTO
    {
        public KeyValuePair<string,float>[] ColumnWeights { get; set; }
        public string RawResult { get; set; }
        public string SerializedSampleRecord { get; set; }

        public JobResultDTO()
        {
            ColumnWeights = Array.Empty<KeyValuePair<string, float>>();
            RawResult = "";
            SerializedSampleRecord = "";
        }
    }
}
