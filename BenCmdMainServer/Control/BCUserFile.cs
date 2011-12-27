using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BenCmdMainServer.Control
{
	public class BCUserFile : IEnumerable<BCUser>
	{
		private static BCUserFile _file = null;

		public static BCUserFile UserFile
		{
			get
			{
				if (_file == null)
				{
					return _file = new BCUserFile();
				}
				else
				{
					return _file;
				}
			}
		}
		
		private Dictionary<string, BCUser> users;
		
		private BCUserFile()
		{
			users = new Dictionary<string, BCUser>();
		}
		
		public bool UserExists(string user)
		{
			return users.ContainsKey(user);
		}
		
		public BCUser GetUser(string user)
		{
			user = user.ToLower();
			if (UserExists(user))
			{
				return users[user];
			}
			else
			{
				BCUser u = new BCUser(user, false, "");
				users.Add(user, u);
				Save();
				return u;
			}
		}
		
		public void Load()
		{
			users.Clear();
			if (!File.Exists("bcuser.db"))
			{
				File.CreateText("bcuser.db").Close();
				return;
			}
			StreamReader r = File.OpenText("bcuser.db");
			while (!r.EndOfStream)
			{
				string v = r.ReadLine();
				if (v == "")
				{
					continue;
				}
				string n = v.Split(',')[0];
				string d = v.Split(',')[1];
				if (d == ".")
				{
					d = "";
				}
				bool dev = v.Split(',')[2] == "true";
				users.Add(n, new BCUser(n, dev, d));
			}
			r.Close();
		}
		
		public void Save()
		{
			StreamWriter w = File.CreateText("bcuser.db");
			foreach (BCUser u in this)
			{
				w.WriteLine(u.UserName + "," + ((u.DuplicateOf == "") ? "." : u.DuplicateOf) + "," + ((u.IsDeveloper) ? "true" : "false"));
			}
			w.Close();
		}
		
		#region Enumeration
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return users.Values.GetEnumerator();
		}
		
		public IEnumerator<BCUser> GetEnumerator()
		{
			return users.Values.GetEnumerator();
		}
		
		#endregion
	}
}

