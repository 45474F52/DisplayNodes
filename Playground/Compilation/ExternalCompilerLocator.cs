using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace DisplayNodes.Playground.Compilation
{
	/// <summary>
	/// Ищет внешний компилятор C# 4.0+ (csc.exe). Ищем в стандартных
	/// папках .NET Framework 4.x. Если компилятор найден и его версия
	/// >= 4.0, возвращаем путь к нему. Иначе null.
	/// </summary>
	internal static class ExternalCompilerLocator
	{
		public static string FindCsc4()
		{
			string windir = Environment.GetEnvironmentVariable("windir");
			if (string.IsNullOrEmpty(windir))
				return null;

			string[] candidates =
			{
				Path.Combine(windir, @"Microsoft.NET\Framework64\v4.0.30319\csc.exe"),
				Path.Combine(windir, @"Microsoft.NET\Framework\v4.0.30319\csc.exe")
			};

			foreach (string path in candidates)
			{
				if (!File.Exists(path))
					continue;

				if (IsCscVersionAtLeast4(path))
					return path;
			}

			return null;
		}

		private static bool IsCscVersionAtLeast4(string cscPath)
		{
			try
			{
				var psi = new ProcessStartInfo(cscPath, "/?")
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				};

				using (var process = Process.Start(psi))
				{
					string output = process.StandardOutput.ReadToEnd();
					string err = process.StandardError.ReadToEnd();
					process.WaitForExit(2000);

					string combined = output + err;

					// Пример строки: "Microsoft (R) Visual C# Compiler version 4.0.30319.17929"
					// Или: "Microsoft (R) Visual C# 2010 Compiler version 4.0.30319.1"
					// Или для новых: "Microsoft (R) Visual C# Compiler version 4.8.4084.0"
					Match m = Regex.Match(combined, @"version\s+(\d+)\.(\d+)");
					if (!m.Success)
						return false;

					int major = int.Parse(m.Groups[1].Value);
					return major >= 4;
				}
			}
			catch
			{
				return false;
			}
		}
	}
}
