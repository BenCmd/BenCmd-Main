using System;
using System.Collections.Generic;

namespace BScript
{
	public abstract class Function
	{
		public static Dictionary<string, Type> functions;
		
		public File ScriptFile { get; private set; }
		public VariableController Variables { get; private set; }
		
		public Function(File script, VariableController variables)
		{
			ScriptFile = script;
			Variables = variables;
		}
		
		public abstract object CallFunction();
		public abstract object CallFunction(object arg);
		public abstract object CallFunction(object arg1, object arg2);
	}
	
	public class OutFunction : Function
	{
		public OutFunction(File script, VariableController variables)
			: base(script, variables)
		{
		}
		
		public override object CallFunction()
		{
			throw new ScriptSyntaxException("@out requires 1 argument!");
		}
		
		public override object CallFunction(object arg)
		{
			ScriptFile.Append(arg.ToString());
			return arg.ToString();
		}
		
		public override object CallFunction(object arg1, object arg2)
		{
			throw new ScriptSyntaxException("@out requires 1 argument!");
		}
	}
	
	public class SetFunction : Function
	{
		public SetFunction(File script, VariableController variables)
			: base(script, variables)
		{
		}
		
		public override object CallFunction()
		{
			throw new ScriptSyntaxException("@set requires 2 arguments!");
		}
		
		public override object CallFunction(object arg)
		{
			throw new ScriptSyntaxException("@set requires 2 arguments!");
		}
		
		public override object CallFunction(object arg1, object arg2)
		{
			if (!(arg1 is string)) throw new ScriptSyntaxException("@set requires a variable FIRST!");
			if (!(arg1 as string).Contains("+")) throw new ScriptSyntaxException("Global variables are BANNED!");
			Variables.SetVariable(arg1 as string, Variables.GetVariable("sys+createOnNull").ToString() != "0", arg2);
			return arg2;
		}
	}
}

