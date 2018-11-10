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

			string template = "asdfasdf^Replace '{0}': 'Мартомай'-'Мартомая', 'Июгуст'-'Июгуста', 'Сентоноябрь'-'Сентоноября', 'Декавраль'-'Декавраля'^asdfasdf";

			string original = string.Format(template, "Декавраль");

			string actual = lw.PostProcessed(original);

			Assert.AreEqual("asdfasdfДекавраляasdfasdf", actual);
		}
	}
}
