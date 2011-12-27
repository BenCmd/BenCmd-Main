using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using BenCmdMainServer.Control;
using BenCmdMainServer.Net;

namespace BenCmdMainServer
{
	public class MainServer : IDisposable
	{
		private TcpListener listen;

		public List<BenCmdServer> Servers { get; private set; }

		private bool _disposed = false;

		public DateTime NextPing { get; private set; }
		
		public MainServer(int port)
		{
			listen = new TcpListener(IPAddress.Any, port);
			listen.Start(); // Start listening for BenCmd servers
			Servers = new List<BenCmdServer>();
			NextPing = DateTime.Now;
			ServerTokenFile.TokenFile.Load();
			BCUserFile.UserFile.Load();
			BCBanlistController.Instance.LoadAll();
		}
		
		~MainServer()
		{
			try
			{
				if (!_disposed)
				{
					Dispose(); // Close all connections to servers
				}
			}
			catch (Exception)
			{
			}
		}
		
		/// <summary>
		/// Closes all server connections used by the <see cref="BenCmdMainServer.MainServer"/> object.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="BenCmdMainServer.MainServer"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="BenCmdMainServer.MainServer"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="BenCmdMainServer.MainServer"/> so
		/// the garbage collector can reclaim the memory that the <see cref="BenCmdMainServer.MainServer"/> was occupying.
		/// </remarks>
		public void Dispose()
		{
			if (!_disposed)
			{
				listen.Stop(); // Stop listening for new connections
				foreach (BenCmdServer s in Servers)
				{
					s.Close(); // Close existing connections
				}
				Servers.Clear();
				_disposed = true;
			}
		}
		
		/// <summary>
		/// Tick this instance. Listens for new connections and deals with any instances of <see cref="BenCmdMainServer.Net.Packet" />
		/// that are pending for clients.
		/// </summary>
		/// <exception cref='ObjectDisposedException'>
		/// Is thrown when an operation is performed on a disposed object.
		/// </exception>
		public void Tick()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("MainServer");
			}
			TickNew();
			TickExisting();
			if (DateTime.Now > NextPing)
			{
				PingAll();
			}
		}
		
		private void TickNew()
		{
			if (listen.Pending())
			{
				BenCmdServer s = new BenCmdServer(listen.AcceptTcpClient());
				Servers.Add(s);
				Logger.Log(Logger.Level.Info, "Server request accepted from " + s.IP);
			}
		}
		
		private void TickExisting()
		{
			int i = 0;
			while (i < Servers.Count)
			{
				if (Servers[i].Tick())
				{
					i++;
				}
				else
				{
					Servers.RemoveAt(i);
				}
			}
		}
		
		private void PingAll()
		{
			foreach (BenCmdServer s in Servers)
			{
				s.SendPacket(new PacketStatusPing());
			}
			NextPing = DateTime.Now.AddSeconds(15);
		}
	}
}

