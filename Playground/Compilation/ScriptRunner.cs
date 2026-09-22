using System;

using DisplayNodes.Core;
using DisplayNodes.Playground.Infrastructure;

namespace DisplayNodes.Playground.Compilation
{
	/// <summary>
	/// Оркестратор компиляции и выполнения скриптов.
	/// Выбирает компилятор (встроенный или внешний), делегирует
	/// компиляцию ICompiler, парсинг ошибок — ErrorParser,
	/// вызов Build() — ScriptInvoker.
	/// </summary>
	internal sealed class ScriptRunner : IDisposable
	{
		private readonly ICompiler _compiler;
		private readonly ScriptInvoker _invoker;
		private readonly bool _usesExternalCompiler;
		private readonly string[] _referencedAssemblies;

		public ScriptRunner(EditorSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			// Ссылки на сборки — одинаковы для обоих компиляторов.
			_referencedAssemblies = new[]
			{
				typeof(object).Assembly.Location, // mscorelib Assembly
				typeof(System.Drawing.Color).Assembly.Location, // System.Drawing Assembly
				typeof(System.Linq.Enumerable).Assembly.Location, // LINQ Assembly
				typeof(LayoutNode).Assembly.Location, // DisplayNodes Kernel Assembly
				typeof(LibDisplayDrawing.IComponent).Assembly.Location, // LibDisplayDrawing Assembly
				typeof(DisplayNodes.Gdi.GdiFont).Assembly.Location, // GDI Conversions Assembly
			};

			var errorParser = new ErrorParser(wrapperLineOffset: settings.WrapperLineOffset);
			_invoker = new ScriptInvoker();

			// Ищем внешний компилятор C# 4.0+ (csc.exe). Если он есть,
			// используем его — тогда скрипты поддерживают именованные и
			// опциональные аргументы. Если нет — падаем на встроенный v3.5.
			string externalCscPath = ExternalCompilerLocator.FindCsc4();
			if (externalCscPath != null)
			{
				_compiler = new ExternalCscCompiler(externalCscPath, errorParser);
				_usesExternalCompiler = true;
			}
			else
			{
				_compiler = new BuiltInCompiler(errorParser);
				_usesExternalCompiler = false;
			}
		}

		/// <summary>
		/// true, если используется внешний компилятор C# 4.0+.
		/// false, если встроенный v3.5 (тогда именованные аргументы недоступны).
		/// </summary>
		public bool UsesExternalCompiler => _usesExternalCompiler;

		public LayoutNode Execute(string code, out string error)
		{
			string wrappedCode = BuildWrappedCode(code);
			CompileResult result = _compiler.Compile(wrappedCode, _referencedAssemblies);

			if (!result.Success)
			{
				error = result.ErrorOutput;
				return null;
			}

			return _invoker.Invoke(result.AssemblyBytes, out error);
		}

		private string BuildWrappedCode(string code)
		{
			return ScriptWrapper.GetWrappedCode(code);
		}

		public void Dispose()
		{
			if (_compiler is IDisposable disposable)
				disposable.Dispose();
		}
	}
}
