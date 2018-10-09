using System;

namespace LexerTestbed
{
	class Program
	{
		static void Main(string[] args)
		{
			Lexer lexer = new Lexer();

			while (true)
			{
				Console.Write("Enter line: ");
				string input = Console.ReadLine();

				foreach(Lexem lexem in lexer.Parse(input))
				{
					Console.WriteLine("{0}: \"{1}\"", lexem.LexemType, lexem.StrValue);
				}
				Console.WriteLine();

				if (string.IsNullOrWhiteSpace(input))
					return;
			}
		}
	}
}
