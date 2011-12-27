using System;

namespace BScript
{
	public abstract class File
	{
		public abstract void Append(string message);
		public abstract void Read();
	}
	
	public class ScriptFile : File
	{
		public System.IO.StreamReader Stream { get; private set; }
		
		public ScriptFile (System.IO.StreamReader r)
		{
			Stream = r;
		}
		
		public override void Append (string message)
		{
			throw new InvalidOperationException("Cannot write to script file!");
		}
		
		public override void Read()
		{
		}
	}
}

