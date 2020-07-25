using RimWorld_LanguageWorker_Russian;
using System;
using System.Collections.Generic;
using Verse;

namespace LanguageWorkerRussian_Test.Resolving
{
	/// <summary>
	/// Puts the argument word or phrase in specified case
	/// Example: ^Case({TargetA} | 4)^
	/// If '{0}' is 'винтовка', it will be replaced with instrumental case form: 'винтовкой'
	/// Needs a dictionary to work
	/// </summary>
	public class CaseMethod : IMethod
	{
		private readonly CaseMap _caseMap;

		public CaseMethod(CaseMap caseMap)
		{
			_caseMap = caseMap;
		}

		public string Call(string[] arguments)
		{
			if (arguments.Length == 0)
			{
				Log.Error($"Resolving.CaseMethod: Argument list is empty");
				return null;
			}

			string input = arguments[0];

			if (arguments.Length == 1)
			{
				Log.Error($"Resolving.CaseMethod: No case number specified for entry \"{input}\"");
				return input;
			}

			string caseNumStr = arguments[1];

			if (!int.TryParse(caseNumStr, out int caseNum))
			{
				Log.Error($"Resolving.CaseMethod: Cannot parse case number \"{caseNumStr}\" for entry \"{input}\"");
				return input;
			}

			// need to process "булава из стали (нормально)". The replaced word must be "булава"
			foreach (string variant in GetVariants(input))
			{
				if (_caseMap.TryResolveCase(variant, caseNum, out string casedWord))
					return input.Replace(variant, casedWord);
			}

			Log.Warning($"Resolving.CaseMethod: No case \"{caseNumStr}\" found for entry \"{input}\"");
			return input;
		}

		/// <summary>
		/// For "mace of steel (norm)" this method returns values 'mace of steel (norm)', 'mace of steel', 'mace of', 'mace'.
		/// This is necessary to find a proper value in dictionary
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private IEnumerable<string> GetVariants(string input)
		{
			string[] parts = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			for (int variantLength = parts.Length; variantLength > 0; --variantLength)
			{
				yield return string.Join(" ", parts, 0, variantLength);
			}
		}
	}

}