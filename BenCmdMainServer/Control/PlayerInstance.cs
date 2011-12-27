using System;
using System.Collections.Generic;

namespace BenCmdMainServer.Control
{
	public class PlayerInstance
	{
		private static Dictionary<string, PlayerInstance> players = new Dictionary<string, PlayerInstance>();
		
		public static bool HasInstance(string player)
		{
			return players.ContainsKey(player);
		}
		
		public static PlayerInstance GetInstance(string player)
		{
			if (HasInstance(player))
			{
				return players[player];
			}
			else
			{
				PlayerInstance i = new PlayerInstance(BCUserFile.UserFile.GetUser(player));
				players.Add(player, i);
				return i;
			}
		}
		
		public static void DestroyInstance(string player)
		{
			if (HasInstance(player))
			{
				players.Remove(player);
			}
		}
		
		public BCUser User { get; private set; }
		
		public string UserName
		{
			get { return User.UserName; }
		}
		
		public PlayerInstance(BCUser user)
		{
			User = user;
		}
	}
}

