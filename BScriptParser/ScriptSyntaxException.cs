using System;

namespace BScript
{
	public class ScriptSyntaxException : Exception
	{
		public ScriptSyntaxException(string message)
			: base(message)
		{
		}
	}
}

