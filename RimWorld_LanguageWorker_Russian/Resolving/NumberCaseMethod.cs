using System.Text.RegularExpressions;
using Verse;

namespace LanguageWorkerRussian_Test.Resolving
{
	/// <summary>
	/// Uses the correct form of expression for specified number. Designed for Russian language
	/// Example: ^Number('{0}' | '# day passed' | '# days passed' | '# days passed' )^
	/// The first argument is an expression for numbers: 1, 21, 101...
	/// The second is for numbers: 2-4, 22, 23, 124...
	/// The third is for numbers: 5-10, 11-20, 25, 126 and all fractional numbers
	/// '#' will be replaced with the number itself
	/// </summary>
	public class NumberCaseMethod : IMethod
	{
		/// <summary>
		/// Regex for number including frac part: 42, 3.14
		/// </summary>
		private static readonly Regex _numberRegex = new Regex(@"(?<floor>[0-9]+)(\.(?<frac>[0-9]+))?", RegexOptions.Compiled);

		public string Call(string[] arguments)
		{
			if (arguments.Length != 4)
			{
				Log.Error($"Resolving.NumberCaseMethod: Wrong number of arguments: {arguments.Length} instead of 4");
				return null;
			}

			string numberStr = arguments[0];
			Match numberMatch = _numberRegex.Match(numberStr);
			if (!numberMatch.Success)
			{
				Log.Error($"Resolving.NumberCaseMethod: Wrong number format \"{numberStr}\"");
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

}