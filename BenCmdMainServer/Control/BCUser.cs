using System;

namespace BenCmdMainServer.Control
{
	public class BCUser
	{
		private bool dev;

		public bool IsDeveloper
		{
			get
			{
				return dev;
			}
			
			set
			{
				dev = value;
				Save();
			}
		}
		
		private string duplicate;

		public string DuplicateOf
		{
			get
			{
				return duplicate;
			}
			
			set
			{
				duplicate = value;
				Save();
			}
		}
		
		public string UserName { get; private set; }
		
		public BCUser(string name, bool dev, string duplicate)
		{
			this.dev = dev;
			this.duplicate = duplicate;
			UserName = name;
		}
		
		public void Save()
		{
			BCUserFile.UserFile.Save();
		}
	}
}

