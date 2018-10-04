using System.Text.RegularExpressions;

namespace LanguageWorkerRussian_Test
{
	internal static class StringReplaceExt
	{
		public static string ProcessTimeSpan(this string str)
		{
			return str
				.ReplaceAtStartSpaceOrNewline("1 лет", "1 год")
				.ReplaceAtStartSpaceOrNewline("1 кварталов", "1 квартал")
				.ReplaceAtStartSpaceOrNewline("1 дней", "1 день")
				.ReplaceAtStartSpaceOrNewline("1 часов", "1 час")
				.ReplaceRegex("([02-9])1 лет", "${1}1 год")
				.ReplaceRegex("([02-9])1 кварталов", "${1}1 квартал")
				.ReplaceRegex("([02-9])1 дней", "${1}1 день")
				.ReplaceRegex("([02-9])1 часов", "${1}1 час")

				.ReplaceAtStartSpaceOrNewline("2 лет", "2 года")
				.ReplaceAtStartSpaceOrNewline("2 кварталов", "2 квартала")
				.ReplaceAtStartSpaceOrNewline("2 дней", "2 дня")
				.ReplaceAtStartSpaceOrNewline("2 часов", "2 часа")
				.ReplaceRegex("([02-9])2 лет", "${1}2 года")
				.ReplaceRegex("([02-9])2 кварталов", "${1}2 квартала")
				.ReplaceRegex("([02-9])2 дней", "${1}2 дня")
				.ReplaceRegex("([02-9])2 часов", "${1}2 часа")

				.ReplaceAtStartSpaceOrNewline("3 лет", "3 года")
				.ReplaceAtStartSpaceOrNewline("3 кварталов", "3 квартала")
				.ReplaceAtStartSpaceOrNewline("3 дней", "3 дня")
				.ReplaceAtStartSpaceOrNewline("3 часов", "3 часа")
				.ReplaceRegex("([02-9])3 лет", "${1}3 года")
				.ReplaceRegex("([02-9])3 кварталов", "${1}3 квартала")
				.ReplaceRegex("([02-9])3 дней", "${1}3 дня")
				.ReplaceRegex("([02-9])3 часов", "${1}3 часа")

				.ReplaceAtStartSpaceOrNewline("4 лет", "4 года")
				.ReplaceAtStartSpaceOrNewline("4 кварталов", "4 квартала")
				.ReplaceAtStartSpaceOrNewline("4 дней", "4 дня")
				.ReplaceAtStartSpaceOrNewline("4 часов", "4 часа")
				.ReplaceRegex("([02-9])4 лет", "${1}4 года")
				.ReplaceRegex("([02-9])4 кварталов", "${1}4 квартала")
				.ReplaceRegex("([02-9])4 дней", "${1}4 дня")
				.ReplaceRegex("([02-9])4 часов", "${1}4 часа");
		}

		public static string ProcessDate(this string str)
		{
			return str
				.Replace("-е мартомай", "-е мартомая")
				.Replace("-е июгуст", "-е июгуста")
				.Replace("-е сентоноябрь", "-е сентоноября")
				.Replace("-е декавраль", "-е декавраля");
		}

		public static string ReplaceAtIndex(this string str, int index, int length, char replacement)
		{
			return str.ReplaceAtIndex(index, length, replacement.ToString());
		}

		public static string ReplaceAtIndex(this string str, int index, int length, string replacement)
		{
			return str.Remove(index, length).Insert(index, replacement);
		}

		public static string ReplaceRegex(this string str, string pattern, string replaceWith)
		{
			return Regex.Replace(str, pattern, replaceWith);
		}

		public static string ReplaceAtStartSpaceOrNewline(this string str, string toReplace, string replaceWith)
		{
			if (!str.Contains(toReplace))
				return str;

			str = str
				.Replace(' ' + toReplace, ' ' + replaceWith)
				.Replace('\n' + toReplace, '\n' + replaceWith);

			if (str.StartsWith(toReplace))
				str = replaceWith + str.Substring(toReplace.Length);

			return str;
		}
	}

}