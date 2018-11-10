using System;
using System.IO;
using LanguageWorkerRussian_Test;
using System.Text.RegularExpressions;

namespace LexerTestbed
{
	class Program
	{
		static void Main(string[] args)
		{
			// "asdfasdf": "old1"-"new1", "old2"-"new2", "old3"-"new3", "old4"-"new4"
			Regex _replacementArgumentsLineRegex = new Regex("^\"(?<input>[^\"]*?)\":\\s*(\"(?<old>[^\"]*?)\"-\"(?<new>[^\"]*?)\")(,\\s*\"(?<old>[^\"]*?)\"-\"(?<new>[^\"]*?)\")*$", RegexOptions.Compiled);

			// "3.14": 1-"one"  2-"two"X-"many"
			Regex _numberCaseArgumentsLineRegex = new Regex("^\"(?<number>(?<floor>[0-9]+)(\\.(?<frac>[0-9]+)))?\":\\s*1-\"(?<one>[^\"]*?)\"\\s*2-\"(?<several>[^\"]*?)\"\\s*X-\"(?<many>[^\"]*?)\"$", RegexOptions.Compiled);

			while (true)
			{
				Console.Write("Enter line: ");
				string input = Console.ReadLine();

				TestRegex(_numberCaseArgumentsLineRegex, input);
				
				Console.WriteLine();

				if (string.IsNullOrEmpty(input))
					return;
			}



			//      string filenameInput = args[0];

			//      LanguageWorker_Russian lw = new LanguageWorker_Russian();

			//      string text = File.ReadAllText(filenameInput);
			//string[] inputLines = text.Split(new [] { "\n\n\n\n" }, StringSplitOptions.RemoveEmptyEntries);
			//      foreach (string line in inputLines)
			//      {
			//          Console.WriteLine(lw.PostProcessedKeyedTranslation(line));
			//      }

			//      Console.WriteLine("Press ENTER to finish");
			//      Console.ReadLine();




			//Lexer lexer = new Lexer();

			//while (true)
			//{
			//	Console.Write("Enter line: ");
			//	string input = Console.ReadLine();

			//	foreach(Lexem lexem in lexer.Parse(input))
			//	{
			//		Console.WriteLine("{0}: \"{1}\"", lexem.LexemType, lexem.StrValue);
			//	}
			//	Console.WriteLine();

			//	if (string.IsNullOrWhiteSpace(input))
			//		return;
			//}
		}

		private static void TestRegex(Regex regex, string input)
		{
			Match match = regex.Match(input);

			if (match.Success)
			{
				string[] groupNames = regex.GetGroupNames();

				foreach (string groupName in groupNames)
				{
					Group group = match.Groups[groupName];
					Console.WriteLine("Group \"{0}\": \"{1}\"", groupName, group.Value);

					for (int c = 0; c < group.Captures.Count; ++c)
					{
						Console.WriteLine("Group \"{0}\", Capture {1}: \"{2}\"", groupName, c, group.Captures[c].Value);
					}
				}

				for (int g = 0; g < match.Groups.Count; ++g)
				{
					Console.WriteLine("Group {0}: \"{1}\"", g, match.Groups[g].Value);

					for (int c = 0; c < match.Groups[g].Captures.Count; ++c)
					{
						Console.WriteLine("Group {0}, Capture {1}: \"{2}\"", g, c, match.Groups[g].Captures[c].Value);
					}
				}
			}
			else
			{
				Console.WriteLine("No match");
			}
		}
	}
}
