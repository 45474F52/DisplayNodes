using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

using DisplayNodes.Playground.Infrastructure;

using Microsoft.CSharp;

namespace DisplayNodes.Playground.Compilation
{
	/// <summary>
	/// Компилятор на базе встроенного CSharpCodeProvider (C# 3.0, .NET 3.5).
	/// Не поддерживает именованные и опциональные аргументы.
	/// </summary>
	internal sealed class BuiltInCompiler : ICompiler, IDisposable
	{
		private readonly CSharpCodeProvider _provider;
		private readonly ErrorParser _errorParser;

		public BuiltInCompiler(ErrorParser errorParser)
		{
			_errorParser = errorParser ?? throw new ArgumentNullException("errorParser");
			_provider = new CSharpCodeProvider(new Dictionary<string, string>
			{
				["CompilerVersion"] = "v3.5"
			});
		}

		public CompileResult Compile(string wrappedCode, string[] referencedAssemblies)
		{
			var parameters = new CompilerParameters
			{
				GenerateInMemory = false,
				TreatWarningsAsErrors = false,
				IncludeDebugInformation = false,
				OutputAssembly = Path.Combine(Path.GetTempPath(), "Playground_" + Guid.NewGuid().ToString("N") + ".dll")
			};

			foreach (string asm in referencedAssemblies)
				_ = parameters.ReferencedAssemblies.Add(asm);

			try
			{
				CompilerResults results = _provider.CompileAssemblyFromSource(parameters, wrappedCode);

				if (results.Errors.HasErrors)
				{
					return new CompileResult
					{
						Success = false,
						ErrorOutput = _errorParser.FormatBuiltIn(results)
					};
				}

				byte[] bytes = File.ReadAllBytes(results.CompiledAssembly.Location);
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
					if (File.Exists(parameters.OutputAssembly))
						File.Delete(parameters.OutputAssembly);
				}
				catch (Exception ex)
				{
					AppLog.Error("BuiltInCompiler.Compile", ex);
				}
			}
		}

		public void Dispose()
		{
			_provider?.Dispose();
		}
	}
}
