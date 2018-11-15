using SystemRegex=System.Text.RegularExpressions.Regex;
namespace Mejram
{
    public class Regex
    {
        public static string ReplaceColumnPrefix(string text)
        {
            return SystemRegex.Replace(text, @"^[a-zA-Z]{2,4}_", "");
        }
    }
}