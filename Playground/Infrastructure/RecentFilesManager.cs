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
using System.IO;
using System.Text;

namespace DisplayNodes.Playground.Infrastructure
{
	/// <summary>
	/// Хранит список недавно открытых/сохранённых файлов.
	/// Файл лежит в %APPDATA%\DisplayNodesPlayground\recent.txt, одна строка — один путь.
	/// </summary>
	internal sealed class RecentFilesManager
	{
		private readonly string _storageFile;
		private readonly List<string> _items = new List<string>();
		private readonly int _maxEntries;

		public RecentFilesManager(EditorSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			_maxEntries = settings.RecentFilesMax;

			string dir = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"DisplayNodesPlayground");

			try
			{
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}
			catch (Exception ex) { AppLog.Error("RecentFilesManager.ctor", ex); }

			_storageFile = Path.Combine(dir, "recent.txt");
			Load();
		}

		/// <summary>Список путей, от самых новых к самым старым.</summary>
		public IList<string> Items => _items.AsReadOnly();

		/// <summary>
		/// Добавить путь наверх. Существующие вхождения поднимаются наверх.
		/// Пустой список — не проблема.
		/// </summary>
		public void Push(string path)
		{
			if (string.IsNullOrEmpty(path))
				return;

			// Нормализуем путь — убираем относительные сегменты.
			try
			{
				path = Path.GetFullPath(path);
			}
			catch (Exception ex) { AppLog.Error("RecentFilesManager.Push", ex); }

			// Убираем существующие вхождения (без учёта регистра).
			for (int i = _items.Count - 1; i >= 0; i--)
			{
				if (string.Equals(_items[i], path, StringComparison.OrdinalIgnoreCase))
					_items.RemoveAt(i);
			}

			_items.Insert(0, path);

			while (_items.Count > _maxEntries)
				_items.RemoveAt(_items.Count - 1);

			Save();
		}

		/// <summary>Убрать путь из списка (например, если файл больше не существует).</summary>
		public void Remove(string path)
		{
			for (int i = _items.Count - 1; i >= 0; i--)
			{
				if (string.Equals(_items[i], path, StringComparison.OrdinalIgnoreCase))
					_items.RemoveAt(i);
			}
			Save();
		}

		/// <summary>Очистить список.</summary>
		public void Clear()
		{
			_items.Clear();
			Save();
		}

		// ---------- IO ----------

		private void Load()
		{
			_items.Clear();

			try
			{
				if (!File.Exists(_storageFile))
					return;

				string[] lines = File.ReadAllLines(_storageFile, Encoding.UTF8);
				foreach (string line in lines)
				{
					if (!string.IsNullOrEmpty(line) && _items.Count < _maxEntries)
						_items.Add(line);
				}
			}
			catch (Exception ex) { AppLog.Error("RecentFilesManager.Load", ex); }
		}

		private void Save()
		{
			try
			{
				var sb = new StringBuilder();
				foreach (string item in _items)
					sb.AppendLine(item);

				File.WriteAllText(_storageFile, sb.ToString(), Encoding.UTF8);
			}
			catch (Exception ex) { AppLog.Error("RecentFilesManager.Save", ex); }
		}
	}
}
