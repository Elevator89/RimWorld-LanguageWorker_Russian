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

		private class PluralInfo
		{
			private Dictionary<string, string> _pluralWords = new Dictionary<string, string>();

			public void ReadFromLines(IEnumerable<string> lines)
			{
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

					_pluralWords[word] = lineWords[1];
				}
			}

			public string GetPlural(string str)
			{
				if (TryGetPlural(str, out string pluralWord))
					return pluralWord;

				return str;
			}
			public bool TryGetPlural(string str, out string pluralWord)
			{
				return _pluralWords.TryGetValue(str, out pluralWord);
			}
		}

		private class CaseInfo
		{
			private Dictionary<string, CasedWord> _casedWords = new Dictionary<string, CasedWord>();

			public void ReadFromLines(IEnumerable<string> lines)
			{
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
			private static readonly Regex _cleanEntryRegex = new Regex(@"[\w\s]*\w", RegexOptions.Compiled);

			private CaseInfo _caseInfo;

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

				return _cleanEntryRegex.Replace(input, match => _caseInfo.ResolveCase(match.Value, caseNum));
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

			public static string GetFormForNumber(int number, string formOne, string formSeveral, string formMany)
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
		private PluralInfo _pluralInfo = new PluralInfo();

		public LanguageWorker_Russian()
		{
			foreach (Tuple<VirtualDirectory, ModContentPack, string> localDirectory in LanguageDatabase.activeLanguage.AllDirectories)
			{
				VirtualDirectory wordInfoDir = localDirectory.Item1.GetDirectory("WordInfo");
				if (TryLoadLinesFromFile(wordInfoDir.GetFile("Case.txt"), localDirectory, LanguageDatabase.activeLanguage, out IEnumerable<string> caseLines))
					_caseInfo.ReadFromLines(caseLines);

				if (TryLoadLinesFromFile(wordInfoDir.GetFile("Plural.txt"), localDirectory, LanguageDatabase.activeLanguage, out IEnumerable<string> pluralLines))
					_pluralInfo.ReadFromLines(pluralLines);
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

		public override string Pluralize(string wordSingular, Gender gender, int count = -1)
		{
			if (!_pluralInfo.TryGetPlural(wordSingular, out string wordPlural))
				return wordSingular;

			if (count == -1)
				return wordPlural;

			string wordPluralSeveral = _caseInfo.ResolveCase(wordSingular, 2);
			string wordPluralMany = _caseInfo.ResolveCase(wordPlural, 2);

			return NumberCaseResolver.GetFormForNumber(count, wordSingular, wordPluralSeveral, wordPluralMany);
		}

		private static bool TryLoadLinesFromFile(VirtualFile file, Tuple<VirtualDirectory, ModContentPack, string> dir, LoadedLanguage lang, out IEnumerable<string> lines)
		{
			lines = null;
			try
			{
				lines = GenText.LinesFromString(file.ReadAllText());

				if (!lang.TryRegisterFileIfNew(dir, file.FullPath))
					return false;

				return true;
			}
			catch (DirectoryNotFoundException)
			{
				return false;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}
	}

}