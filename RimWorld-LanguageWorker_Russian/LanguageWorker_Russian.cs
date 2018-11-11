using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Verse;

namespace LanguageWorkerRussian_Test
{
	public class LanguageWorker_Russian : LanguageWorker
	{
		private interface IResolver
		{
			string Resolve(string arguments);
		}

		private class ReplaceResolver : IResolver
		{
			// ^Replace "{0}": "Мартомай"-"Мартомая", "Июгуст"-"Июгуста", "Сентоноябрь"-"Сентоноября", "Декавраль"-"Декавраля"^
			private static readonly Regex _replacementArgumentsLineRegex = new Regex("^'(?<input>[^']*?)':\\s*('(?<old>[^']*?)'-'(?<new>[^']*?)')(,\\s*'(?<old>[^']*?)'-'(?<new>[^']*?)')*$", RegexOptions.Compiled);

			public string Resolve(string argumentsLine)
			{
				Match match = _replacementArgumentsLineRegex.Match(argumentsLine);
				if (!match.Success)
				{
					throw new ApplicationException(string.Format("Syntax error in LW resolver arguments: \"{0}\"", argumentsLine));
					//Log.Error(string.Format("Syntax error in LW resolver arguments: \"{0}\"", argumentsLine));
					//return argumentsLine;
				}

				string input = match.Groups["input"].Value;

				Group oldGroup = match.Groups["old"];
				Group newGroup = match.Groups["new"];

				if (oldGroup.Captures.Count != newGroup.Captures.Count)
				{
					throw new ApplicationException(string.Format("Syntax error in LW resolver arguments: \"{0}\"", argumentsLine));
					//Log.Error(string.Format("Syntax error in LW resolver arguments: \"{0}\"", argumentsLine));
					//return input;
				}

				for (int i = 0; i < oldGroup.Captures.Count; ++i)
				{
					input = input.Replace(oldGroup.Captures[i].Value, newGroup.Captures[i].Value);
				}

				return input;
			}
		}

		private class NumberCaseResolver : IResolver
		{
			// "3.14": 1-"прошёл # день" 2-"прошло # дня" X-"прошло # дней"
			private static readonly Regex _numberCaseArgumentsLineRegex = new Regex("^'(?<number>(?<floor>[0-9]+)(.(?<frac>[0-9]+))?)':\\s*1-'(?<one>[^']*?)'\\s*2-'(?<several>[^']*?)'\\s*X-'(?<many>[^']*?)'$", RegexOptions.Compiled);

			public string Resolve(string argumentsLine)
			{
				Match match = _numberCaseArgumentsLineRegex.Match(argumentsLine);
				if (!match.Success)
				{
					throw new ApplicationException(string.Format("Syntax error in LW resolver arguments: \"{0}\"", argumentsLine));
				}

				bool hasFracPart = match.Groups["frac"].Success;

				string numberStr = match.Groups["number"].Value;
				string floorStr = match.Groups["floor"].Value;
				string formOne = match.Groups["one"].Value;
				string formSeveral = match.Groups["several"].Value;
				string formMany = match.Groups["many"].Value;

				if (hasFracPart)
				{
					return formSeveral.Replace("#", numberStr);
				}

				int floor = int.Parse(floorStr);
				return GetFormForNumber(floor, formOne, formSeveral, formMany).Replace("#", numberStr);
			}

			private static string GetFormForNumber(int number, string formOne, string formSeveral, string formMany)
			{
				int firstPos = number % 10;
				int secondPos = number / 10 % 10;

				if (secondPos == 1)
				{
					return formMany;
				}

				switch (firstPos)
				{
					case 1:
						return formOne;
					case 2:
					case 3:
					case 4:
						return formSeveral;
					default:
						return formMany;
				}
			}
		}

		private static readonly ReplaceResolver replaceResolver = new ReplaceResolver();
		private static readonly NumberCaseResolver numberCaseResolver = new NumberCaseResolver();

		private static readonly Regex _languageWorkerResolverRegex = new Regex("\\^(?<keyword>[a-zA-Z]+)\\s+(?<arguments>.*?)\\^", RegexOptions.Compiled);

		public override string PostProcessedKeyedTranslation(string translation)
		{
			translation = base.PostProcessedKeyedTranslation(translation);
			return PostProcess(translation);
		}

		public override string PostProcessed(string str)
		{
			str = base.PostProcessed(str);
			return PostProcess(str);
		}

		private static string PostProcess(string translation)
		{
			return _languageWorkerResolverRegex.Replace(translation, EvaluateResolver);
		}

		private static string EvaluateResolver(Match match)
		{
			string keyword = match.Groups["keyword"].Value;
			string arguments = match.Groups["arguments"].Value.Trim();
			IResolver resolver = GetResolverByKeyword(keyword);

			try
			{
				return resolver.Resolve(arguments);
			}
			catch (Exception ex)
			{
				// Logging
				return match.Value;
			}
		}

		private static IResolver GetResolverByKeyword(string keyword)
		{
			switch (keyword)
			{
				case "Replace":
					return replaceResolver;
				case "Number":
					return numberCaseResolver;
				default:
					return null;
			}
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

		private static bool IsConsonant(char ch)
		{
			return "бвгджзклмнпрстфхцчшщБВГДЖЗКЛМНПРСТФХЦЧШЩ".IndexOf(ch) >= 0;
		}
	}

}