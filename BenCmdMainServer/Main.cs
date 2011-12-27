using System;
using System.Net.Sockets;

using BenCmdMainServer.Control;

namespace BenCmdMainServer
{
	public static class MainClass
	{
		public static MainServer ms;
		public static bool tick = true;
		
		public static void Main(string[] args)
		{
#if DEBUG
			args = new string[] { "-t" };
#endif
			if (args.Length >= 1 && (args[0] == "-t" || args[0] == "--token"))
			{
				Console.WriteLine(TokenGenerator.GenerateToken());
				Console.ReadKey();
				return;
			}
			Logger.Init(); // Initialize the logger
			try
			{
				ms = new MainServer(2392); // Bind main BenCmd server listener to port 2392
				Logger.Log(Logger.Level.Info, "Main server bound to port 2392!");
			}
			catch (SocketException e)
			{
				Logger.Log(Logger.Level.Error, "Failed to bind main server to port 2392:");
				Logger.Log(Logger.Level.Error, e.ToString());
			}
			while (tick)
			{
				DateTime end = DateTime.Now.AddSeconds(1);
				ms.Tick();
				while (DateTime.Now < end)
				{
				}
			}
		}
	}
}
