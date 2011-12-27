using System;
using System.Collections.Generic;

namespace BenCmdMainServer.Control
{
	public class BCServer
	{
		private string auth;

		public string AuthenticationToken
		{
			get
			{
				return auth;
			}
			
			set
			{
				auth = value;
				Save();
			}
		}
		
		private string name;

		public string ServerName
		{
			get
			{
				return name;
			}
			
			set
			{
				name = value;
				Save();
			}
		}
		
		private Dictionary<BCUser, int> admins;

		public Dictionary<BCUser, int> Administrators
		{
			get
			{
				return admins;
			}
			
			set
			{
				admins = value;
				Save();
			}
		}
		
		public List<BCBanlist> Subscribed
		{
			get
			{
				List<BCBanlist> l = new List<BCBanlist>();
				return l;
				//foreach (BCBanlist list in 
			}
		}
		
		public BCServer(string token, string name)
		{
			auth = token;
			this.name = name;
		}
		
		public void Save()
		{
			ServerTokenFile.TokenFile.Save();
		}
	}
}

