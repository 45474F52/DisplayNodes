using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using DisplayNodes.Playground.Infrastructure;

namespace DisplayNodes.Playground.Compilation
{
	/// <summary>
	/// Компилятор на базе внешнего csc.exe 4.0+.
	/// Поддерживает именованные и опциональные аргументы.
	/// </summary>
	internal sealed class ExternalCscCompiler : ICompiler
	{
		private readonly string _cscPath;
		private readonly ErrorParser _errorParser;

		public ExternalCscCompiler(string cscPath, ErrorParser errorParser)
		{
			_cscPath = cscPath ?? throw new ArgumentNullException("cscPath");
			_errorParser = errorParser ?? throw new ArgumentNullException("errorParser");
		}

		public CompileResult Compile(string wrappedCode, string[] referencedAssemblies)
		{
			string tempDir = Path.Combine(
				Path.GetTempPath(),
				"Playground_" + Guid.NewGuid().ToString("N"));
			string sourceFile = Path.Combine(tempDir, "UserScript.cs");
			string outputFile = Path.Combine(tempDir, "UserScript.dll");

			try
			{
				_ = Directory.CreateDirectory(tempDir);
				File.WriteAllText(sourceFile, wrappedCode);

				string arguments = BuildCscArguments(sourceFile, outputFile, referencedAssemblies);

				var psi = new ProcessStartInfo(_cscPath, arguments)
				{
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					StandardOutputEncoding = Encoding.GetEncoding(866),
					StandardErrorEncoding = Encoding.GetEncoding(866)
				};

				string stdout;
				string stderr;
				int exitCode;

				using (var process = Process.Start(psi))
				{
					stdout = process.StandardOutput.ReadToEnd();
					stderr = process.StandardError.ReadToEnd();

					const int timeoutMs = 30_000;

					if (!process.WaitForExit(timeoutMs))
					{
						try
						{
							process.Kill();
						}
						catch (Exception ex)
						{
							AppLog.Error("ExternalCscCompiler.Compile", ex);
						}

						return new CompileResult
						{
							Success = false,
							ErrorOutput = "Компиляция прервана по таймауту (30 с)."
						};
					}

					exitCode = process.ExitCode;
				}

				if (exitCode != 0 || !File.Exists(outputFile))
				{
					string error = _errorParser.FormatExternal(stdout + Environment.NewLine + stderr);
					if (string.IsNullOrEmpty(error))
						error = "Компиляция не удалась (код выхода " + exitCode + ").";

					return new CompileResult
					{
						Success = false,
						ErrorOutput = error
					};
				}

				byte[] bytes = File.ReadAllBytes(outputFile);
				return new CompileResult
				{
					Success = true,
					AssemblyBytes = bytes
				};
			}
			finally
			{
				try
				{
					if (Directory.Exists(tempDir))
						Directory.Delete(tempDir, true);
				}
				catch (Exception ex)
				{
					AppLog.Error("ExternalCscCompiler.Compile", ex);
				}
			}
		}

		private static string BuildCscArguments(string sourceFile, string outputFile, string[] referencedAssemblies)
		{
			var args = new List<string>();
			args.Add("/target:library");
			args.Add("/out:" + Quote(outputFile));
			args.Add("/nostdlib");
			args.Add("/noconfig");
			args.Add("/nologo");
			args.Add("/optimize-");
			args.Add("/debug-");

			foreach (string asm in referencedAssemblies)
				args.Add("/reference:" + Quote(asm));

			args.Add(Quote(sourceFile));

			return string.Join(" ", args.ToArray());
		}

		private static string Quote(string value)
		{
			if (value == null)
				return "\"\"";
			return "\"" + value.Replace("\"", "\\\"") + "\"";
		}
	}
}
