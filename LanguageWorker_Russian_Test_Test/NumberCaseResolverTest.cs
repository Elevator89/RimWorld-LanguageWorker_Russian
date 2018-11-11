using LanguageWorkerRussian_Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageWorker_Russian_Test_Test
{
	[TestClass]
	public class NumberCaseResolverTest
	{
		[TestMethod]
		public void ProcessesInt()
		{
			LanguageWorker_Russian lw = new LanguageWorker_Russian();

			string template = "asd\n\nfasdf^'{0}': 1-'прошёл # день' 2-'прошло # дня' X-'прошло # дней'^\n\nasdfasdf";

			Assert.AreEqual("asd\n\nfasdfпрошёл 1 день\n\nasdfasdf", lw.PostProcessed(string.Format(template, 1)));
			Assert.AreEqual("asd\n\nfasdfпрошло 3 дня\n\nasdfasdf", lw.PostProcessed(string.Format(template, 3)));
			Assert.AreEqual("asd\n\nfasdfпрошло 5 дней\n\nasdfasdf", lw.PostProcessed(string.Format(template, 5)));
			Assert.AreEqual("asd\n\nfasdfпрошло 11 дней\n\nasdfasdf", lw.PostProcessed(string.Format(template, 11)));
			Assert.AreEqual("asd\n\nfasdfпрошло 13 дней\n\nasdfasdf", lw.PostProcessed(string.Format(template, 13)));
			Assert.AreEqual("asd\n\nfasdfпрошло 15 дней\n\nasdfasdf", lw.PostProcessed(string.Format(template, 15)));
			Assert.AreEqual("asd\n\nfasdfпрошёл 21 день\n\nasdfasdf", lw.PostProcessed(string.Format(template, 21)));
			Assert.AreEqual("asd\n\nfasdfпрошло 23 дня\n\nasdfasdf", lw.PostProcessed(string.Format(template, 23)));
			Assert.AreEqual("asd\n\nfasdfпрошло 25 дней\n\nasdfasdf", lw.PostProcessed(string.Format(template, 25)));
		}

	}
}
