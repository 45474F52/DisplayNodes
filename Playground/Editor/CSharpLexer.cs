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

using DisplayNodes.Playground.Infrastructure;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DisplayNodes.Playground.Editor
{
	internal enum TokenType
	{
		Keyword,
		ControlFlow,
		Type,
		Interface,
		Method,
		Struct,
		String,
		Comment,
		Number,
		Preprocessor
	}

	internal readonly struct Token
	{
		public readonly int Start;
		public readonly int Length;
		public readonly TokenType Type;

		public Token(int start, int length, TokenType type)
		{
			Start = start;
			Length = length;
			Type = type;
		}
	}

	internal static class CSharpLexer
	{
		private static readonly HashSet<string> _keywords = new HashSet<string>
		{
			"abstract","as","base","bool","break","byte","case","catch","char",
			"checked","class","const","continue","decimal","default","delegate",
			"do","double","else","enum","event","explicit","extern","false",
			"finally","fixed","float","for","foreach","goto","if","implicit",
			"in","int","interface","internal","is","lock","long","namespace",
			"new","null","object","operator","out","override","params","private",
			"protected","public","readonly","ref","return","sbyte","sealed",
			"short","sizeof","stackalloc","static","string","struct","switch",
			"this","throw","true","try","typeof","uint","ulong","unchecked",
			"unsafe","ushort","using","var","virtual","void","volatile","while",
			"add","get","set","value","yield","async","await","dynamic",
		};

		private static readonly HashSet<string> _controlFlow = new HashSet<string>
		{
			"if","else","for","foreach","while","do","switch","case","break",
			"continue","return","throw","try","catch","finally","goto","yield",
		};

		private static readonly HashSet<string> _knownStructs = new HashSet<string>
		{
			"Point","Point2D","Size","Size2D","Color","DateTime","TimeSpan","Guid",
			"Rectangle","Thickness","Rect","GridLength","MainAxisAlignment","Alignment",
		};

		private static readonly string _hexDigits = "0123456789abcdefABCDEF";

		public static List<Token> Tokenize(string text)
		{
			var tokens = new List<Token>();
			if (string.IsNullOrEmpty(text))
				return tokens;

			int n = text.Length;
			int i = 0;

			while (i < n)
			{
				char c = text[i];

				// -------- // комментарий --------
				if (c == '/' && i + 1 < n && text[i + 1] == '/')
				{
					int start = i;
					while (i < n && text[i] != '\n')
						i++;
					tokens.Add(new Token(start, i - start, TokenType.Comment));
					continue;
				}

				// -------- /* комментарий */ --------
				if (c == '/' && i + 1 < n && text[i + 1] == '*')
				{
					int start = i;
					i += 2;
					while (i + 1 < n && !(text[i] == '*' && text[i + 1] == '/'))
						i++;
					if (i + 1 < n)
						i += 2;
					else
						i = n;
					tokens.Add(new Token(start, i - start, TokenType.Comment));
					continue;
				}

				// -------- @"verbatim" --------
				if (c == '@' && i + 1 < n && text[i + 1] == '"')
				{
					int start = i;
					i += 2;
					while (i < n)
					{
						if (text[i] == '"')
						{
							if (i + 1 < n && text[i + 1] == '"')
							{ i += 2; continue; }
							i++;
							break;
						}
						i++;
					}
					tokens.Add(new Token(start, i - start, TokenType.String));
					continue;
				}

				// -------- "строка" --------
				if (c == '"')
				{
					int start = i;
					i++;
					while (i < n && text[i] != '"' && text[i] != '\n')
					{
						if (text[i] == '\\' && i + 1 < n)
							i++;
						i++;
					}
					if (i < n && text[i] == '"')
						i++;
					tokens.Add(new Token(start, i - start, TokenType.String));
					continue;
				}

				// -------- 'символ' --------
				if (c == '\'')
				{
					int start = i;
					i++;
					while (i < n && text[i] != '\'' && text[i] != '\n')
					{
						if (text[i] == '\\' && i + 1 < n)
							i++;
						i++;
					}
					if (i < n && text[i] == '\'')
						i++;
					tokens.Add(new Token(start, i - start, TokenType.String));
					continue;
				}

				// -------- #препроцессор --------
				if (c == '#' && (i == 0 || text[i - 1] == '\n' || text[i - 1] == '\r'))
				{
					int start = i;
					while (i < n && text[i] != '\n')
						i++;
					tokens.Add(new Token(start, i - start, TokenType.Preprocessor));
					continue;
				}

				// -------- число --------
				if (char.IsDigit(c))
				{
					int start = i;
					if (c == '0' && i + 1 < n && (text[i + 1] == 'x' || text[i + 1] == 'X'))
					{
						i += 2;
						while (i < n && (_hexDigits.IndexOf(text[i]) >= 0 || text[i] == '_'))
							i++;
					}
					else
					{
						while (i < n && (char.IsDigit(text[i]) || text[i] == '.' || text[i] == '_'))
							i++;
					}
					while (i < n && "fFdDmMlLuU".IndexOf(text[i]) >= 0)
						i++;
					tokens.Add(new Token(start, i - start, TokenType.Number));
					continue;
				}

				// -------- идентификатор / ключевое слово --------
				if (char.IsLetter(c) || c == '_')
				{
					int start = i;
					while (i < n && (char.IsLetterOrDigit(text[i]) || text[i] == '_'))
						i++;
					string word = text.Substring(start, i - start);

					TokenType type;
					if (_controlFlow.Contains(word))
					{
						type = TokenType.ControlFlow;
					}
					else if (_keywords.Contains(word))
					{
						type = TokenType.Keyword;
					}
					else if (_knownStructs.Contains(word))
					{
						type = TokenType.Struct;
					}
					else if (char.IsUpper(word[0]))
					{
						// Является ли следующий непробельный символ точкой?
						// Если да — текущий идентификатор это namespace (или промежуточный
						// сегмент вложенного типа). Не подсвечиваем как Type.
						bool followedByDot = IsFollowedBy(text, i, '.');

						if (IsPrecededBy(text, start, '.'))
						{
							// Идентификатор после точки.
							if (IsPartOfNewQualifiedName(text, start))
							{
								// Мы в контексте new Namespace.Type(...)
								// Промежуточные сегменты (с точкой справа) — не подсвечиваем.
								// Последний (без точки справа) — Type.
								if (followedByDot)
									continue;

                                type = ApiIndex.IsInterface(word) ? TokenType.Interface : TokenType.Type;
                            }
                            else
                            {
                                // Обычный член после точки.
                                // Порядок проверок важен: сначала метод (перед '('), потом тип.
                                // Иначе UI.Font / UI.Brush (методы) будут подсвечены как типы,
                                // потому что "Font" и "Brush" совпадают с именами типов из BCL.
                                if (IsFollowedByParen(text, i))
                                {
                                    type = TokenType.Method;
                                }
                                else if (ApiIndex.KnowsType(word))
                                {
                                    type = ApiIndex.IsInterface(word) ? TokenType.Interface : TokenType.Type;
                                }
                                else
                                {
                                    // Не тип, не метод — либо свойство (обычный цвет), либо namespace.
                                    continue;
                                }
                            }
                        }
						else if (IsPrecededByWord(text, start, "new"))
						{
							// После new: если справа точка — это namespace, обычный цвет.
							// Если справа "(" или что-то ещё — это Type.
							if (followedByDot)
								continue;

							type = TokenType.Type;
						}
						else if (IsFollowedByParen(text, i))
						{
							type = TokenType.Method;
						}
						else if (followedByDot)
						{
							// Идентификатор перед точкой, не после new. Если известен как тип —
							// подсветить как Type. Если неизвестен — не трогать.
							if (ApiIndex.KnowsType(word))
                                type = ApiIndex.IsInterface(word) ? TokenType.Interface : TokenType.Type;
                            else
								continue;
						}
						else
						{
							if (ApiIndex.KnowsType(word))
                                type = ApiIndex.IsInterface(word) ? TokenType.Interface : TokenType.Type;
                            else
								continue;
						}
					}
					else
					{
						continue;
					}

					tokens.Add(new Token(start, i - start, type));
					continue;
				}

				i++;
			}

			return tokens;
		}

        /// <summary>
        /// Пропустить пробелы вперёд и проверить, что там символ c.
        /// </summary>
        private static bool IsFollowedBy(string text, int pos, char c)
		{
			int j = pos;
			while (j < text.Length && (text[j] == ' ' || text[j] == '\t'))
				j++;
			return j < text.Length && text[j] == c;
		}

		/// <summary>
		/// Возвращает идентификатор, стоящий слева от точки, которая
		/// находится непосредственно перед позицией dotPos.
		/// </summary>
		private static string GetIdentifierBeforeDot(string text, int dotPos)
		{
			int j = dotPos - 1;
			while (j >= 0 && (text[j] == ' ' || text[j] == '\t'))
				j--;
			if (j < 0 || text[j] != '.')
				return null;
			j--;

			while (j >= 0 && (text[j] == ' ' || text[j] == '\t'))
				j--;
			int end = j + 1;
			while (j >= 0 && (char.IsLetterOrDigit(text[j]) || text[j] == '_'))
				j--;
			int start = j + 1;

			if (end <= start)
				return null;
			return text.Substring(start, end - start);
		}

		/// <summary>
		/// Проверяет, что идентификатор является сегментом qualified name,
		/// стоящего сразу после "new". Например: new A.B.C(...) — для A, B, C
		/// вернёт true. Работает в том числе когда перед "new" стоит пробел.
		/// </summary>
		private static bool IsPartOfNewQualifiedName(string text, int pos)
		{
			int j = pos - 1;
			while (j >= 0 && (text[j] == ' ' || text[j] == '\t'))
				j--;
			if (j < 0)
				return false;

			// Идём назад по цепочке идентификаторов, разделённых точками.
			while (j >= 0)
			{
				// Отматываем идентификатор.
				if (!char.IsLetterOrDigit(text[j]) && text[j] != '_')
					break;
				while (j >= 0 && (char.IsLetterOrDigit(text[j]) || text[j] == '_'))
					j--;

				// Пропускаем пробелы перед ним.
				while (j >= 0 && (text[j] == ' ' || text[j] == '\t'))
					j--;

				// Есть точка — цепочка продолжается.
				if (j >= 0 && text[j] == '.')
				{
					j--;
					while (j >= 0 && (text[j] == ' ' || text[j] == '\t'))
						j--;
					continue;
				}

				// Дошли до начала цепочки.
				break;
			}

			// Прямо перед началом цепочки должно стоять слово "new".
			return IsWordEndingAt(text, j, "new");
		}

		/// <summary>
		/// Проверяет, что слово word заканчивается точно на позиции end.
		/// </summary>
		private static bool IsWordEndingAt(string text, int end, string word)
		{
			int j = end;
			while (j >= 0 && (text[j] == ' ' || text[j] == '\t'))
				j--;
			if (j < 0)
				return false;

			int wEnd = j + 1;
			while (j >= 0 && (char.IsLetterOrDigit(text[j]) || text[j] == '_'))
				j--;
			int wStart = j + 1;

			if (wEnd - wStart != word.Length)
				return false;
			return string.CompareOrdinal(text, wStart, word, 0, word.Length) == 0;
		}

		// Пропустить пробелы/табы назад и проверить, что там символ c.
		private static bool IsPrecededBy(string text, int pos, char c)
		{
			int j = pos - 1;
			while (j >= 0 && (text[j] == ' ' || text[j] == '\t'))
				j--;
			return j >= 0 && text[j] == c;
		}

		// Пропустить пробелы/табы назад и проверить, что там целое слово word.
		private static bool IsPrecededByWord(string text, int pos, string word)
		{
			int j = pos - 1;
			while (j >= 0 && (text[j] == ' ' || text[j] == '\t'))
				j--;
			if (j < 0)
				return false;
			int end = j + 1;
			while (j >= 0 && (char.IsLetterOrDigit(text[j]) || text[j] == '_'))
				j--;
			int startIdx = j + 1;
			if (end - startIdx != word.Length)
				return false;
			return string.CompareOrdinal(text, startIdx, word, 0, word.Length) == 0;
		}

		// Пропустить пробелы вперёд и проверить, что там '('.
		private static bool IsFollowedByParen(string text, int pos)
		{
			int j = pos;
			while (j < text.Length && (text[j] == ' ' || text[j] == '\t'))
				j++;
			return j < text.Length && text[j] == '(';
		}
	}
}
