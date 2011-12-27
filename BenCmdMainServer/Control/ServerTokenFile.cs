using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BenCmdMainServer.Control
{
	public class ServerTokenFile : IEnumerable<BCServer>
	{
		private static ServerTokenFile _file = null;

		public static ServerTokenFile TokenFile
		{
			get
			{
				if (_file == null)
				{
					return _file = new ServerTokenFile();
				}
				else
				{
					return _file;
				}
			}
		}
		
		private Dictionary<string, BCServer> servers;
		
		private ServerTokenFile()
		{
			servers = new Dictionary<string, BCServer>();
		}
		
		public void Load()
		{
			servers.Clear();
			if (!File.Exists("servers.db"))
			{
				File.CreateText("servers.db").Close();
				return;
			}
			StreamReader r = File.OpenText("servers.db");
			while (!r.EndOfStream)
			{
				string v = r.ReadLine();
				if (v == "")
				{
					continue;
				}
				string t = v.Split('|')[0];
				Dictionary<BCUser, int> a = new Dictionary<BCUser, int>();
				foreach (string s in v.Split('|')[1].Split(','))
				{
					int ul = 0;
					Int32.TryParse(s.Split(':')[1], out ul);
					a.Add(BCUserFile.UserFile.GetUser(s.Split(':')[0]), ul);
				}
				string n = v.Split(new char[] { '|' }, 3)[2];
				servers.Add(t, new BCServer(t, n));
			}
			r.Close();
		}
		
		public void Save()
		{
			StreamWriter w = File.CreateText("servers.db");
			foreach (BCServer s in this)
			{
				w.Write(s.AuthenticationToken + "|");
				bool first = true;
				foreach (KeyValuePair<BCUser, int> a in s.Administrators)
				{
					if (first)
					{
						first = false;
					}
					else
					{
						w.Write(",");
					}
					w.Write(a.Key.UserName + ":" + a.Value);
				}
			}
			w.Close();
		}
		
		public bool TokenExists(string token)
		{
			foreach (KeyValuePair<string, BCServer> s in servers)
			{
				if (s.Key == token)
				{
					return true;
				}
			}
			return false;
		}
		
		public BCServer GetServer(string token)
		{
			return servers[token];
		}
		
		#region Enumeration
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return servers.Values.GetEnumerator();
		}
		
		public IEnumerator<BCServer> GetEnumerator()
		{
			return servers.Values.GetEnumerator();
		}
		
		#endregion
	}
}

