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

		private class PluralInfo
		{
			private readonly Dictionary<string, string> _pluralWords = new Dictionary<string, string>();

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

				Log.Warning($"LW: Plural form for \"{str}\" not found");
				return str;
			}
			public bool TryGetPlural(string str, out string pluralWord)
			{
				return _pluralWords.TryGetValue(str, out pluralWord);
			}
		}

		private class CaseInfo
		{
			private readonly Dictionary<string, string[]> _casedWords = new Dictionary<string, string[]>();

			public void ReadFromLines(IEnumerable<string> lines)
			{
				foreach (string line in lines)
				{
					if (line.NullOrEmpty())
						continue;

					string[] cases = line.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);

					if (cases.Length < 2)
						continue;

					string word = cases[0];
					if (word.NullOrEmpty())
						continue;

					_casedWords[word] = cases;
				}
			}

			public string ResolveCase(string str, int caseNum)
			{
				if (TryResolveCase(str, caseNum, out string casedWord))
					return casedWord;

				Log.Warning($"LW: Case form {caseNum} for \"{str}\" not found");
				return str;
			}

			public bool TryResolveCase(string str, int caseNum, out string casedWord)
			{
				casedWord = str;

				if (!_casedWords.TryGetValue(str, out string[] cases))
					return false;

				if (caseNum >= cases.Length)
					return false;

				casedWord = cases[caseNum];
				return true;
			}
		}

		private class CaseResolver : IResolver
		{
			private readonly CaseInfo _caseInfo;

			public CaseResolver(CaseInfo caseInfo)
			{
				_caseInfo = caseInfo;
			}

			public string Resolve(string[] arguments)
			{
				if (arguments.Length == 0)
				{
					Log.Error($"LW CaseResolver: Argument list is empty");
					return null;
				}

				string input = arguments[0];

				if (arguments.Length == 1)
				{
					Log.Error($"LW CaseResolver: No case number specified for entry \"{input}\"");
					return input;
				}

				string caseNumStr = arguments[1];

				if (!int.TryParse(caseNumStr, out int caseNum))
				{
					Log.Error($"LW CaseResolver: Cannot parse case number \"{caseNumStr}\" for entry \"{input}\"");
					return input;
				}

				// need to process "булава из стали (нормально)". The replaced word must be "булава"
				foreach (string variant in GetVariants(input))
				{
					if (_caseInfo.TryResolveCase(variant, caseNum, out string casedWord))
						return input.Replace(variant, casedWord);
				}

				Log.Warning($"LW CaseResolver: No case \"{caseNumStr}\" found for entry \"{input}\"");
				return input;
			}

			private IEnumerable<string> GetVariants(string input)
			{
				string[] parts = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				for (int variantLength = parts.Length; variantLength > 0; --variantLength)
				{
					yield return string.Join(" ", parts, 0, variantLength);
				}
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
					Log.Error($"LW ReplaceResolver: Argument list is empty");
					return null;
				}

				string input = arguments[0];

				if (arguments.Length == 1)
				{
					Log.Warning($"LW ReplaceResolver: No replace insturctions for input \"{input}\"");
					return input;
				}

				for (int i = 1; i < arguments.Length; ++i)
				{
					string argument = arguments[i];

					Match match = _argumentRegex.Match(argument);
					if (!match.Success)
					{
						Log.Error($"LW ReplaceResolver: Wrong format for replace instruction: \"{argument}\"");
						return null;
					}

					string oldValue = match.Groups["old"].Value;
					string newValue = match.Groups["new"].Value;

					if (oldValue == input)
						return newValue;
				}

				Log.Warning($"LW ReplaceResolver: No replacement found for \"{input}\"");
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
					Log.Error($"LW NumberCaseResolver: Wrong number of arguments: {arguments.Length} instead of 4");
					return null;
				}

				string numberStr = arguments[0];
				Match numberMatch = _numberRegex.Match(numberStr);
				if (!numberMatch.Success)
				{
					Log.Error($"LW NumberCaseResolver: Wrong number format \"{numberStr}\"");
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

		private const string CaseFileName = "Case.txt";
		private const string PluralFileName = "Plural.txt";

		private readonly CaseResolver _caseResolver;
		private static readonly ReplaceResolver replaceResolver = new ReplaceResolver();
		private static readonly NumberCaseResolver numberCaseResolver = new NumberCaseResolver();

		private static readonly Regex _languageWorkerResolverRegex = new Regex(@"\^(?<resolverName>\w+)\(\s*(?<argument>[^|]+?)\s*(\|\s*(?<argument>[^|]+?)\s*)*\)\^", RegexOptions.Compiled);

		private readonly CaseInfo _caseInfo = new CaseInfo();
		private readonly PluralInfo _pluralInfo = new PluralInfo();

		public LanguageWorker_Russian()
		{
			LoadedLanguage language = LanguageDatabase.activeLanguage;

			foreach (Tuple<VirtualDirectory, ModContentPack, string> localDirectory in language.AllDirectories)
			{
				VirtualDirectory wordInfoDir = localDirectory.Item1.GetDirectory("WordInfo");
				if (TryLoadLinesFromFile(wordInfoDir.GetFile(CaseFileName), localDirectory, language, out IEnumerable<string> caseLines))
				{
					_caseInfo.ReadFromLines(caseLines);
					Log.Message($"LW: Case dictionary loaded from file \"{wordInfoDir.FullPath}/{CaseFileName}\"");
				}

				if (TryLoadLinesFromFile(wordInfoDir.GetFile(PluralFileName), localDirectory, language, out IEnumerable<string> pluralLines))
				{
					_pluralInfo.ReadFromLines(pluralLines);
					Log.Message($"LW: Plural words loaded from file \"{wordInfoDir.FullPath}/{PluralFileName}\"");
				}
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
			if (resolver == null)
			{
				Log.Error($"LW: No resolver found for instruction \"{keyword}\"");
				return match.Value;
			}

			string result = resolver.Resolve(arguments);
			if (result == null)
			{
				Log.Error($"LW: Error happened while resolving instruction: \"{match.Value}\"");
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
			{
				Log.Warning($"LW: No plural form found for \"{wordSingular}\"");
				return wordSingular;
			}

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