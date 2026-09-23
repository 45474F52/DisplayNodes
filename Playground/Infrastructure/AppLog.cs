///////////////////////////////////////////////////////////////////////////
//
// Copyright 2026 AES
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
///////////////////////////////////////////////////////////////////////////

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
