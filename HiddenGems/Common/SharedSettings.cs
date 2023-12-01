using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace HiddenGems.Common
{
    public static class SharedSettings
    {
        public static double EstimationMultiplier { get; set; }
        public static int IdleTimeout { get; set; }
        public static int MinimumEstimatedDuration { get; set; }
        public static int NumberOfIterations { get; set; }
        public static List<string> FillerValues { get; set; }
        public static int DemoModeRowLimit { get; set; }
        public static List<string> YesValues { get; set; }
        public static List<string> NoValues { get; set; }

        public static void PopulateSharedSettings(IConfiguration configuration)
        {
            EstimationMultiplier = configuration.GetValue("EstimationMultiplier", .5);
            NumberOfIterations = configuration.GetValue("NumberOfIterations", 1);
            if (LicenseManager.IsDemoMode())
            {
                IdleTimeout = configuration.GetValue("DemoModeIdleTimeout", 720);
            }
            else
            {
                IdleTimeout = configuration.GetValue("IdleTimeout", 720);
            }
            MinimumEstimatedDuration = configuration.GetValue("MinimumEstimatedDuration", 30);
            FillerValues = configuration.GetValue("FillerValues", "").Split("|").ToList();
            DemoModeRowLimit = configuration.GetValue("DemoModeRowLimit", 1000);
            YesValues = configuration.GetValue("YesValues", "").Split("|").Select(x => x.ToUpper()).ToList();
            NoValues = configuration.GetValue("NoValues", "").Split("|").Select(x => x.ToUpper()).ToList();
        }
    }
}
