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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DisplayNodes.Playground.Infrastructure
{
	/// <summary>
	/// Хранилище состояния приложения: геометрия окна, сплиттеры,
	/// последний файл, флаги. Файл — простой key=value в %APPDATA%.
	/// </summary>
	internal sealed class AppState
	{
		private readonly string _storageFile;
		private readonly Dictionary<string, string> _values =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public AppState()
		{
			string dir = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"DisplayNodesPlayground");

			try
			{
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}
			catch (Exception ex) { AppLog.Error("AppState.ctor", ex); }

			_storageFile = Path.Combine(dir, "state.txt");
			Load();
		}

		// ---------- типизированный доступ ----------

		public string GetString(string key, string defaultValue = null)
			=> _values.TryGetValue(key, out string v) ? v : defaultValue;

		public int GetInt(string key, int defaultValue)
		{
			if (_values.TryGetValue(key, out string v))
			{
				if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
					return result;
			}
			return defaultValue;
		}

		public bool GetBool(string key, bool defaultValue)
		{
			if (_values.TryGetValue(key, out string v))
			{
				if (bool.TryParse(v, out bool result))
					return result;
			}
			return defaultValue;
		}

		public void SetString(string key, string value)
		{
			if (value == null)
				_values.Remove(key);
			else
				_values[key] = value;
		}

		public void SetInt(string key, int value)
		{
			_values[key] = value.ToString(CultureInfo.InvariantCulture);
		}

		public void SetBool(string key, bool value)
		{
			_values[key] = value ? "True" : "False";
		}

		/// <summary>Удалить ключ (например, если файл больше не существует).</summary>
		public void Remove(string key)
		{
			_values.Remove(key);
		}

		// ---------- IO ----------

		public void Save()
		{
			try
			{
				var sb = new StringBuilder();
				foreach (var kv in _values)
				{
					sb.Append(kv.Key);
					sb.Append('=');
					sb.Append(kv.Value);
					sb.AppendLine();
				}

				File.WriteAllText(_storageFile, sb.ToString(), Encoding.UTF8);
			}
			catch (Exception ex) { AppLog.Error("AppState.Save", ex); }
		}

		private void Load()
		{
			try
			{
				if (!File.Exists(_storageFile))
					return;

				foreach (string line in File.ReadAllLines(_storageFile, Encoding.UTF8))
				{
					if (line.IsNullOrWhiteSpace())
						continue;
					if (line.StartsWith("#"))
						continue;

					int eq = line.IndexOf('=');
					if (eq <= 0)
						continue;

					string key = line.Substring(0, eq).Trim();
					string value = line.Substring(eq + 1);

					if (key.Length > 0)
						_values[key] = value;
				}
			}
			catch (Exception ex) { AppLog.Error("AppState.Load", ex); }
		}
	}
}
