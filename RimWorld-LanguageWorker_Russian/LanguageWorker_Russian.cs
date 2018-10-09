using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Verse;

namespace LanguageWorkerRussian_Test
{
    public class LanguageWorker_Russian : LanguageWorker
    {
        private static readonly Regex _languageWorkerTagRegex = new Regex(@"\$(.*?)\$", RegexOptions.Compiled);

        private static readonly Regex _numYearsRegex = new Regex("([0-9]+) лет", RegexOptions.Compiled);
        private static readonly Regex _numQuadrumsRegex = new Regex("([0-9]+) кварталов", RegexOptions.Compiled);
        private static readonly Regex _numDaysRegex = new Regex("([0-9]+) дней", RegexOptions.Compiled);
        private static readonly Regex _numTimesRegex = new Regex("([0-9]+) раз", RegexOptions.Compiled);

        private static readonly Regex _passedDaysRegex = new Regex("Прошло ([0-9]+)", RegexOptions.Compiled);

        public override string PostProcessedKeyedTranslation(string translation)
        {
            translation = base.PostProcessedKeyedTranslation(translation);
            string oldTranslation = translation;

            List<string> tags = new List<string>();
            translation = _languageWorkerTagRegex.Replace(translation, (match) => EvaluateTags(match, tags));

            foreach (string tag in tags)
            {
                switch (tag)
                {
                    case "date":
                        translation = translation
                            .Replace("Мартомай", "Мартомая")
                            .Replace("Июгуст", "Июгуста")
                            .Replace("Сентоноябрь", "Сентоноября")
                            .Replace("Декавраль", "Декавраля");
                        break;
                    case "XItems1":
                        translation = _numYearsRegex.Replace(translation, (match) => EvaluateCasedItem(match, "лет", "год", "года"));
                        translation = _numQuadrumsRegex.Replace(translation, (match) => EvaluateCasedItem(match, "кварталов", "квартал", "квартала"));
                        translation = _numDaysRegex.Replace(translation, (match) => EvaluateCasedItem(match, "дней", "день", "дня"));
                        translation = _numTimesRegex.Replace(translation, (match) => EvaluateCasedItem(match, "раз", "раз", "раза"));
                        break;
                    case "XItems2":
                        translation = _numYearsRegex.Replace(translation, (match) => EvaluateCasedItem(match, "лет", "года", "лет"));
                        translation = _numQuadrumsRegex.Replace(translation, (match) => EvaluateCasedItem(match, "кварталов", "квартала", "кварталов"));
                        translation = _numDaysRegex.Replace(translation, (match) => EvaluateCasedItem(match, "дней", "дня", "дней"));
                        translation = _numTimesRegex.Replace(translation, (match) => EvaluateCasedItem(match, "раз", "раза", "раз"));
                        break;
                    case "passed":
                        translation = _passedDaysRegex.Replace(translation, (match) => EvaluateCasedItem(match, "Прошло", "Прошёл", "Прошло"));
                        break;
                    default:
                        Log.Warning(string.Format("Unexpected LanguageWorker_Russian tag: {0}", tag));
                        break;
                }
            }

            return translation;
        }

        public override string Pluralize(string str, Gender gender, int count = -1)
        {
            if (str.NullOrEmpty())
            {
                return str;
            }
            char c = str[str.Length - 1];
            char c2 = (str.Length < 2) ? '\0' : str[str.Length - 2];
            if (gender != Gender.Male)
            {
                if (gender != Gender.Female)
                {
                    if (gender == Gender.None)
                    {
                        if (c == 'o')
                        {
                            return str.Substring(0, str.Length - 1) + 'a';
                        }
                        if (c == 'O')
                        {
                            return str.Substring(0, str.Length - 1) + 'A';
                        }
                        if (c == 'e' || c == 'E')
                        {
                            char value = char.ToUpper(c2);
                            if ("ГКХЖЧШЩЦ".IndexOf(value) >= 0)
                            {
                                if (c == 'e')
                                {
                                    return str.Substring(0, str.Length - 1) + 'a';
                                }
                                if (c == 'E')
                                {
                                    return str.Substring(0, str.Length - 1) + 'A';
                                }
                            }
                            else
                            {
                                if (c == 'e')
                                {
                                    return str.Substring(0, str.Length - 1) + 'я';
                                }
                                if (c == 'E')
                                {
                                    return str.Substring(0, str.Length - 1) + 'Я';
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (c == 'я')
                    {
                        return str.Substring(0, str.Length - 1) + 'и';
                    }
                    if (c == 'ь')
                    {
                        return str.Substring(0, str.Length - 1) + 'и';
                    }
                    if (c == 'Я')
                    {
                        return str.Substring(0, str.Length - 1) + 'И';
                    }
                    if (c == 'Ь')
                    {
                        return str.Substring(0, str.Length - 1) + 'И';
                    }
                    if (c == 'a' || c == 'A')
                    {
                        char value2 = char.ToUpper(c2);
                        if ("ГКХЖЧШЩ".IndexOf(value2) >= 0)
                        {
                            if (c == 'a')
                            {
                                return str.Substring(0, str.Length - 1) + 'и';
                            }
                            return str.Substring(0, str.Length - 1) + 'И';
                        }
                        else
                        {
                            if (c == 'a')
                            {
                                return str.Substring(0, str.Length - 1) + 'ы';
                            }
                            return str.Substring(0, str.Length - 1) + 'Ы';
                        }
                    }
                }
            }
            else
            {
                if (IsConsonant(c))
                {
                    return str + 'ы';
                }
                if (c == 'й')
                {
                    return str.Substring(0, str.Length - 1) + 'и';
                }
                if (c == 'ь')
                {
                    return str.Substring(0, str.Length - 1) + 'и';
                }
                if (c == 'Й')
                {
                    return str.Substring(0, str.Length - 1) + 'И';
                }
                if (c == 'Ь')
                {
                    return str.Substring(0, str.Length - 1) + 'И';
                }
            }
            return str;
        }

        private static string EvaluateTags(Match match, List<string> tags)
        {
            if (match.Groups.Count <= 1)
                return string.Empty;

            string tagLine = match.Groups[1].Value;

            foreach (string tag in tagLine.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                tags.AddDistinct(tag);
            }
            return string.Empty;
        }

        private static string EvaluateCasedItem(Match match, string caseDefault, string case1, string case2)
        {
            int number;
            if (!TryParseNumber(match, out number))
            {
                Log.Warning(string.Format("{0} doesn't have a number", match.Value));
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

        private static string ProcessDate(string str)
        {
            return str
                .Replace("Мартомай", "Мартомая")
                .Replace("Июгуст", "Июгуста")
                .Replace("Сентоноябрь", "Сентоноября")
                .Replace("Декавраль", "Декавраля");
        }

        private static bool IsConsonant(char ch)
        {
            return "бвгджзклмнпрстфхцчшщБВГДЖЗКЛМНПРСТФХЦЧШЩ".IndexOf(ch) >= 0;
        }
    }

}