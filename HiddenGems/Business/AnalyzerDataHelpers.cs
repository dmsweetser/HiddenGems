using HiddenGems.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using System.Globalization;
using System.Reflection;
using HiddenGems.Runtime;
using System;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System.Web;

namespace HiddenGems.Business
{
    public static partial class AnalyzerHelpers
    {
        public static async Task<Stream> RetrieveCsvFromUrl(string dataUrl)
        {
            try
            {
                var requestUrl = dataUrl;
                if (dataUrl.Contains("docs.google.com/spreadsheets"))
                {
                    var docId = dataUrl.Split("/")[5];
                    requestUrl = String.Format("https://docs.google.com/spreadsheets/d/{0}/gviz/tq?tqx=out:csv", docId);
                }
                var retrieveClient = new HttpClient();
                return await retrieveClient.GetStreamAsync(requestUrl);
            }
            catch
            {
                return new MemoryStream();
            }
        }

        public static string[] GetPropertyNamesForDataRecord(int labelColumnNumber, int numberOfColumns)
        {
            var propertyNames = new List<string>();
            for (int i = 1; i <= numberOfColumns; i++)
            {
                if (i == labelColumnNumber)
                {
                    continue;
                }
                propertyNames.Add($"Column{i}");
            }
            var result = propertyNames.ToArray();
            return result;
        }

        public static dynamic ProcessDataStreamFromCsv(Stream dataToAnalyze, out List<string> propertyNames)
        {
            var delimiter = ",";
            var config = GetConfig(delimiter);

            using var initialStreamReader = new StreamReader(dataToAnalyze);
            using var csv = new CsvReader(initialStreamReader, config);
            csv.Read();
            var rawRecord = Regex.Replace(csv.Parser.RawRecord, "(\"[^\",]+)(,)([^\"]+\")", "$1_$3");
            propertyNames = rawRecord.Split(',').ToList();
            if (propertyNames.Count == 1)
            {
                propertyNames = propertyNames[0].Split('\t').ToList();
            }
            var dataRecordType = DataRecordBuilder.BuildDataRecord(propertyNames.Count);
            dynamic result = typeof(AnalyzerHelpers).GetMethod("ExtractDataFromCsv", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(dataRecordType).Invoke(null, new object[] { csv });
            return result;
        }

        private static CsvConfiguration GetConfig(string delimiter)
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = (args) =>
                {
                    if (string.IsNullOrWhiteSpace(args.Header))
                    {
                        return args.FieldIndex.ToString();
                    }
                    return args.Header;
                },
                MissingFieldFound = null,
                BufferSize = 500000,
                HasHeaderRecord = false,
                DetectDelimiter = true
            };
        }

        private static dynamic ExtractDataFromCsv<T>(CsvReader csv)
        {
            if (LicenseManager.IsDemoMode())
            {
                return csv.GetRecords<T>().Take(SharedSettings.DemoModeRowLimit).ToList().Select(x => DataRecordBuilder.ScrubValues(x)).ToList();
            }
            else
            {
                return csv.GetRecords<T>().ToList().Select(x => DataRecordBuilder.ScrubValues(x)).ToList();
            }

        }

        public static List<T> ScrubTrainingData<T>(IEnumerable<T> trainData)
        {
            return trainData.Select(y => DataRecordBuilder.ScrubValues(y)).ToList();
        }

        public static void PopulateSampleRecord(JobRequest currentJobRequest, List<string> propertyNames, object existingRecord)
        {
            var existingRecordProperties = existingRecord.GetType().GetProperties();
            var sampleRecord = new Dictionary<string, string>();
            for (int i = 0; i < propertyNames.Count; i++)
            {
                var foundPropertyValue = existingRecordProperties?[i]?.GetValue(existingRecord);
                var currentPropertyName = propertyNames[i];
                if (sampleRecord.ContainsKey(currentPropertyName))
                {
                    currentPropertyName = $"{currentPropertyName}[{i}]";
                }
                else if (string.IsNullOrWhiteSpace(currentPropertyName))
                {
                    currentPropertyName = $"Column{i}";
                }
                sampleRecord.Add(currentPropertyName, foundPropertyValue?.ToString() ?? "");
            }
            currentJobRequest.Data.SerializedSampleRecord = JsonConvert.SerializeObject(sampleRecord);
        }

        public static void CategorizeColumns<T>(
            IEnumerable<T> trainData,
            List<string> originalColumnNames,
            int labelColumnNumber,
            JobRequest currentJobRequest,
            out List<Tuple<string, string>> eligibleColumns)
        {
            eligibleColumns = new List<Tuple<string, string>>();

            //Identifies numeric and text columns in the provided data
            var totalCount = trainData.Count();
            for (int i = 1; i <= originalColumnNames.Count; i++)
            {
                if (i == labelColumnNumber || originalColumnNames[i - 1] == "SYSTEM_SCORE")
                {
                    continue;
                }

                var binaryCount = trainData
                    .Where(x => TrainerOptions<T>.IsValidTrainingData(x, "Column" + i, AlgorithmType.BinaryClassification))
                    .Count();
                var regressionCount = trainData
                    .Where(x => TrainerOptions<T>.IsValidTrainingData(x, "Column" + i, AlgorithmType.Regression))
                    .Count();

                if (binaryCount > totalCount * .25 || regressionCount > totalCount * .25)
                {
                    eligibleColumns.Add(new Tuple<string, string>("Column" + i, originalColumnNames[i - 1]));
                }

                currentJobRequest.Status.UploadPercentCompleted = .5 + ((double)i / originalColumnNames.Count / 2);
            }
        }
    }
}
