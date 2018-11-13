using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LanguageWorkerRussian_Test;

namespace LanguageWorker_Russian_Test_Test
{
	[TestClass]
	public class ReplaceResolverTest
	{
		[TestMethod]
		public void ReplacesRegularLine()
		{
			LanguageWorker_Russian lw = new LanguageWorker_Russian();

			string template = "asd\n\nfasdf^Replace({0} | 'Мартомай'-'Мартомая'| 'Июгуст'-'Июгуста'| 'Сентоноябрь'-'Сентоноября'| 'Декавраль'-'Декавраля')^\n\nasdfasdf";

			string original = string.Format(template, "Декавраль");
			string actual = lw.PostProcessed(original);

			Assert.AreEqual("asd\n\nfasdfДекавраля\n\nasdfasdf", actual);
		}

		[TestMethod]
		public void ProcessesToLastCorrectValueValueOnSyntaxError()
		{
			LanguageWorker_Russian lw = new LanguageWorker_Russian();

			string template = "asd\n\nfasdf^Replace( {0} | 'Мартомай'-'Мартомая'| 'Июгуст'-'Июгуста' 'Сентоноябрь'-'Сентоноября'| 'Декавраль'-'Декавраля')^\n\nasdfasdf";

			string original = string.Format(template, "Июгуст");
			string actual = lw.PostProcessed(original);

			Assert.AreEqual("asd\n\nfasdfИюгуста\n\nasdfasdf", actual);

			original = string.Format(template, "Сентоноябрь");
			actual = lw.PostProcessed(original);

			Assert.AreEqual("asd\n\nfasdfСентоноябрь\n\nasdfasdf", actual);

		}

		[TestMethod]
		public void SpacesInTheEndIsOk()
		{
			LanguageWorker_Russian lw = new LanguageWorker_Russian();

			string template = "asd\n\nfasdf^Replace( {0} | 'Мартомай'-'Мартомая'| 'Июгуст'-'Июгуста'| 'Сентоноябрь'-'Сентоноября'| 'Декавраль'-'Декавраля'     )^\n\nasdfasdf";

			string original = string.Format(template, "Декавраль");
			string actual = lw.PostProcessed(original);

			Assert.AreEqual("asd\n\nfasdfДекавраля\n\nasdfasdf", actual);
		}

		[TestMethod]
		public void PrecessesMultipleCase()
		{
			LanguageWorker_Russian lw = new LanguageWorker_Russian();

			string template = "Головная боль пронзает черепа ваших ^Replace( {0} | 'мужского пола'-'мужчин' | 'женского пола'-'женщин')^ словно гвоздь!\n\nПсихоактивный гул, исходящий от некоего далёкого излучателя, пронизывает эту местность ненавистью. Похоже, частота излучения влияет только на ^Replace( {0} | 'мужского пола'-'мужчин'| 'женского пола'-'женщин')^. В течение нескольких дней настроение у них будет немного хуже.\n\nУровень гула — {1}.";

			string original = string.Format(template, "мужского пола", "низкий");
			string actual = lw.PostProcessed(original);

			Assert.AreEqual("Головная боль пронзает черепа ваших мужчин словно гвоздь!\n\nПсихоактивный гул, исходящий от некоего далёкого излучателя, пронизывает эту местность ненавистью. Похоже, частота излучения влияет только на мужчин. В течение нескольких дней настроение у них будет немного хуже.\n\nУровень гула — низкий.", actual);
		}
	}
}
