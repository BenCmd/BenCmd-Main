using System;
using System.Net;
using System.Net.Sockets;

namespace BenCmdMainServer.Web
{
	public class WebServer
	{
		public TcpListener listen;
		
		public WebServer()
		{
			listen = new TcpListener(IPAddress.Any, 8080);
			listen.Start(); // Start listening for web requests
		}
		
		public void Tick()
		{
		}
	}
}

