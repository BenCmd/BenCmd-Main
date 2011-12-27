using System;
using System.Collections.Generic;

namespace BenCmdMainServer.Control
{
	public class BCBanlist
	{
		private List<BCServer> subscribed;

		public List<BCServer> SubscribedServers
		{
			get
			{
				return subscribed;
			}
			
			set
			{
				subscribed = value;
				Save();
			}
		}
		
		private Dictionary<BCServer, int> servertrust;

		public Dictionary<BCServer, int> ServerTrustLevel
		{
			get
			{
				return servertrust;
			}
			
			set
			{
				servertrust = value;
				Save();
			}
		}
		
		private bool publicWrite;

		public bool AllowsPublicWrite
		{
			get
			{
				return publicWrite;
			}
			
			set
			{
				publicWrite = value;
				Save();
			}
		}
		
		private bool publicRead;

		public bool AllowsPublicRead
		{
			get
			{
				return publicRead;
			}
			
			set
			{
				publicRead = value;
				Save();
			}
		}
		
		private List<BCBanlistEntry> entries;

		public List<BCBanlistEntry> Entries
		{
			get
			{
				return entries;
			}
			
			set
			{
				entries = value;
				Save();
			}
		}
		
		public string Name { get; private set; }
		
		public BCBanlist(List<BCBanlistEntry> entries, List<BCServer> subscribed, Dictionary<BCServer, int> trust, bool read, bool write, string name)
		{
			this.entries = entries;
			this.subscribed = subscribed;
			this.servertrust = trust;
			this.publicRead = read;
			this.publicWrite = write;
			this.Name = name;
		}
		
		public void Save()
		{
			BCBanlistController.Instance.SaveBanlist(this);
		}
	}
}

