namespace LanguageWorkerRussian_Test
{
    internal static class Log
    {
        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.
        /// </summary>
        /// 
        /// <returns>
        /// A copy of <paramref name="format"/> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args"/>.
        /// </returns>
        /// <param name="format">A composite format string (see Remarks). </param>
        /// <param name="args">An object array that contains zero or more objects to format. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is null. </exception><exception cref="T:System.FormatException"><paramref name="format"/> is invalid.-or- The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args"/> array. </exception><filterpriority>1</filterpriority>
        public static void MessageFormat(string format, params object[] args)
        {
            Verse.Log.Message(string.Format(format, args));
        }

        public static void WarningFormat(string format, params object[] args)
        {
            Verse.Log.Warning(string.Format(format, args));
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            Verse.Log.Error(string.Format(format, args));
        }

    }
}