using System.Collections.Generic;
using Verse;

namespace RimWorld_LanguageWorker_Russian
{
	/// <summary>
	/// Simple wrapper around Dictionary for cased forms of words
	/// </summary>
	public class CaseMap
	{
		private readonly Dictionary<string, string[]> _map = new Dictionary<string, string[]>();

		public void AddEntry(string[] casedWordForms)
		{
			if (casedWordForms.Length < 2)
				return;

			_map[casedWordForms[0]] = casedWordForms;
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

			if (!_map.TryGetValue(str, out string[] cases))
				return false;

			if (caseNum >= cases.Length)
				return false;

			casedWord = cases[caseNum];
			return true;
		}
	}

}