using System.Collections.Generic;
namespace KSP.Localization
{
    public static class Localizer
    {
        public static Dictionary<string, string> Strings = new Dictionary<string, string>();
        public static string Format(string tag)
        {
            string value;
            return Strings.TryGetValue(tag, out value) ? value : tag;
        }
    }
}
