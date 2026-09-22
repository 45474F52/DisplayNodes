using System;
using System.IO;

using DisplayNodes.Core;
using DisplayNodes.Helpers;
using DisplayNodes.Playground.Compilation;

namespace DisplayNodes.Playground.Infrastructure
{
	/// <summary>
	/// Бизнес-логика приложения: компиляция, загрузка/сохранение файлов,
	/// управление текущим LayoutNode и состоянием.
	/// Не знает о UI (Form, PictureBox, ListBox).
	/// </summary>
	internal sealed class AppViewModel : IDisposable
	{
		public LayoutNode CurrentRoot { get; private set; }

		private bool _isDirty;
		public bool IsDirty
		{
			get => _isDirty;
			set
			{
				if (value != _isDirty)
				{
					_isDirty = value;
					OnIsDirtyChanged(value);
				}
			}
		}

		public string CurrentFilePath { get; private set; }

		private readonly AppSettings _settings;
		public AppSettings Settings => _settings;

		private readonly RecentFilesManager _recentFiles;
		public RecentFilesManager RecentFiles => _recentFiles;

		private readonly ScriptRunner _runner;
		public ScriptRunner Runner => _runner;


		public event Action<bool> OnIsDirtyChanged = delegate { };

		public AppViewModel(AppSettings settings, RecentFilesManager recentFiles, ScriptRunner runner)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_recentFiles = recentFiles ?? throw new ArgumentNullException(nameof(recentFiles));
			_runner = runner ?? throw new ArgumentNullException(nameof(runner));
		}

		/// <summary>
		/// Скомпилировать и выполнить код. Возвращает результат.
		/// Старый CurrentRoot автоматически уничтожается.
		/// </summary>
		public CompileResult Run(string code)
		{
			DisposeCurrentRoot();

			LayoutNode root = _runner.Execute(code, out string error);

			var result = new CompileResult();
			result.ErrorOutput = error;
			result.Success = string.IsNullOrEmpty(error);

			if (result.Success)
			{
				CurrentRoot = root;
			}

			return result;
		}

		/// <summary>
		/// Сохранить код в файл. Добавляет файл в recent.
		/// </summary>
		public bool Save(string path, string code)
		{
			try
			{
				File.WriteAllText(path, code);
				_recentFiles.Push(path);
				CurrentFilePath = path;
				IsDirty = false;
				return true;
			}
			catch (Exception ex)
			{
				AppLog.Error("AppViewModel.Save", ex);
				return false;
			}
		}

		/// <summary>
		/// Загрузить код из файла. Добавляет файл в recent.
		/// Возвращает null при ошибке.
		/// </summary>
		public string Load(string path)
		{
			try
			{
				string text = File.ReadAllText(path);
				_recentFiles.Push(path);
				CurrentFilePath = path;
				IsDirty = false;
				return text;
			}
			catch (Exception ex)
			{
				AppLog.Error("AppViewModel.Load", ex);
				return null;
			}
		}

		/// <summary>
		/// Очистить текущий скрипт и результат.
		/// </summary>
		public void Clear()
		{
			DisposeCurrentRoot();
			CurrentFilePath = null;
			IsDirty = false;
		}

		public void Dispose()
		{
			DisposeCurrentRoot();
			_runner?.Dispose();
		}

		private void DisposeCurrentRoot()
		{
			if (CurrentRoot != null)
			{
				CurrentRoot.DisposeTree();
				CurrentRoot = null;
			}
		}
	}
}
