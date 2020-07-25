namespace LanguageWorkerRussian_Test.Resolving
{
	/// <summary>
	/// A method for composing a line from specified arguments
	/// </summary>
	public interface IMethod
	{
		/// <summary>
		/// Call the method with the specified arguments
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		string Call(string[] arguments);
	}
}