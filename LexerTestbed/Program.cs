using System;
using System.IO;
using LanguageWorkerRussian_Test;

namespace LexerTestbed
{
	class Program
	{
		static void Main(string[] args)
		{
            string filenameInput = args[0];

            LanguageWorker_Russian lw = new LanguageWorker_Russian();

            string text = File.ReadAllText(filenameInput);
		    string[] inputLines = text.Split(new [] { "\n\n\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in inputLines)
            {
                Console.WriteLine(lw.PostProcessedKeyedTranslation(line));
            }

            Console.WriteLine("Press ENTER to finish");
            Console.ReadLine();

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
    }
}
