using System;
using System.Collections.Generic;
using System.IO;

namespace BenCmdMainServer.Control
{
	public class BCBanlistController
	{
		private static BCBanlistController _instance;

		public static BCBanlistController Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new BCBanlistController();
				}
				return _instance;
			}
		}
		
		private List<BCBanlist> blists;

		public List<BCBanlist> Banlists { get { return blists; } }
		
		public BCBanlistController()
		{
			blists = new List<BCBanlist>();
		}
		
		public void LoadAll()
		{
			foreach (string file in Directory.GetFiles("banlists"))
			{
				if (file.EndsWith(".bcban"))
				{
					string toLoad = file.Split(Path.DirectorySeparatorChar)[file.Split(Path.DirectorySeparatorChar).Length - 1];
					toLoad = toLoad.Remove(toLoad.LastIndexOf('.'));
					blists.Add(LoadBanlist(toLoad));
				}
			}
		}
		
		private BCBanlist LoadBanlist(string name)
		{
			StreamReader r = new StreamReader("banlists/" + name + ".bcban");
			string line = r.ReadLine();
			bool read = (line.Split('|')[0] == "true");
			bool write = (line.Split('|')[1] == "true");
			r.ReadLine();
			List<BCBanlistEntry> entries = new List<BCBanlistEntry>();
			while (!r.EndOfStream && (line = r.ReadLine()) != "=== SERVER METADATA ===")
			{
				BCBanlistEntry entry = new BCBanlistEntry();
				entry.AdminSender = BCUserFile.UserFile.GetUser(line.Split('|')[0]);
				entry.ServerSender = ServerTokenFile.TokenFile.GetServer(line.Split('|')[1]);
				int d = 0;
				Int32.TryParse(line.Split('|')[2], out d);
				entry.DemeritImpact = d;
				entry.BannedUser = BCUserFile.UserFile.GetUser(line.Split('|')[3]);
				byte a = 0;
				Byte.TryParse(line.Split('|')[4], out a);
				entry.Appealed = (BCBanlistEntry.AppealStatus)a;
				long t = DateTime.Now.Ticks;
				Int64.TryParse(line.Split('|')[5], out t);
				entry.TimeIssued = new DateTime(t);
				entries.Add(entry);
			}
			Dictionary<BCServer, int> trust = new Dictionary<BCServer, int>();
			List<BCServer> sub = new List<BCServer>();
			while (!r.EndOfStream)
			{
				line = r.ReadLine();
				bool subbed = (line.Split('|')[0] == "true");
				int trustlvl = -1;
				Int32.TryParse(line.Split('|')[1], out trustlvl);
				BCServer server = ServerTokenFile.TokenFile.GetServer(line.Split('|')[2]);
				if (trustlvl != -1)
				{
					trust.Add(server, trustlvl);
				}
				if (subbed)
				{
					sub.Add(server);
				}
			}
			return new BCBanlist(entries, sub, trust, read, write, name);
		}
		
		public void SaveBanlist(BCBanlist list)
		{
			StreamWriter w = new StreamWriter("banlists/" + list.Name + ".bcban");
			w.WriteLine(((list.AllowsPublicRead) ? "true" : "false") + "|" + ((list.AllowsPublicWrite) ? "true" : "false"));
			w.WriteLine("=== BAN ENTRIES ===");
			foreach (BCBanlistEntry entry in list.Entries)
			{
				w.WriteLine(entry.AdminSender.UserName + "|" + entry.ServerSender.AuthenticationToken + "|" + entry.DemeritImpact + "|" + entry.BannedUser.UserName + "|" + ((byte)entry.Appealed) + "|" + entry.TimeIssued.Ticks);
			}
			w.WriteLine("=== SERVER METADATA ===");
			List<BCServer> done = new List<BCServer>();
			foreach (KeyValuePair<BCServer, int> server in list.ServerTrustLevel)
			{
				done.Add(server.Key);
				w.WriteLine(((list.SubscribedServers.Contains(server.Key)) ? "true" : "false") + "|" + server.Value + "|" + server.Key.AuthenticationToken);
			}
			foreach (BCServer server in list.SubscribedServers)
			{
				if (!done.Contains(server))
				{
					done.Add(server);
					w.WriteLine("true|-1|" + server.ServerName);
				}
			}
		}
	}
}

