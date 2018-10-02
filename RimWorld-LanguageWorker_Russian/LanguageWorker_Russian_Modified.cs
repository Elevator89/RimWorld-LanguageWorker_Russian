//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
using Verse;

namespace LanguageWorkerRussian_Test
{
	public class LanguageWorker_Russian_Modified : LanguageWorker
	{
		protected LanguageWorker_Russian_Modified()
		{
			Log.Message("LanguageWorker_Russian_Modified is created");
		}

		public override string PostProcessedKeyedTranslation(string translation)
		{
			Log.MessageFormat("PostProcessedKeyedTranslation: \"{1}\"", translation);
			translation = translation
			  //.ProcessTimeSpan()
			  .ProcessDate();

			Log.MessageFormat("PostProcessedKeyedTranslation: \"{1}\"", translation);

			return translation;
		}

		public override string PostProcessed(string str)
		{
			Log.MessageFormat("Initial string {0}", str);
			string postProcessedStr = base.PostProcessed(str);
			Log.MessageFormat("PostProcessed string {0}", postProcessedStr);

			return postProcessedStr;
		}

		public override string OrdinalNumber(int number, Gender gender = Gender.None)
		{
			switch (gender)
			{
				case Gender.None:
					return number + "-е";
				case Gender.Male:
					return number + "-й";
				case Gender.Female:
					return number + "-я";
				default:
					return number.ToString();
			}
		}

		public override string Pluralize(string str, Gender gender, int count = -1)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			char c = str[str.Length - 1];
			char c2 = (str.Length < 2) ? '\0' : str[str.Length - 2];
			if (gender != Gender.Male)
			{
				if (gender != Gender.Female)
				{
					if (gender == Gender.None)
					{
						if (c == 'o')
						{
							return str.Substring(0, str.Length - 1) + 'a';
						}
						if (c == 'O')
						{
							return str.Substring(0, str.Length - 1) + 'A';
						}
						if (c == 'e' || c == 'E')
						{
							char value = char.ToUpper(c2);
							if ("ГКХЖЧШЩЦ".IndexOf(value) >= 0)
							{
								if (c == 'e')
								{
									return str.Substring(0, str.Length - 1) + 'a';
								}
								if (c == 'E')
								{
									return str.Substring(0, str.Length - 1) + 'A';
								}
							}
							else
							{
								if (c == 'e')
								{
									return str.Substring(0, str.Length - 1) + 'я';
								}
								if (c == 'E')
								{
									return str.Substring(0, str.Length - 1) + 'Я';
								}
							}
						}
					}
				}
				else
				{
					if (c == 'я')
					{
						return str.Substring(0, str.Length - 1) + 'и';
					}
					if (c == 'ь')
					{
						return str.Substring(0, str.Length - 1) + 'и';
					}
					if (c == 'Я')
					{
						return str.Substring(0, str.Length - 1) + 'И';
					}
					if (c == 'Ь')
					{
						return str.Substring(0, str.Length - 1) + 'И';
					}
					if (c == 'a' || c == 'A')
					{
						char value2 = char.ToUpper(c2);
						if ("ГКХЖЧШЩ".IndexOf(value2) >= 0)
						{
							if (c == 'a')
							{
								return str.Substring(0, str.Length - 1) + 'и';
							}
							return str.Substring(0, str.Length - 1) + 'И';
						}
						else
						{
							if (c == 'a')
							{
								return str.Substring(0, str.Length - 1) + 'ы';
							}
							return str.Substring(0, str.Length - 1) + 'Ы';
						}
					}
				}
			}
			else
			{
				if (this.IsConsonant(c))
				{
					return str + 'ы';
				}
				if (c == 'й')
				{
					return str.Substring(0, str.Length - 1) + 'и';
				}
				if (c == 'ь')
				{
					return str.Substring(0, str.Length - 1) + 'и';
				}
				if (c == 'Й')
				{
					return str.Substring(0, str.Length - 1) + 'И';
				}
				if (c == 'Ь')
				{
					return str.Substring(0, str.Length - 1) + 'И';
				}
			}
			return str;
		}

		public bool IsConsonant(char ch)
		{
			return "бвгджзклмнпрстфхцчшщБВГДЖЗКЛМНПРСТФХЦЧШЩ".IndexOf(ch) >= 0;
		}
	}

}