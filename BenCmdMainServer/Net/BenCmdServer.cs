using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

using BenCmdMainServer.Control;

namespace BenCmdMainServer.Net
{
	public class BenCmdServer
	{
		private const int MinimumBCBuild = 41;
		
		/// <summary>
		/// Gets the TCP object representing the attached server.
		/// </summary>
		/// <value>
		/// The server connection.
		/// </value>
		public TcpClient Connection { get; private set; }
		
		/// <summary>
		/// Gets the remote server's IP Address.
		/// </summary>
		/// <value>
		/// The IP Address of the remote server.
		/// </value>
		public string IP { get; private set; }
		
		/// <summary>
		/// Gets the token of the server, if it is authenticated.
		/// </summary>
		/// <value>
		/// The token of the server if authenticated, <c>null</c> otherwise.
		/// </value>
		public BCServer Identity { get; private set; }
		
		public List<PlayerInstance> OnlinePlayers { get; private set; }
		
		public StreamWriter LogFile { get; private set; }
		
		private DateTime timeout;
		
		public BenCmdServer(TcpClient client)
		{
			IP = client.Client.RemoteEndPoint.ToString();
			Connection = client;
			Identity = null;
			OnlinePlayers = new List<PlayerInstance>();
			LogFile = null;
			timeout = DateTime.Now.AddSeconds(5);
		}
		
		#region Tick handling
		
		public bool Tick()
		{
			if (!CheckForPackets())
			{
				return false;
			}
			if (!CheckTimeout())
			{
				return false;
			}
			return true;
		}
		
		private bool CheckTimeout()
		{
			if (DateTime.Now >= timeout)
			{
				Disconnect("Timed out!");
				return false;
			}
			return true;
		}
		
		private bool CheckForPackets()
		{
			try
			{
				while (Connection.Available != 0)
				{
					PacketType t;
					try
					{
						t = (PacketType)ReadByte();
					}
					catch (Exception)
					{
						Disconnect("Invalid packet ID!");
						return false;
					}
					Packet p;
					try
					{
						p = (Packet)PacketMap.Packets[t].GetConstructor(new Type[0]).Invoke(new object[0]);
					}
					catch (Exception)
					{
						Disconnect("Packet not implemented yet!");
						return false;
					}
					if (!p.CanRead)
					{
						Disconnect("Attempt to send read-only packet!");
						return false;
					}
					p.Read(this);
					try
					{
						return RecvPacket(p);
					}
					catch (Exception e)
					{
						Disconnect(e.Message);
						return false;
					}
				}
				return true;
			}
			catch (ObjectDisposedException)
			{
				if (Identity == null)
				{
					Logger.Log(Logger.Level.Info, IP + " has disconnected unexpectedly...");
				}
				else
				{
					Logger.Log(Logger.Level.Info, Identity.ServerName + " has disconnected unexpectedly...");
				}
				return false;
			}
		}
		
		#endregion
		
		#region Packet receiving
		
		public bool RecvPacket(Packet p)
		{
			try
			{
				if (p is PacketAuthRequest)
				{
					return RecvPacket(p as PacketAuthRequest);
				}
				else if (p is PacketPlayerJoin)
				{
					return RecvPacket(p as PacketPlayerJoin);
				}
				else if (p is PacketPlayerLeave)
				{
					return RecvPacket(p as PacketPlayerLeave);
				}
				else if (p is PacketLogMessage)
				{
					return RecvPacket(p as PacketLogMessage);
				}
				else if (p is PacketStatusPing)
				{
					return RecvPacket(p as PacketStatusPing);
				}
				else if (p is PacketDisconnect)
				{
					return RecvPacket(p as PacketDisconnect);
				}
				else
				{
					Disconnect("Packet not implemented yet!");
					return false;
				}
			}
			catch (Exception e)
			{
				if (Identity == null)
				{
					Logger.Log(Logger.Level.Error, "Failed to receive packet from " + IP + ":");
				}
				else
				{
					Logger.Log(Logger.Level.Error, "Failed to receive packet from " + Identity.ServerName + ":");
				}
				Logger.Log(Logger.Level.Error, e.ToString());
				try
				{
					PacketDisconnect d = new PacketDisconnect();
					d.Reason = "Internal server error!";
					Write(d.PacketID);
					d.Write(this);
				}
				catch (Exception)
				{
				}
				Close();
				return false;
			}
		}
		
		public bool RecvPacket(PacketAuthRequest p)
		{
			if (Identity != null)
			{
				Disconnect("Attempt to authenticate while already authenticated!");
				return false;
			}
			if (p.BCBuild < MinimumBCBuild)
			{
				Logger.Log(Logger.Level.Info, IP + " has been disconnected: Outdated server");
				SendPacket(new PacketAuthResponse()
				{
					Response = PacketAuthResponse.AuthResponse.OutdatedServer
				});
				Close();
				return false;
			}
			if (p.AuthToken.Length != 52)
			{
				Logger.Log(Logger.Level.Info, IP + " has been disconnected: Invalid token");
				SendPacket(new PacketAuthResponse()
				{
					Response = PacketAuthResponse.AuthResponse.InvalidToken
				});
				Close();
				return false;
			}
			if (!ServerTokenFile.TokenFile.TokenExists(p.AuthToken))
			{
				Logger.Log(Logger.Level.Info, IP + " has been disconnected: Invalid token");
				SendPacket(new PacketAuthResponse()
				{
					Response = PacketAuthResponse.AuthResponse.InvalidToken
				});
				Close();
				return false;
			}
			BCServer bcs = ServerTokenFile.TokenFile.GetServer(p.AuthToken);
			foreach (BenCmdServer s in MainClass.ms.Servers)
			{
				if (s.Identity == bcs)
				{
					Logger.Log(Logger.Level.Info, IP + " has been disconnected: Token in use");
					SendPacket(new PacketAuthResponse()
					{
						Response = PacketAuthResponse.AuthResponse.DoubleConnect
					});
					Close();
					return false;
				}
			}
			Identity = bcs;
			LogFile = File.AppendText("log/" + Identity.ServerName + ".log");
			Logger.Log(Logger.Level.Info, IP + " has successfully authenticated as \"" + Identity.ServerName + "\"");
			timeout = MainClass.ms.NextPing.AddSeconds(30);
			SendPacket(new PacketAuthResponse()
			{
				Response = PacketAuthResponse.AuthResponse.Accepted
			});
			return true;
		}
		
		public bool RecvPacket(PacketPlayerJoin p)
		{
			if (Identity == null)
			{
				Disconnect("Cannot do that without authenticating first!");
				return false;
			}
			if (IsPlayerOnline(p.PlayerName))
			{
				Disconnect("Two of the same player on server!");
				return false;
			}
			OnlinePlayers.Add(PlayerInstance.GetInstance(p.PlayerName));
			return true;
		}
		
		public bool RecvPacket(PacketPlayerLeave p)
		{
			if (Identity == null)
			{
				Disconnect("Cannot do that without authenticating first!");
				return false;
			}
			for (int i = 0; i < OnlinePlayers.Count; i++)
			{
				if (OnlinePlayers[i].UserName == p.PlayerName)
				{
					OnlinePlayers.RemoveAt(i);
					return true;
				}
			}
			Disconnect("Non-existant user disconnected?");
			return false;
		}
		
		public bool RecvPacket(PacketUserStatsRequest p)
		{
			if (Identity == null)
			{
				Disconnect("Cannot do that without authenticating first!");
				return false;
			}
			PacketUserStatsResponse response = new PacketUserStatsResponse();
			return true;
		}
		
		public bool RecvPacket(PacketLogMessage p)
		{
			if (Identity == null)
			{
				Disconnect("Cannot do that without authenticating first!");
				return false;
			}
			if (p.Level >= PacketLogMessage.LogLevel.WARNING)
			{
				Logger.Log(Logger.Level.Info, Identity.ServerName + ": [" + p.Level.ToString() + "] " + p.Message);
			}
			DateTime logTime = DateTime.Now;
			LogFile.WriteLine(logTime.ToString() + " [" + p.Level.ToString() + "] " + p.Message);
			return true;
		}
		
		public bool RecvPacket(PacketStatusPing p)
		{
			if (Identity == null)
			{
				Disconnect("Cannot do that without authenticating first!");
				return false;
			}
			timeout = MainClass.ms.NextPing.AddSeconds(30);
			return true;
		}
		
		public bool RecvPacket(PacketDisconnect p)
		{
			Close();
			if (Identity == null)
			{
				Logger.Log(Logger.Level.Info, IP + " has disconnected: " + p.Reason);
			}
			else
			{
				Logger.Log(Logger.Level.Info, Identity.ServerName + " has disconnected: " + p.Reason);
			}
			return false;
		}
		
		#endregion
		
		#region Packet sending
		
		/// <summary>
		/// Sends a packet to the server.
		/// </summary>
		/// <param name='p'>
		/// The packet to send.
		/// </param>
		/// <exception cref='ArgumentException'>
		/// Is thrown when the packet passed in cannot be written to a client.
		/// </exception>
		public void SendPacket(Packet p)
		{
			if (!p.CanWrite)
			{
				throw new ArgumentException("Packet (ID: " + p.PacketID + ") cannot be written!");
			}
			try
			{
				Write(p.PacketID);
				p.Write(this);
			}
			catch (Exception e)
			{
				Logger.Log(Logger.Level.Error, "Failed to send packet to " + IP + ":");
				Logger.Log(Logger.Level.Error, e.ToString());
				try
				{
					PacketDisconnect d = new PacketDisconnect();
					d.Reason = "Internal server error!";
					Write(d.PacketID);
					d.Write(this);
				}
				catch (Exception)
				{
				}
				Close();
			}
		}
		
		/// <summary>
		/// Disconnect this server.
		/// </summary>
		/// <param name='reason'>
		/// The reason to provide the server with.
		/// </param>
		public void Disconnect(String reason)
		{
			if (Identity == null)
			{
				Logger.Log(Logger.Level.Info, IP + " has been disconnected: " + reason);
			}
			else
			{
				Logger.Log(Logger.Level.Info, Identity.ServerName + " has been disconnected: " + reason);
			}
			PacketDisconnect p = new PacketDisconnect();
			p.Reason = reason;
			SendPacket(p);
			Close();
		}
		
		#endregion
		
		#region Stream Read
		
		/// <summary>
		/// Reads all bytes from the stream.
		/// </summary>
		/// <returns>
		/// The bytes.
		/// </returns>
		public byte[] ReadAllBytes()
		{
			byte[] result = new byte[Connection.Available];
			Connection.GetStream().Read(result, 0, Connection.Available);
			return result;
		}
		
		/// <summary>
		/// Reads a single byte from the stream.
		/// </summary>
		/// <returns>
		/// The byte.
		/// </returns>
		public byte ReadByte()
		{
			return (byte)Connection.GetStream().ReadByte();
		}
		
		/// <summary>
		/// Reads a single signed byte from the stream.
		/// </summary>
		/// <returns>
		/// The signed byte.
		/// </returns>
		public sbyte ReadSByte()
		{
			return unchecked((sbyte)ReadByte());
		}
		
		/// <summary>
		/// Reads a short integer from the stream.
		/// </summary>
		/// <returns>
		/// The short integer.
		/// </returns>
		public short ReadShort()
		{
			byte[] b = new byte[2];
			Connection.GetStream().Read(b, 0, 2);
			return unchecked((short)((b[0] << 8) | b[1]));
		}
		
		/// <summary>
		/// Reads a float from the stream.
		/// </summary>
		/// <returns>
		/// The float.
		/// </returns>
		public unsafe float ReadFloat()
		{
			int f = ReadInt();
			return *(float*)&f;
		}
		
		/// <summary>
		/// Reads a double from the stream.
		/// </summary>
		/// <returns>
		/// The double.
		/// </returns>
		public unsafe double ReadDouble()
		{
			byte[] r = new byte[8];
			for (int i = 7; i >= 0; i--)
			{
				r[i] = ReadByte();
			}
			return BitConverter.ToDouble(r, 0);
		}
		
		/// <summary>
		/// Reads an integer from the stream.
		/// </summary>
		/// <returns>
		/// The integer.
		/// </returns>
		public int ReadInt()
		{
			byte[] b = new byte[4];
			Connection.GetStream().Read(b, 0, 4);
			return unchecked((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]);
		}
		
		/// <summary>
		/// Reads a long integer from the stream.
		/// </summary>
		/// <returns>
		/// The long integer.
		/// </returns>
		public long ReadLong()
		{
			byte[] b = new byte[8];
			Connection.GetStream().Read(b, 0, 8);
			return unchecked((b[0] << 56) | (b[1] << 48) | (b[2] << 40) | (b[3] << 32) |
			                 (b[4] << 24) | (b[5] << 16) | (b[6] << 8) | b[7]);
		}
		
		/// <summary>
		/// Reads a unicode 16-bit string from the stream.
		/// </summary>
		/// <returns>
		/// The string.
		/// </returns>
		public string ReadString(short maxLength)
		{
			short length = ReadShort();
			if (length > maxLength)
			{
				throw new IOException("Received string was too long. (" + length + " > " + maxLength + ")");
			}
			byte[] b = new byte[length * 2];
			Connection.GetStream().Read(b, 0, length * 2);
			string s = ASCIIEncoding.BigEndianUnicode.GetString(b);
			return s;
		}
		
		/// <summary>
		/// Reads a boolean from the stream.
		/// </summary>
		/// <returns>
		/// The boolean.
		/// </returns>
		public bool ReadBool()
		{
			return ReadByte() != 0;
		}
		
		#endregion
		
		#region Stream Write
		
		/// <summary>
		/// Writes the specified byte to the stream.
		/// </summary>
		/// <param name='write'>
		/// The byte to write to the stream.
		/// </param>
		public void Write(byte write)
		{
			Connection.GetStream().WriteByte(write);
		}
		
		/// <summary>
		/// Writes the specified signed byte to the stream.
		/// </summary>
		/// <param name='write'>
		/// The signed byte to write to the stream.
		/// </param>
		public void Write(sbyte write)
		{
			Connection.GetStream().WriteByte((byte)write);
		}
		
		/// <summary>
		/// Writes a group of bytes to the stream.
		/// </summary>
		/// <param name='write'>
		/// The bytes to write to the stream.
		/// </param>
		public void Write(byte[] write)
		{
			Connection.GetStream().Write(write, 0, write.Length);
		}
		
		/// <summary>
		/// Writes the specified unicode 16-bit string to the stream.
		/// </summary>
		/// <param name='write'>
		/// The string to write to the stream.
		/// </param>
		public void Write(string write)
		{
			byte[] b = ASCIIEncoding.BigEndianUnicode.GetBytes(write);
			Write((short)write.Length);
			Write(b);
		}
		
		/// <summary>
		/// Writes the specified short integer to the stream.
		/// </summary>
		/// <param name='write'>
		/// The short integer to write to the stream.
		/// </param>
		public void Write(short write)
		{
			Write(unchecked((byte)(write >> 8)));
			Write(unchecked((byte)write));
		}
		
		/// <summary>
		/// Writes the specified integer to the stream.
		/// </summary>
		/// <param name='write'>
		/// The integer to write to the stream.
		/// </param>
		public void Write(int write)
		{
			Write(unchecked((byte)(write >> 24)));
			Write(unchecked((byte)(write >> 16)));
			Write(unchecked((byte)(write >> 8)));
			Write(unchecked((byte)write));
		}
		
		/// <summary>
		/// Writes the specified long integer to the stream.
		/// </summary>
		/// <param name='write'>
		/// The long integer to write to the stream.
		/// </param>
		public void Write(long write)
		{
			Write(unchecked((byte)(write >> 56)));
			Write(unchecked((byte)(write >> 48)));
			Write(unchecked((byte)(write >> 40)));
			Write(unchecked((byte)(write >> 32)));
			Write(unchecked((byte)(write >> 24)));
			Write(unchecked((byte)(write >> 16)));
			Write(unchecked((byte)(write >> 8)));
			Write(unchecked((byte)write));
		}
		
		/// <summary>
		/// Writes the specified double to the stream.
		/// </summary>
		/// <param name='write'>
		/// The double to write to the stream.
		/// </param>
		public unsafe void Write(double write)
		{
			Write(*(long*)&write);
		}
		
		/// <summary>
		/// Writes the specified float to the stream.
		/// </summary>
		/// <param name='write'>
		/// The float to write to the stream.
		/// </param>
		public unsafe void Write(float write)
		{
			Write(*(int*)&write);
		}
		
		/// <summary>
		/// Writes the specified boolean to the stream.
		/// </summary>
		/// <param name='write'>
		/// The boolean to write to the stream.
		/// </param>
		public void Write(bool write)
		{
			Write((byte)((write) ? 1 : 0));
		}
		
		#endregion
		
		public bool IsPlayerOnline(string player)
		{
			foreach (PlayerInstance i in OnlinePlayers)
			{
				if (i.UserName == player)
				{
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Terminates the server connection immediately.
		/// </summary>
		public void Close()
		{
			Connection.Close();
			LogFile.Close();
		}
	}
}

