using HiddenGems.Common;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace HiddenGems.Business
{
    public static partial class AnalyzerHelpers
    {
        public static byte[] GenerateFiles(
            List<JobResult> reportResults
            )
        {
            //Taken in part from https://stackoverflow.com/questions/50513852/using-ziparchive-with-asp-net-core-web-api
            byte[] bytes = null;

            using (MemoryStream zipStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    if (reportResults.Count == 0)
                    {
                        return Array.Empty<byte>();
                    }

                    var summaryBuilder = new StringBuilder();

                    foreach (var result in reportResults)
                    {
                        string algorithmName = "";
                        switch (result.SelectedAlgorithm)
                        {
                            case AlgorithmType.BinaryClassification:
                                algorithmName = "_BinaryClassification_";
                                break;
                            case AlgorithmType.Regression:
                                algorithmName = "_Regression_";
                                break;
                            default:
                                break;
                        }

                        var scrubbedColumnName = result.LabelColumn;
                        if (scrubbedColumnName != null)
                        {
                            foreach (char c in Path.GetInvalidFileNameChars())
                            {
                                scrubbedColumnName = scrubbedColumnName.Replace("" + c, string.Empty);
                            }
                        }

                        if (result.GeneratedDataPrepModel.Length > 0 && result.GeneratedTrainerModel.Length > 0)
                        {
                            ZipArchiveEntry modelEntry = archive.CreateEntry(scrubbedColumnName + algorithmName + "GeneratedModel.hdgr", CompressionLevel.Fastest);
                            using (Stream stream = modelEntry.Open())
                            {
                                var generatedModelBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                                stream.Write(generatedModelBytes);
                            }
                        }

                        summaryBuilder.Append(result.GetSummary());
                    }
                    
                    ZipArchiveEntry summaryEntry = archive.CreateEntry("Summary.txt", CompressionLevel.Fastest);
                    using (var originalFileMemoryStream = new MemoryStream())
                    using (Stream stream = summaryEntry.Open())
                    {
                        stream.Write(Encoding.ASCII.GetBytes(summaryBuilder.ToString()));
                    }
                }
                zipStream.Position = 0;
                bytes = zipStream.ToArray();
            }
            return bytes;
        }
    }
}
