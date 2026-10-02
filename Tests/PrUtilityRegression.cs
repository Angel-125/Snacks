using System;
using Snacks;
using KSP.Localization;

public static class GameSettings { public static bool KERBIN_TIME = true; }
public class Calendar { public double Minute = 60, Hour = 3600, Day = 21600, Year = 9203545; }
public static class KSPUtil { public static Calendar dateTimeFormatter; }

class PrUtilityRegression
{
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Main()
    {
        Check(SnacksTime.Day == 21600, "Null formatter uses Kerbin fallback");
        GameSettings.KERBIN_TIME = false;
        Check(SnacksTime.Day == 86400, "Fallback responds to settings changes");
        KSPUtil.dateTimeFormatter = new Calendar { Minute = 90, Hour = 5400, Day = 43200, Year = 4320000 };
        Check(SnacksTime.Day == 43200 && SnacksTime.Hour == 5400 && SnacksTime.Minute == 90 && SnacksTime.Year == 4320000, "Read custom calendar units");
        KSPUtil.dateTimeFormatter = new Calendar { Day = 108000 };
        Check(SnacksTime.Day == 108000, "Replacing formatter refreshes day length");
        KSPUtil.dateTimeFormatter.Day = double.NaN;
        Check(SnacksTime.Day == 86400, "Invalid formatter values have finite fallback");
        Check(SnacksText.Get("#missing", "English") == "English", "Missing localization uses fallback");
        Localizer.Strings["#test"] = "value {0:N2}\\nnext";
        Check(SnacksText.Get("#test", "").Contains("{0:N2}"), "Deferred numeric formatting is preserved");
        Check(SnacksText.Format("#test", "", 123.5).Contains("\nnext"), "Config newline and numeric formatting");
        Localizer.Strings["#LOC_Snacks_INFO_STRESSOUTNAME"] = "压力过大";
        var canonical = "Stressed Out, Relaxing";
        Check(SnacksText.DisplayConditions(canonical).StartsWith("压力过大"), "Localize condition presentation");
        Check(canonical == "Stressed Out, Relaxing", "Presentation does not mutate saved identifiers");
        Check(SnacksText.DisplayConditions("Custom condition") == "Custom condition", "Unknown addon conditions retain display");
        Console.WriteLine("PASS: dynamic calendar/fallbacks, localization fallback/formatting, and canonical condition preservation.");
    }
}
