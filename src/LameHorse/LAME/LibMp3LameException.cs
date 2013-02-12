using System;

namespace LameHorse.LAME
{
    /// <remarks>
    /// Originally from http://sixfeetsix.blogspot.co.uk/2012/04/managed-code-to-interact-with.html
    /// </remarks>
	public class LibMp3LameException : Exception
	{
		public LibMp3LameException(string message)
			: base(message)
		{
		}

		public LibMp3LameException(string message, LibMp3LameException innerException)
			: base(message, innerException)
		{
		}
	}
}