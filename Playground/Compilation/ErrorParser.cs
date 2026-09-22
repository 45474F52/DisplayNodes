using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DisplayNodes.Playground.Compilation
{
	/// <summary>
	/// Парсит и форматирует ошибки компиляции из разных источников.
	/// </summary>
	internal sealed class ErrorParser
	{
		private readonly int _wrapperLineOffset;

		public ErrorParser(int wrapperLineOffset)
		{
			_wrapperLineOffset = wrapperLineOffset;
		}

		/// <summary>
		/// Парсит вывод внешнего компилятора csc.exe.
		/// Формат: "UserScript.cs(10,20): error CS1002: ; expected"
		/// </summary>
		public string FormatExternal(string output)
		{
			if (string.IsNullOrEmpty(output))
				return null;

			var regex = new Regex(
				@"UserScript\.cs\((\d+),(\d+)\):\s*(error|warning)\s+([A-Z]+\d+):\s*(.+)",
				RegexOptions.IgnoreCase);

			var result = new List<string>();
			string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string line in lines)
			{
				Match m = regex.Match(line);
				if (!m.Success)
					continue;

				int lineNum = int.Parse(m.Groups[1].Value);
				int colNum = int.Parse(m.Groups[2].Value);
				string code = m.Groups[4].Value;
				string message = m.Groups[5].Value.Trim();

				int userLine = lineNum - _wrapperLineOffset;
				if (userLine < 1)
					userLine = 1;

				result.Add(string.Format(
					"Line {0}, Col {1}: {2} ({3})",
					userLine, colNum, message, code));
			}

			if (result.Count == 0)
				return output.Trim();

			return string.Join(Environment.NewLine, result.ToArray());
		}

		/// <summary>
		/// Форматирует ошибки из CompilerResults (встроенный компилятор).
		/// </summary>
		public string FormatBuiltIn(CompilerResults results)
		{
			if (results == null || !results.Errors.HasErrors)
				return null;

			var errorList = new List<string>();
			foreach (CompilerError e in results.Errors)
			{
				int userLine = e.Line - _wrapperLineOffset;
				if (userLine < 1)
					userLine = 1;

				errorList.Add(string.Format(
					"Line {0}, Col {1}: {2} ({3})",
					userLine, e.Column, e.ErrorText, e.ErrorNumber));
			}

			return string.Join(Environment.NewLine, errorList.ToArray());
		}
	}
}
