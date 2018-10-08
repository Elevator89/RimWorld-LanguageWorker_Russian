using System.Text.RegularExpressions;

namespace LanguageWorkerRussian_Test
{
    internal static class StringReplaceExt
    {
        private static readonly Regex numYears = new Regex("[0-9]+ лет", RegexOptions.Compiled);
        private static readonly Regex numQuadrums = new Regex("[0-9]+ кварталов", RegexOptions.Compiled);
        private static readonly Regex numDays = new Regex("[0-9]+ дней", RegexOptions.Compiled);
        private static readonly Regex numTimes = new Regex("[0-9]+ раз", RegexOptions.Compiled);

        public static string ProcessTimeSpan(this string str)
        {
            MatchCollection matches = numYears.Matches(str);

            str = numYears.Replace(str, (match) => EvaluateCasedItem(match, "лет", "год", "года"));
            str = numQuadrums.Replace(str, (match) => EvaluateCasedItem(match, "кварталов", "квартал", "квартала"));
            str = numDays.Replace(str, (match) => EvaluateCasedItem(match, "дней", "день", "дня"));
            str = numTimes.Replace(str, (match) => EvaluateCasedItem(match, "раз", "раз", "раза"));

            return str;
        }

        private static string EvaluateCasedItem(Match match, string caseDefault, string case1, string case2)
        {
            int number;
            if (!TryParseNumber(match, out number))
            {
                Log.WarningFormat("{0} doesn't have a number", match.Value);
                return match.Value;
            }

            return match.Value.Replace(caseDefault, GetCasedItem(number, caseDefault, case1, case2));
        }

        private static bool TryParseNumber(Match match, out int number)
        {
            number = int.MinValue;

            if (match.Groups.Count <= 1)
                return false;

            string intStr = match.Groups[1].Value;
            return int.TryParse(intStr, out number);
        }

        private static string GetCasedItem(int number, string caseDefault, string case1, string case2)
        {
            switch (GetNumberCase(number))
            {
                case 1:
                    return case1;
                case 2:
                    return case2;
                default:
                    return caseDefault;
            }
        }

        private static int GetNumberCase(int number)
        {
            int firstPos = number % 10;
            int secondPos = number / 10 % 10;

            if (secondPos == 1)
            {
                return 0;
            }

            switch (firstPos)
            {
                case 1:
                    return 1;
                case 2:
                case 3:
                case 4:
                    return 2;
                default:
                    return 0;
            }
        }

        public static string ProcessDate(this string str)
        {
            return str
                .Replace("Мартомай", "Мартомая")
                .Replace("Июгуст", "Июгуста")
                .Replace("Сентоноябрь", "Сентоноября")
                .Replace("Декавраль", "Декавраля");
        }
    }

}