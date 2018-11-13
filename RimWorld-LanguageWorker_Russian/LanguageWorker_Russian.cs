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
			string Resolve(string[] arguments);
		}

		private class ReplaceResolver : IResolver
		{
			// ^Replace('{0}', 'Мартомай'-'Мартомая', 'Июгуст'-'Июгуста', 'Сентоноябрь'-'Сентоноября', 'Декавраль'-'Декавраля')^
			private static readonly Regex _argumentRegex = new Regex(@"'(?<old>[^']*?)'-'(?<new>[^']*?)'", RegexOptions.Compiled);

			public string Resolve(string[] arguments)
			{
				if(arguments.Length == 0)
				{
					throw new ApplicationException("No args found for ReplaceResolver");
				}

				string input = arguments[0];

				if (arguments.Length == 1)
				{
					return input;
				}

				for (int i = 1; i < arguments.Length; ++i)
				{
					string argument = arguments[i];

					Match match = _argumentRegex.Match(argument);
					if (!match.Success)
					{
						throw new ApplicationException(string.Format("Syntax error in ReplaceResolver argument: \"{0}\"", argument));
					}

					string oldValue = match.Groups["old"].Value;
					string newValue = match.Groups["new"].Value;

					if(oldValue == input)
					{
						return newValue;
					}
					//Log.Message(string.Format("input: {0}, old: {1}, new: {2}", input, oldGroup.Captures[i].Value, newGroup.Captures[i].Value));
				}

				return input;
			}
		}

		private class NumberCaseResolver : IResolver
		{
			// '3.14': 1-'прошёл # день', 2-'прошло # дня', X-'прошло # дней'
			private static readonly Regex _numberRegex = new Regex(@"(?<floor>[0-9]+)(\.(?<frac>[0-9]+))?", RegexOptions.Compiled);

			public string Resolve(string[] arguments)
			{
				if (arguments.Length != 4)
				{
					throw new ApplicationException("Incorrect number of arguments found for ReplaceResolver");
				}

				string numberStr = arguments[0];
				Match numberMatch = _numberRegex.Match(numberStr);
				if (!numberMatch.Success)
				{
					throw new ApplicationException(string.Format("Syntax error in NumberCaseResolver argument: \"{0}\"", numberStr));
				}

				bool hasFracPart = numberMatch.Groups["frac"].Success;

				string floorStr = numberMatch.Groups["floor"].Value;

				string formOne = arguments[1].Trim('\'');
				string formSeveral = arguments[2].Trim('\'');
				string formMany = arguments[3].Trim('\'');

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

		private static readonly Regex _languageWorkerResolverRegex = new Regex(@"\^(?<resolverName>\w+)\(\s*(?<argument>[^|]+?)\s*(\|\s*(?<argument>[^|]+?)\s*)*\)\^", RegexOptions.Compiled);

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
			string keyword = match.Groups["resolverName"].Value;

			Group argumentsGroup = match.Groups["argument"];

			string[] arguments = new string[argumentsGroup.Captures.Count];
			for(int i = 0; i < argumentsGroup.Captures.Count; ++i)
			{
				arguments[i] = argumentsGroup.Captures[i].Value.Trim();
			}

			IResolver resolver = GetResolverByKeyword(keyword);

			try
			{
				return resolver.Resolve(arguments);
			}
			catch
			{
				//Log.Error(string.Format("Error happened while resolving LW construction: \"{0}\"", match.Value));
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