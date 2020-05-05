using RimWorld.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Verse;

namespace RimWorld_LanguageWorker_Russian
{
	public class LanguageWorker_Russian : LanguageWorker
	{
		private interface IResolver
		{
			string Resolve(string[] arguments);
		}

		public enum Case
		{
			Nominative = 0,
			Genitive = 1,
			Dative = 2,
			Accusative = 3,
			Instrumental = 4,
			Prepositional = 5
		}

		private class CasedWord
		{
			private string[] _cases = new string[6];

			public CasedWord(string nominative, string genitive, string dative, string accusative, string instrumental, string prepositional)
			{
				_cases[(int)Case.Nominative] = nominative;
				_cases[(int)Case.Genitive] = genitive;
				_cases[(int)Case.Dative] = dative;
				_cases[(int)Case.Accusative] = accusative;
				_cases[(int)Case.Instrumental] = instrumental;
				_cases[(int)Case.Prepositional] = prepositional;
			}

			public CasedWord(string[] forms)
			{
				for (int i = 0; i < forms.Length; ++i)
					_cases[i] = forms[i];
			}

			public bool TryForCase(Case wordCase, out string casedWord)
			{
				return TryForCase((int)wordCase, out casedWord);
			}

			public bool TryForCase(int caseNum, out string casedWord)
			{
				casedWord = _cases[caseNum];
				return !casedWord.NullOrEmpty();
			}
		}

		private class CaseInfo
		{
			private Dictionary<string, CasedWord> _casedWords = new Dictionary<string, CasedWord>();

			public void LoadFrom(Tuple<VirtualDirectory, ModContentPack, string> dir, LoadedLanguage lang)
			{
				VirtualDirectory directory = dir.Item1.GetDirectory("WordInfo").GetDirectory("Case");
				TryLoadFromFile(directory.GetFile("Case.txt"), dir, lang);
			}

			private void TryLoadFromFile(VirtualFile file, Tuple<VirtualDirectory, ModContentPack, string> dir, LoadedLanguage lang)
			{
				IEnumerable<string> lines;
				try
				{
					lines = GenText.LinesFromString(file.ReadAllText());
				}
				catch (DirectoryNotFoundException)
				{
					return;
				}
				catch (FileNotFoundException)
				{
					return;
				}
				if (!lang.TryRegisterFileIfNew(dir, file.FullPath))
				{
					return;
				}

				foreach (string line in lines)
				{
					if (line.NullOrEmpty())
						continue;

					string[] lineWords = line.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);

					if (lineWords.Length < 2)
						continue;

					string word = lineWords[0];
					if (word.NullOrEmpty())
						continue;

					_casedWords[word] = new CasedWord(lineWords);
				}
			}

			public string ResolveCase(string str, int caseNum)
			{
				if (TryResolveCase(str, caseNum, out string casedWord))
					return casedWord;

				return str;
			}

			public bool TryResolveCase(string str, int caseNum, out string casedWord)
			{
				casedWord = str;

				if (!_casedWords.TryGetValue(str, out CasedWord word))
					return false;

				if (!word.TryForCase(caseNum, out casedWord))
					return false;

				return true;
			}
		}

		private class CaseResolver : IResolver
		{
			CaseInfo _caseInfo;

			public CaseResolver(CaseInfo caseInfo)
			{
				_caseInfo = caseInfo;
			}

			public string Resolve(string[] arguments)
			{
				if (arguments.Length == 0)
					return null;

				string input = arguments[0];

				if (arguments.Length == 1)
					return input;

				string caseNumStr = arguments[1];

				if (!int.TryParse(caseNumStr, out int caseNum))
					return input;

				return _caseInfo.ResolveCase(input, caseNum);
			}
		}

		private class ReplaceResolver : IResolver
		{
			// ^Replace('{0}', 'Мартомай'-'Мартомая', 'Июгуст'-'Июгуста', 'Сентоноябрь'-'Сентоноября', 'Декавраль'-'Декавраля')^
			private static readonly Regex _argumentRegex = new Regex(@"'(?<old>[^']*?)'-'(?<new>[^']*?)'", RegexOptions.Compiled);

			public string Resolve(string[] arguments)
			{
				if (arguments.Length == 0)
				{
					return null;
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
						return null;
					}

					string oldValue = match.Groups["old"].Value;
					string newValue = match.Groups["new"].Value;

					if (oldValue == input)
					{
						return newValue;
					}
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
					return null;
				}

				string numberStr = arguments[0];
				Match numberMatch = _numberRegex.Match(numberStr);
				if (!numberMatch.Success)
				{
					return null;
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

		private readonly CaseResolver _caseResolver;
		private static readonly ReplaceResolver replaceResolver = new ReplaceResolver();
		private static readonly NumberCaseResolver numberCaseResolver = new NumberCaseResolver();

		private static readonly Regex _languageWorkerResolverRegex = new Regex(@"\^(?<resolverName>\w+)\(\s*(?<argument>[^|]+?)\s*(\|\s*(?<argument>[^|]+?)\s*)*\)\^", RegexOptions.Compiled);

		private CaseInfo _caseInfo = new CaseInfo();

		public LanguageWorker_Russian()
		{
			foreach (Tuple<VirtualDirectory, ModContentPack, string> localDirectory in LanguageDatabase.activeLanguage.AllDirectories)
			{
				_caseInfo.LoadFrom(localDirectory, LanguageDatabase.activeLanguage);
			}

			_caseResolver = new CaseResolver(_caseInfo);
		}

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

		private string PostProcess(string translation)
		{
			return _languageWorkerResolverRegex.Replace(translation, EvaluateResolver);
		}

		private string EvaluateResolver(Match match)
		{
			string keyword = match.Groups["resolverName"].Value;

			Group argumentsGroup = match.Groups["argument"];

			string[] arguments = new string[argumentsGroup.Captures.Count];
			for (int i = 0; i < argumentsGroup.Captures.Count; ++i)
			{
				arguments[i] = argumentsGroup.Captures[i].Value.Trim();
			}

			IResolver resolver = GetResolverByKeyword(keyword);

			string result = resolver.Resolve(arguments);
			if (result == null)
			{
				Log.Error(string.Format("Error happened while resolving LW instruction: \"{0}\"", match.Value));
				return match.Value;
			}

			return result;
		}

		private IResolver GetResolverByKeyword(string keyword)
		{
			switch (keyword)
			{
				case "Case":
					return _caseResolver;
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