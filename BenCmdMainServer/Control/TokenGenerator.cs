using System;
using System.Security.Cryptography;

namespace BenCmdMainServer.Control
{
	public static class TokenGenerator
	{
		public static string GenerateToken()
		{
			RNGCryptoServiceProvider r = new RNGCryptoServiceProvider();
			byte[] buffer = new byte[39];
			r.GetNonZeroBytes(buffer);
			return Convert.ToBase64String(buffer);
		}
	}
}

