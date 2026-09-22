using System;
using System.IO;

namespace DisplayNodes.Playground.Infrastructure
{
	internal static class AppLog
	{
		private static readonly string _logFile;

		static AppLog()
		{
			try
			{
				string dir = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"DisplayNodesPlayground");

				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);

				_logFile = Path.Combine(dir, "error.log");
			}
			catch
			{
				_logFile = null;
			}
		}

		public static void Error(string context, Exception ex)
		{
			if (_logFile == null)
				return;

			try
			{
				string line = string.Format(
					"[{0:O}] {1}: {2}{3}",
					DateTime.Now, context, ex, Environment.NewLine);

				File.AppendAllText(_logFile, line);
			}
			catch { }
		}
	}
}
