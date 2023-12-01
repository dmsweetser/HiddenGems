using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace HiddenGems.Common
{
    public static class Extensions
    {
        public static string GetDescription(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            ?.GetCustomAttribute<DescriptionAttribute>()
                            .Description ?? "";
        }
    }

    public enum AlgorithmType
    {
        [Display(Name = "Is it or isn't it?")]
        [Description("Is it or isn't it? [Binary Classification]")]
        BinaryClassification = 0,
        [Display(Name = "What will the value be?")]
        [Description("What will the value be? [Regression]")]
        Regression = 1
    }

    public enum ErrorCode
    {
        [Description("")]
        NoError,
        [Description("No work was done")]
        NoWorkDone,
        [Description("No valid results could be obtained")]
        NoValidResult,
        [Description("All cross validation folds have empty train or test data. Try increasing the number of rows provided in training data, or lowering specified number of cross validation folds.")]
        InsufficientData,
        [Description("Something went wrong!")]
        GenericError
    }
}
