using System.Collections.Generic;
using System.Text.RegularExpressions;
using Verse;

namespace LanguageWorkerRussian_Test.Resolving
{
	/// <summary>
	/// Processes in-text instructions, determines the required method and calls it
	/// </summary>
	public class Resolver
	{
		/// <summary>
		/// Regex to find and process in-text resolver instructions. An instruction should look like "^DoSomething( argument1 | argument2 | argument3 )^"
		/// </summary>
		private static readonly Regex _resolverInstructionRegex = new Regex(@"\^(?<methodName>\w+)\(\s*(?<argument>[^|]+?)\s*(\|\s*(?<argument>[^|]+?)\s*)*\)\^", RegexOptions.Compiled);

		private readonly Dictionary<string, IMethod> _methodMap = new Dictionary<string, IMethod>();

		/// <summary>
		/// Register a method for text processing pruposes
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="method"></param>
		public void RegisterMethod(string methodName, IMethod method)
		{
			_methodMap[methodName] = method;
		}

		/// <summary>
		/// Read and evaluate all in-text resolver instructions in the specified text
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		public string ProcessLine(string line)
		{
			return _resolverInstructionRegex.Replace(line, Process);
		}

		/// <summary>
		/// Process a single instruction
		/// </summary>
		/// <param name="instructionMatch"></param>
		/// <returns>text for replacement</returns>
		private string Process(Match instructionMatch)
		{
			string methodName = instructionMatch.Groups["methodName"].Value;

			Group argumentsGroup = instructionMatch.Groups["argument"];

			string[] arguments = new string[argumentsGroup.Captures.Count];
			for (int i = 0; i < argumentsGroup.Captures.Count; ++i)
			{
				arguments[i] = argumentsGroup.Captures[i].Value.Trim();
			}

			if (!_methodMap.TryGetValue(methodName, out IMethod method))
			{
				Log.Error($"Resolving: No method found for instruction \"{methodName}\"");
				return instructionMatch.Value;
			}

			string result = method.Call(arguments);
			if (result == null)
			{
				Log.Error($"Resolving: Error happened while resolving instruction: \"{instructionMatch.Value}\"");
				return instructionMatch.Value;
			}

			return result;
		}
	}
}
