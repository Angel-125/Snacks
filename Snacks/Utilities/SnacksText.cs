using System.Globalization;
using KSP.Localization;

namespace Snacks
{
    // Localize presentation only. Resource/condition/option names remain save identifiers.
    public static class SnacksText
    {
        public static string Get(string tag, string fallback)
        {
            string value = Localizer.Format(tag);
            return string.IsNullOrEmpty(value) || value == tag ? fallback : value.Replace("\\n", "\n").Replace("\\r", "\r");
        }

        public static string Format(string tag, string fallback, params object[] arguments)
        {
            return string.Format(CultureInfo.CurrentCulture, Get(tag, fallback), arguments);
        }

        public static string DisplayConditions(string summary)
        {
            if (string.IsNullOrEmpty(summary)) return summary;
            string[] conditions = summary.Split(',');
            for (int i = 0; i < conditions.Length; i++)
            {
                switch (conditions[i].Trim())
                {
                    case "Stressed Out": conditions[i] = Get("#LOC_Snacks_INFO_STRESSOUTNAME", "Stressed Out"); break;
                    case "Relaxing": conditions[i] = Get("#LOC_Snacks_INFO_RELAXNAME", "Relaxing"); break;
                    case "Sick": conditions[i] = Get("#LOC_Snacks_INFO_SICKNAME", "Sick"); break;
                }
            }
            return string.Join(", ", conditions);
        }
    }
}
