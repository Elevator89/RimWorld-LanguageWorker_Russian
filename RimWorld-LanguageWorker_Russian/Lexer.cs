using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanguageWorkerRussian_Test
{
	public enum LexemType
	{
		Text = 0,
		TagOpeninig,
		TagClosing,
	}

	public enum LexerState
	{
		Initial = 0,
		Text,
		TagUnconfirmed,
		TagOpeninig,
		TagClosingUnconfirmed,
		TagClosing,
		TagFinish,
	}

	public class Lexem
	{
		public LexemType LexemType { get; private set; }
		public string StrValue { get; private set; }

		public Lexem(LexemType lexemType, string strValue)
		{
			LexemType = lexemType;
			StrValue = strValue;
		}
	}

	public class Lexer
	{
		private int _lexemBaseIndex = 0;
		private int _lexemUnconfirmedBaseIndex = 0;
		private LexerState _state = LexerState.Initial;

		public IEnumerable<Lexem> Parse(string input)
		{
			for (int i = 0; i < input.Length; ++i)
			{
				char c = input[i];

				switch (_state)
				{
					case LexerState.Initial:
						switch (c)
						{
							case '[':
								_state = LexerState.TagUnconfirmed;
								_lexemUnconfirmedBaseIndex = i;
								break;
							default:
								_state = LexerState.Text;
								break;
						}
						break;
					case LexerState.Text:
						switch (c)
						{
							case '[':
								_state = LexerState.TagUnconfirmed;
								_lexemUnconfirmedBaseIndex = i;
								break;
							default:
								break;
						}
						break;
					case LexerState.TagUnconfirmed:
						switch (c)
						{
							case '$':
								_state = LexerState.TagOpeninig;
								_lexemUnconfirmedBaseIndex = i;
								_lexemBaseIndex = i;
								break;
							case '/':
								_state = LexerState.TagClosingUnconfirmed;
								break;
							default:
								_state = LexerState.Text;
								break;
						}
						break;
					case LexerState.TagOpeninig:
						switch (c)
						{
							case ']':
								yield return new Lexem(LexemType.TagOpeninig, input.Substring(_lexemBaseIndex, i - _lexemBaseIndex));
								_state = LexerState.TagFinish;
								_lexemBaseIndex = i;
								break;
							default:
								break;
						}
						break;
					case LexerState.TagClosingUnconfirmed:
						switch (c)
						{
							case '$':
								_state = LexerState.TagClosing;
								_lexemBaseIndex = i;
								break;
							default:
								_state = LexerState.Text;
								break;
						}
						break;
					case LexerState.TagClosing:
						switch (c)
						{
							case ']':
								yield return new Lexem(LexemType.TagClosing, input.Substring(_lexemBaseIndex, i - _lexemBaseIndex));
								_state = LexerState.TagFinish;
								_lexemBaseIndex = i;
								break;
							default:
								break;
						}
						break;
					case LexerState.TagFinish:
						switch (c)
						{
							case '[':
								yield return new Lexem(LexemType.Text, string.Empty);
								_state = LexerState.TagUnconfirmed;
								_lexemBaseIndex = i;
								break;
							default:
								_state = LexerState.Text;
								_lexemBaseIndex = i;
								break;
						}
						break;
				}
			}

			switch (_state)
			{
				case LexerState.Initial:
					yield return new Lexem(LexemType.Text, string.Empty);
					break;
				case LexerState.TagFinish:
					break;
				default:
					yield return new Lexem(LexemType.Text, input.Substring(_lexemBaseIndex, input.Length - _lexemBaseIndex));
					break;
			}
		}
	}
}
