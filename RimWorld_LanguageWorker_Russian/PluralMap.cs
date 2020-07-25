using System.Collections.Generic;
using Verse;

namespace RimWorld_LanguageWorker_Russian
{
	/// <summary>
	/// Simple wrapper around Dictionary for plural words
	/// </summary>
	public class PluralMap
	{
		private readonly Dictionary<string, string> _map = new Dictionary<string, string>();

		public void AddEntry(string singular, string plural)
		{
			_map[singular] = plural;
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
			return _map.TryGetValue(str, out pluralWord);
		}
	}

}