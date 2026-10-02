using System;

namespace Snacks
{
    // Read the active formatter each time: calendar mods/settings may replace it.
    public static class SnacksTime
    {
        private static double Valid(double value, double fallback)
        {
            return value > 0 && !double.IsInfinity(value) && !double.IsNaN(value) ? value : fallback;
        }

        public static double Minute { get { return KSPUtil.dateTimeFormatter == null ? 60 : Valid(KSPUtil.dateTimeFormatter.Minute, 60); } }
        public static double Hour { get { return KSPUtil.dateTimeFormatter == null ? 3600 : Valid(KSPUtil.dateTimeFormatter.Hour, 3600); } }
        public static double Day { get { double fallback = GameSettings.KERBIN_TIME ? 21600 : 86400; return KSPUtil.dateTimeFormatter == null ? fallback : Valid(KSPUtil.dateTimeFormatter.Day, fallback); } }
        public static double Year { get { double fallback = Day * (GameSettings.KERBIN_TIME ? 426.08 : 365); return KSPUtil.dateTimeFormatter == null ? fallback : Valid(KSPUtil.dateTimeFormatter.Year, fallback); } }
    }
}
