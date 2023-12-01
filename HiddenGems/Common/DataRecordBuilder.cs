using HiddenGems.Runtime;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace HiddenGems.Common
{
    public static class DataRecordBuilder
    {
        public static string FillerValue = " ";
        public static Type BuildDataRecord(int numberOfColumns)
        {
            var scriptTextBuilder = new StringBuilder();
            scriptTextBuilder.Append("public class DataRecord : HiddenGems.Runtime.IHgDataRecord {");
            for (int i = 1; i <= numberOfColumns; i++)
            {
                scriptTextBuilder.Append("[Order] public string Column" + i + $" {{ get; set; }}");
            }
            scriptTextBuilder.Append($"[Order] public string SYSTEM_SCORE {{ get; set; }}");

            scriptTextBuilder.Append("public string ConvertToCsvRecord() { var recordToReturn = \"\"; ");
            for (int i = 1; i <= numberOfColumns; i++)
            {
                scriptTextBuilder.Append("if (Column" + i + " != \"\") recordToReturn += \"\\\"\" + Column" + i + " + \"\\\",\"; ");
            }
            scriptTextBuilder.Append("if (SYSTEM_SCORE != \"\") recordToReturn += \"\\\"\" + SYSTEM_SCORE + \"\\\",\"; ");
            scriptTextBuilder.Append("return recordToReturn; }");
            scriptTextBuilder.Append("} return typeof(DataRecord);");
            var script = CSharpScript.Create(
                scriptTextBuilder.ToString(),
                ScriptOptions.Default
                .WithReferences(Assembly.GetAssembly(typeof(IHgDataRecord)))
                .WithImports("HiddenGems.Runtime")
                .WithOptimizationLevel(Microsoft.CodeAnalysis.OptimizationLevel.Release)
                );
            script.Compile();
            return (Type)script.RunAsync().Result.ReturnValue;
        }

        public static T ScrubValues<T>(T record)
        {
            var recordProperties = typeof(T).GetProperties().Where(x => x.Name.StartsWith("Column"));
            foreach (var recordProperty in recordProperties)
            {
                var currentValue = recordProperty.GetValue(record).ToString();
                if (string.IsNullOrWhiteSpace(currentValue) || SharedSettings.FillerValues.Contains(currentValue))
                {
                    recordProperty.SetValue(record, FillerValue);
                }
                else
                {
                    var scrubbedValue = Regex.Replace(currentValue, "[^a-zA-Z0-9.,-]", "");
                    if (SharedSettings.YesValues.Contains(scrubbedValue.ToUpper()))
                    {
                        scrubbedValue = "True";
                    }
                    else if (SharedSettings.NoValues.Contains(scrubbedValue.ToUpper()))
                    {
                        scrubbedValue = "False";
                    }
                    recordProperty.SetValue(record, scrubbedValue);
                }
            }
            return record;
        }
    }
}
