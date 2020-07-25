using System.Text.RegularExpressions;
using Verse;

namespace LanguageWorkerRussian_Test.Resolving
{
	/// <summary>
	/// Replaces the entry given in the first argument using the rules specified by the next arguments
	/// Example: ^Replace('{0}' | 'male'-'men' | 'female'-'women' )^
	/// If '{0}' is 'male', it will be replaced by 'men'
	/// </summary>
	public class ReplaceMethod : IMethod
	{
		/// <summary>
		/// Format for replacement rule argument (with apostrophes): 'Мартомай'-'Мартомая'
		/// </summary>
		private static readonly Regex _replacementRuleRegex = new Regex(@"'(?<old>[^']*?)'-'(?<new>[^']*?)'", RegexOptions.Compiled);

		public string Call(string[] arguments)
		{
			if (arguments.Length == 0)
			{
				Log.Error($"Resolving.ReplaceMethod: Argument list is empty");
				return null;
			}

			string input = arguments[0];

			if (arguments.Length == 1)
			{
				Log.Warning($"Resolving.ReplaceMethod: No replacement rules given for input \"{input}\"");
				return input;
			}

			for (int i = 1; i < arguments.Length; ++i)
			{
				string argument = arguments[i];

				Match match = _replacementRuleRegex.Match(argument);
				if (!match.Success)
				{
					Log.Error($"Resolving.ReplaceMethod: Wrong format for replacement rule: \"{argument}\"");
					return null;
				}

				string oldValue = match.Groups["old"].Value;
				string newValue = match.Groups["new"].Value;

				if (oldValue == input)
					return newValue;
			}

			Log.Warning($"Resolving.ReplaceMethod: No replacement found for \"{input}\"");
			return input;
		}
	}

}