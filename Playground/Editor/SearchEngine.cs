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
using System.Text;
using System.Text.RegularExpressions;

namespace DisplayNodes.Playground.Editor
{
	/// <summary>
	/// Чистая логика поиска и замены в тексте.
	/// Не зависит от UI или RichTextBox.
	/// </summary>
	internal sealed class SearchEngine
	{
		/// <summary>
		/// Найти следующее вхождение query в text, начиная с позиции start.
		/// Возвращает индекс начала совпадения или -1, если не найдено.
		/// Возвращает -2, если regex невалиден.
		/// </summary>
		public int FindNext(string text, string query, int start, bool regex, bool matchCase, bool forward)
		{
			if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(text))
				return -1;

			try
			{
				if (regex)
					return RegexFind(text, query, start, matchCase, forward);
				return PlainFind(text, query, start, matchCase, forward);
			}
			catch (ArgumentException)
			{
				return -2; // bad regex
			}
		}

		/// <summary>
		/// Заменить все вхождения query на replacement в text.
		/// Возвращает новый текст и количество замен через out-параметр.
		/// </summary>
		public string ReplaceAll(string text, string query, string replacement, bool regex, bool matchCase, out int count)
		{
			count = 0;
			if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(text))
				return text;

			if (regex)
			{
				var re = new Regex(query, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
				count = re.Matches(text).Count;
				return re.Replace(text, replacement);
			}

			var sb = new StringBuilder();
			int pos = 0;
			var cmp = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

			while (pos < text.Length)
			{
				int idx = text.IndexOf(query, pos, cmp);
				if (idx < 0)
					break;
				sb.Append(text, pos, idx - pos);
				sb.Append(replacement);
				pos = idx + query.Length;
				count++;
			}
			sb.Append(text, pos, text.Length - pos);
			return sb.ToString();
		}

		private static int PlainFind(string text, string query, int start, bool matchCase, bool forward)
		{
			var cmp = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

			if (forward)
			{
				int idx = text.IndexOf(query, Math.Min(start, text.Length), cmp);
				return idx;
			}
			else
			{
				int limit = Math.Min(start, text.Length);
				int last = -1;
				int pos = 0;
				while (pos < limit)
				{
					int idx = text.IndexOf(query, pos, cmp);
					if (idx < 0)
						break;
					if (idx + query.Length <= limit)
						last = idx;
					pos = idx + 1;
				}
				return last;
			}
		}

		private static int RegexFind(string text, string pattern, int start, bool matchCase, bool forward)
		{
			var re = new Regex(pattern, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);

			if (forward)
			{
				var m = re.Match(text, Math.Min(start, text.Length));
				return m.Success ? m.Index : -1;
			}

			int limit = Math.Min(start, text.Length);
			Match last = Match.Empty;
			foreach (Match m in re.Matches(text))
			{
				if (m.Index + m.Length > limit)
					break;
				last = m;
			}
			return last.Success ? last.Index : -1;
		}
	}
}
