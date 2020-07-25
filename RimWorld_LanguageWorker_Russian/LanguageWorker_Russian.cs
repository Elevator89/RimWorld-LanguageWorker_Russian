using LanguageWorkerRussian_Test.Resolving;
using Verse;

namespace RimWorld_LanguageWorker_Russian
{
	public partial class LanguageWorker_Russian : LanguageWorker
	{
		private readonly CaseMap _caseMap;
		private readonly PluralMap _pluralMap;

		private readonly Resolver _resolver = new Resolver();

		public LanguageWorker_Russian()
		{
			_caseMap = LanguageWorkerUtil.ReadAndBuildCaseMap();
			_pluralMap = LanguageWorkerUtil.ReadAndBuildPluralMap();

			_resolver.RegisterMethod("Case", new CaseMethod(_caseMap));
			_resolver.RegisterMethod("Replace", new ReplaceMethod());
			_resolver.RegisterMethod("Number", new NumberCaseMethod());
		}

		public override string PostProcessedKeyedTranslation(string translation)
		{
			translation = base.PostProcessedKeyedTranslation(translation);
			return _resolver.ProcessLine(translation);
		}

		public override string PostProcessed(string str)
		{
			str = base.PostProcessed(str);
			return _resolver.ProcessLine(str);
		}

		public override string Pluralize(string wordSingular, Gender gender, int count = -1)
		{
			if (!_pluralMap.TryGetPlural(wordSingular, out string wordPlural))
			{
				Log.Warning($"LW: No plural form found for \"{wordSingular}\"");
				return wordSingular;
			}

			if (count == -1)
				return wordPlural;

			string wordPluralSeveral = _caseMap.ResolveCase(wordSingular, 2);
			string wordPluralMany = _caseMap.ResolveCase(wordPlural, 2);

			return NumberCaseMethod.GetFormForNumber(count, wordSingular, wordPluralSeveral, wordPluralMany);
		}

		public override string ToTitleCase(string str)
		{
			return GenText.ToTitleCaseSmart(str);
		}
	}
}