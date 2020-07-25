using RimWorld.IO;
using System;
using System.Collections.Generic;
using System.IO;
using Verse;

namespace RimWorld_LanguageWorker_Russian
{
	public static class LanguageWorkerUtil
	{
		private const string WordInfoDirName = "WordInfo";
		private const string CaseFileName = "Case.txt";
		private const string PluralFileName = "Plural.txt";

		/// <summary>
		/// Read CaseMap from WordInfo files
		/// </summary>
		/// <returns></returns>
		public static CaseMap ReadAndBuildCaseMap()
		{
			LoadedLanguage language = LanguageDatabase.activeLanguage;
			CaseMap caseMap = new CaseMap();

			foreach (Tuple<VirtualDirectory, ModContentPack, string> localDirectory in language.AllDirectories)
			{
				VirtualDirectory wordInfoDir = localDirectory.Item1.GetDirectory(WordInfoDirName);
				if (LanguageWorkerUtil.TryLoadLinesFromFile(wordInfoDir.GetFile(CaseFileName), localDirectory, language, out IEnumerable<string> casedLines))
				{
					foreach (string casedline in casedLines)
						if (LanguageWorkerUtil.TryGetSemicolonSeparatedValues(casedline, out string[] casedForms))
							caseMap.AddEntry(casedForms);

					Log.Message($"LW: Case dictionary loaded from file \"{wordInfoDir.FullPath}/{CaseFileName}\"");
				}
			}

			return caseMap;
		}

		/// <summary>
		/// Read PluralMap from WordInfo files
		/// </summary>
		/// <returns></returns>
		public static PluralMap ReadAndBuildPluralMap()
		{
			LoadedLanguage language = LanguageDatabase.activeLanguage;
			PluralMap pluralMap = new PluralMap();

			// Read PluralMap from files
			foreach (Tuple<VirtualDirectory, ModContentPack, string> localDirectory in language.AllDirectories)
			{
				VirtualDirectory wordInfoDir = localDirectory.Item1.GetDirectory(WordInfoDirName);
				if (LanguageWorkerUtil.TryLoadLinesFromFile(wordInfoDir.GetFile(PluralFileName), localDirectory, language, out IEnumerable<string> pluralLines))
				{
					foreach (string pluralLine in pluralLines)
						if (LanguageWorkerUtil.TryGetSemicolonSeparatedValues(pluralLine, out string[] singularAndPlural) && singularAndPlural.Length >= 2)
							pluralMap.AddEntry(singularAndPlural[0], singularAndPlural[1]);

					Log.Message($"LW: Plural words loaded from file \"{wordInfoDir.FullPath}/{PluralFileName}\"");
				}
			}

			return pluralMap;
		}

		public static bool TryGetSemicolonSeparatedValues(string input, out string[] values)
		{
			values = null;
			if (input.NullOrEmpty())
				return false;

			values = input.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < values.Length; ++i)
				values[i] = values[i].Trim();

			bool valuesAreEmpty = true;
			for (int i = 0; i < values.Length; ++i)
				valuesAreEmpty &= values[i].NullOrEmpty();

			return !valuesAreEmpty;
		}

		public static bool TryLoadLinesFromFile(VirtualFile file, Tuple<VirtualDirectory, ModContentPack, string> dir, LoadedLanguage lang, out IEnumerable<string> lines)
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
