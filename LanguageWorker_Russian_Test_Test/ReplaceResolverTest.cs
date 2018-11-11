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

			string template = "asd\n\nfasdf^Replace '{0}': 'Мартомай'-'Мартомая', 'Июгуст'-'Июгуста', 'Сентоноябрь'-'Сентоноября', 'Декавраль'-'Декавраля'^\n\nasdfasdf";

			string original = string.Format(template, "Декавраль");
			string actual = lw.PostProcessed(original);

			Assert.AreEqual("asd\n\nfasdfДекавраля\n\nasdfasdf", actual);
		}

		[TestMethod]
		public void ReturnsOrignalValueOnSyntaxError()
		{
			LanguageWorker_Russian lw = new LanguageWorker_Russian();

			string template = "asd\n\nfasdf^Replace '{0}': 'Мартомай'-'Мартомая', 'Июгуст'-'Июгуста' 'Сентоноябрь'-'Сентоноября', 'Декавраль'-'Декавраля'^\n\nasdfasdf";

			string original = string.Format(template, "Июгуст");
			string actual = lw.PostProcessed(original);

			Assert.AreEqual(original, actual);
		}

		[TestMethod]
		public void SpacesInTheEndIsOk()
		{
			LanguageWorker_Russian lw = new LanguageWorker_Russian();

			string template = "asd\n\nfasdf^Replace '{0}': 'Мартомай'-'Мартомая', 'Июгуст'-'Июгуста', 'Сентоноябрь'-'Сентоноября', 'Декавраль'-'Декавраля'    ^\n\nasdfasdf";

			string original = string.Format(template, "Декавраль");
			string actual = lw.PostProcessed(original);

			Assert.AreEqual("asd\n\nfasdfДекавраля\n\nasdfasdf", actual);
		}
	}
}
