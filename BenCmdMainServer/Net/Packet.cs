using System;
using System.Collections.Generic;

namespace BenCmdMainServer.Net
{
	public enum PacketType
	{
		AuthRequest = 0,		// R-
		AuthResponse = 1,		// -W
		PlayerJoin = 2,			// R-
		PlayerLeave = 3,		// R-
		GlobalBan = 4,			// RW
		DuplicateUser = 5,		// R-
		UserStatsRequest = 6,	// R-
		UserStatsResponse = 7,	// -W
		BanlistInfo = 8,		// RW
		BanlistSubscribe = 9,	// R-
		BanlistUnsubscribe = 10,// R-
		KickUser = 11,			// -W
		LogMessage = 12,		// R-
		StatusPing = 254,		// RW
		Disconnect = 255		// RW
	}
	
	public static class PacketMap
	{
		public static readonly Dictionary<PacketType, Type> Packets = new Dictionary<PacketType, Type>
		{
			{ PacketType.AuthRequest, typeof(PacketAuthRequest) },
			{ PacketType.AuthResponse, typeof(PacketAuthResponse) },
			{ PacketType.PlayerJoin, typeof(PacketPlayerJoin) },
			{ PacketType.PlayerLeave, typeof(PacketPlayerLeave) },
			{ PacketType.GlobalBan, typeof(PacketGlobalBan) },
			{ PacketType.DuplicateUser, typeof(PacketDuplicateUser) },
			{ PacketType.UserStatsRequest, typeof(PacketUserStatsRequest) },
			{ PacketType.UserStatsResponse, typeof(PacketUserStatsResponse) },
			{ PacketType.BanlistInfo, typeof(PacketBanlistInfo) },
			{ PacketType.BanlistSubscribe, typeof(PacketBanlistSubscribe) },
			{ PacketType.KickUser, typeof(PacketKickUser) },
			{ PacketType.LogMessage, typeof(PacketLogMessage) },
			{ PacketType.BanlistUnsubscribe, typeof(PacketBanlistUnsubscribe) },
			{ PacketType.StatusPing, typeof(PacketStatusPing) },
			{ PacketType.Disconnect, typeof(PacketDisconnect) }
		};
	}
	
	public abstract class Packet
	{
		/// <summary>
		/// The packet's unique ID.
		/// </summary>
		public readonly byte PacketID;
		
		/// <summary>
		/// Whether this packet can be accepted incoming from a
		/// server.
		/// </summary>
		public readonly bool CanRead;
		
		/// <summary>
		/// Whether this packet can be sent to a server.
		/// </summary>
		public readonly bool CanWrite;
		
		protected Packet(byte id, bool r, bool w)
		{
			PacketID = id;
			CanRead = r;
			CanWrite = w;
		}
		
		/// <summary>
		/// Read packet data from the specified client.
		/// </summary>
		/// <param name='client'>
		/// The client to read data from.
		/// </param>
		/// <exception cref='InvalidOperationException'>
		/// Is thrown when a write-only package is read.
		/// </exception>
		public virtual void Read(BenCmdServer client)
		{
			throw new InvalidOperationException("Cannot read this packet type (ID: " + PacketID + ")");
		}
		
		/// <summary>
		/// Send packet data to the specified client.
		/// </summary>
		/// <param name='client'>
		/// The client to write data to.
		/// </param>
		/// <exception cref='InvalidOperationException'>
		/// Is thrown when a read-only package is written.
		/// </exception>
		public virtual void Write(BenCmdServer client)
		{
			throw new InvalidOperationException("Cannot write this packet type (ID: " + PacketID + ")");
		}
	}
	
	public class PacketAuthRequest : Packet
	{
		/// <summary>
		/// Gets the auth token sent by the remote server to verify its identity.
		/// </summary>
		/// <value>
		/// The auth token sent.
		/// </value>
		public string AuthToken { get; private set; }
		
		/// <summary>
		/// Gets the BenCmd build that the remote server is using.
		/// </summary>
		/// <value>
		/// The build ID.
		/// </value>
		public int BCBuild { get; private set; }
		
		public PacketAuthRequest()
			: base(0, true, false)
		{
			AuthToken = "";
			BCBuild = 0;
		}
		
		public override void Read(BenCmdServer client)
		{
			AuthToken = client.ReadString(52);
			BCBuild = client.ReadInt();
		}
	}
	
	public class PacketAuthResponse : Packet
	{
		/// <summary>
		/// Sets the response to send back to the server.
		/// </summary>
		/// <value>
		/// The reply to send to the server.
		/// </value>
		public AuthResponse Response { get; set; }
		
		public PacketAuthResponse()
			: base(1, false, true)
		{
			Response = AuthResponse.Failed;
		}
		
		public override void Write(BenCmdServer client)
		{
			client.Write((byte)Response);
		}
		
		public enum AuthResponse : byte
		{
			Accepted = 0,
			InvalidToken = 1,
			ServerBlocked = 2,
			DoubleConnect = 3,
			OutdatedServer = 4,
			Failed = 5
		}
	}
	
	public class PacketPlayerJoin : Packet
	{
		/// <summary>
		/// Gets the name of the <see cref="BenCmdMainServer.BCUser"/>
		/// who just connected.
		/// </summary>
		/// <value>
		/// The name of the <see cref="BenCmdMainServer.BCUser"/>.
		/// </value>
		public string PlayerName { get; private set; }
		
		public PacketPlayerJoin()
			: base(2, true, false)
		{
			PlayerName = "";
		}
		
		public override void Read(BenCmdServer client)
		{
			PlayerName = client.ReadString(16);
		}
	}
	
	public class PacketPlayerLeave : Packet
	{
		/// <summary>
		/// Gets the name of the <see cref="BenCmdMainServer.BCUser"/>
		/// who just disconnected.
		/// </summary>
		/// <value>
		/// The name of the <see cref="BenCmdMainServer.BCUser"/>.
		/// </value>
		public string PlayerName { get; private set; }
		
		/// <summary>
		/// Gets the reason that the <see cref="BenCmdMainServer.BCUser"/>
		/// has disconnected from the server.
		/// </summary>
		/// <value>
		/// The reason.
		/// </value>
		public LeaveReason Reason { get; private set; }
		
		public PacketPlayerLeave()
			: base(3, true, false)
		{
			PlayerName = "";
			Reason = LeaveReason.Quit;
		}
		
		public override void Read(BenCmdServer client)
		{
			PlayerName = client.ReadString(16);
			Reason = (LeaveReason)client.ReadByte();
		}
		
		public enum LeaveReason : byte
		{
			Quit = 0,
			Error = 1,
			Kick = 2
		}
	}
	
	public class PacketGlobalBan : Packet
	{
		public string UserName { get; set; }

		public long Time { get; set; }

		public string Banlist { get; set; }

		public string Reason { get; set; }

		public string Banner { get; set; }
		
		public PacketGlobalBan()
			: base(4, true, false)
		{
			UserName = "";
			Time = 0;
			Banlist = "";
			Reason = "";
			Banner = "";
		}
		
		public override void Read(BenCmdServer client)
		{
			UserName = client.ReadString(16);
			Time = client.ReadLong();
			Banlist = client.ReadString(16);
			Reason = client.ReadString(30);
			Banner = client.ReadString(16);
		}
	}
	
	public class PacketDuplicateUser : Packet
	{
		public string User1 { get; set; }

		public string User2 { get; set; }

		public string Reporter { get; set; }
		
		public PacketDuplicateUser()
			: base(5, true, false)
		{
			User1 = "";
			User2 = "";
			Reporter = "";
		}
		
		public override void Read(BenCmdServer client)
		{
			User1 = client.ReadString(16);
			User2 = client.ReadString(16);
			Reporter = client.ReadString(16);
		}
	}
	
	public class PacketUserStatsRequest : Packet
	{
		public string User { get; set; }
		
		public PacketUserStatsRequest()
			: base(6, true, false)
		{
			User = "";
		}
		
		public override void Read(BenCmdServer client)
		{
			User = client.ReadString(16);
		}
	}
	
	public class PacketUserStatsResponse : Packet
	{
		public bool IsDuplicate { get; set; }

		public short BanDemerits { get; set; }

		public bool IsDeveloper { get; set; }

		public string CurrentServer { get; set; }
		
		public PacketUserStatsResponse()
			: base(7, false, true)
		{
			IsDuplicate = false;
			BanDemerits = 0;
			IsDeveloper = false;
			CurrentServer = "";
		}
		
		public override void Write(BenCmdServer client)
		{
			client.Write(IsDuplicate);
			client.Write(BanDemerits);
			client.Write(IsDeveloper);
			client.Write(CurrentServer);
		}
	}
	
	public class PacketBanlistInfo : Packet
	{
		public string Banlist { get; set; }

		public string Description { get; set; }

		public bool IsSubscribed { get; set; }

		public bool Exists { get; set; }

		public bool Approved { get; set; }
		
		public PacketBanlistInfo()
			: base(8, true, true)
		{
			Banlist = "";
			Description = "";
			IsSubscribed = false;
			Exists = false;
		}
		
		public override void Read(BenCmdServer client)
		{
			Banlist = client.ReadString(16);
		}
		
		public override void Write(BenCmdServer client)
		{
			client.Write(Banlist);
			client.Write(Description);
			client.Write(IsSubscribed);
			client.Write(Exists);
			client.Write(Approved);
		}
	}
	
	public class PacketBanlistSubscribe : Packet
	{
		public string Banlist { get; set; }
		
		public PacketBanlistSubscribe()
			: base(9, true, false)
		{
			Banlist = "";
		}
		
		public override void Read(BenCmdServer client)
		{
			Banlist = client.ReadString(16);
		}
	}
	
	public class PacketBanlistUnsubscribe : Packet
	{
		public string Banlist { get; set; }
		
		public PacketBanlistUnsubscribe()
			: base(10, true, false)
		{
			Banlist = "";
		}
		
		public override void Read(BenCmdServer client)
		{
			Banlist = client.ReadString(16);
		}
	}
	
	public class PacketKickUser : Packet
	{
		public string User { get; set; }
		
		public PacketKickUser()
			: base(11, false, true)
		{
			User = "";
		}
		
		public override void Write(BenCmdServer client)
		{
			client.Write(User);
		}
	}
	
	public class PacketLogMessage : Packet
	{
		public string Message { get; set; }

		public LogLevel Level { get; set; }
		
		public PacketLogMessage()
			: base(12, true, false)
		{
			Message = "";
			Level = LogLevel.INFO;
		}
		
		public override void Read(BenCmdServer client)
		{
			Message = client.ReadString(200);
			Level = (LogLevel)client.ReadSByte();
		}
		
		public enum LogLevel : sbyte
		{
			SEVERE = 1,
			WARNING = 9,
			INFO = 8,
			CONFIG = 7,
			FINE = 5,
			FINER = 4,
			FINEST = 3
		}
	}
	
	public class PacketStatusPing : Packet
	{
		public PacketStatusPing()
			: base(254, true, true)
		{
		}
		
		public override void Read(BenCmdServer client)
		{
			// Do nothing
		}
		
		public override void Write(BenCmdServer client)
		{
			// Do nothing
		}
	}
	
	public class PacketDisconnect : Packet
	{
		/// <summary>
		/// Gets or sets the reason for which the client is being disconnected/is disconnecting.
		/// </summary>
		/// <value>
		/// The reason that the client is disconnecting.
		/// </value>
		public string Reason { get; set; }
		
		public PacketDisconnect()
			: base(255, true, true)
		{
			Reason = "";
		}
		
		public override void Read(BenCmdServer client)
		{
			Reason = client.ReadString(50);
		}
		
		public override void Write(BenCmdServer client)
		{
			client.Write(Reason);
		}
	}
}

