#define LOGDBG
#define LOGDEEP
using System;
using System.IO;

namespace BenCmdMainServer
{
	public static class Logger
	{
		
		private static StreamWriter stream = null;
		
		/// <summary>
		/// Initialize the logger for use.
		/// </summary>
		public static void Init()
		{
			try
			{
				if (File.Exists("access.log")) // Check if a log file already exists
				{
					stream = File.AppendText("access.log");
				}
				else
				{
					stream = File.CreateText("access.log");
				}
				Log(Level.Info, "Loaded access.log...");
			}
			catch (Exception e)
			{
				stream = null;
				Log(Level.Error, "Failed to load access.log for writing:");
				Log(Level.Error, e.ToString());
			}
		}
		
		/// <summary>
		/// Logs a message to the console as well as to access.log, if
		/// <see cref="Init" /> has successfully run.
		/// </summary>
		/// <param name='l'>
		/// The level at which to log the message.
		/// </param>
		/// <param name='m'>
		/// The message to log.
		/// </param>
		public static void Log(Level l, String m)
		{
#if LOGDEEP
#else
			if (l == Level.Deep)
			{
				return;
			}
#endif
#if LOGDBG
#else
			if (l == Level.Debug)
			{
				return;
			}
#endif
			DateTime n = DateTime.Now;
			m = fmt(n.Hour, 2) + ":" + fmt(n.Minute, 2) + ":" + fmt(n.Second, 2) + " [" + l.GetShorthand() + "] " + m; // Format the message
			Console.WriteLine(m); // Write the message to the console
			if (stream != null)
			{
				stream.WriteLine(fmt(n.Month, 2) + "/" + fmt(n.Day, 2) + "/" + fmt(n.Year, 4) + " " + m); // Save the message to the log, if it exists
				stream.Flush();
			}
		}
		
		private static String fmt(int i, int l)
		{
			string r = i.ToString();
			while (r.Length < l)
			{
				r = "0" + r;
			}
			return r;
		}
		
		public enum Level
		{
			Deep,
			Debug,
			Info,
			Warning,
			Error
		}
		
		public static String GetShorthand(this Level level)
		{
			switch (level)
			{
				case Level.Deep:
					return "DEEP";
				case Level.Debug:
					return "DBG";
				case Level.Info:
					return "INFO";
				case Level.Warning:
					return "WARN";
				case Level.Error:
					return "ERROR";
				default:
					return "UNKNOWN";
			}
		}
	}
}

