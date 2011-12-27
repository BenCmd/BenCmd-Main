using System;

namespace BenCmdMainServer.Control
{
	public struct BCBanlistEntry
	{
		private BCUser asend;

		public BCUser AdminSender { get { return asend; } set { asend = value; } }
		
		private BCServer ssend;

		public BCServer ServerSender { get { return ssend; } set { ssend = value; } }
		
		private int impact;

		public int DemeritImpact { get { return impact; } set { impact = value; } }
		
		private BCUser banned;

		public BCUser BannedUser { get { return banned; } set { banned = value; } }
		
		private AppealStatus appeal;

		public AppealStatus Appealed { get { return appeal; } set { appeal = value; } }
		
		private string reason;

		public string Reason { get { return reason; } set { reason = value; } }
		
		private long time;

		public DateTime TimeIssued { get { return new DateTime(time); } set { time = value.Ticks; } }
		
		public enum AppealStatus : byte
		{
			NotStarted = 0,
			Started = 1,
			Reviewed = 2,
			AwaitingEvidence = 3,
			AwaitingJudgement = 4,
			Failed = 5,
			Appealed = 6
		}
	}
}

