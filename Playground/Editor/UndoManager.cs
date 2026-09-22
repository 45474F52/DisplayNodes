using System;
using System.Collections.Generic;
using System.Windows.Forms;

using DisplayNodes.Playground.Infrastructure;

namespace DisplayNodes.Playground.Editor
{
	/// <summary>
	/// Собственная система undo/redo для RichTextBox.
	/// Хранит снапшоты (Text + позиция каретки) и группирует подряд
	/// идущий ввод в один шаг. Встроенный undo RichEdit должен быть
	/// отключён (EM_SETUNDOLIMIT = 0), иначе стеки будут конфликтовать.
	/// </summary>
	internal sealed class UndoManager
	{
		private struct Snapshot
		{
			public string Text;
			public int SelectionStart;
			public int SelectionLength;
		}

		private readonly int _maxSteps;
		private readonly int _typingGroupMs;
		private readonly RichTextBox _editor;
		private readonly LinkedList<Snapshot> _undo = new LinkedList<Snapshot>();
		private readonly LinkedList<Snapshot> _redo = new LinkedList<Snapshot>();

		private Snapshot _last;
		private bool _suspended;
		private bool _typingGroupActive;
		private DateTime _lastTypingTime;

		public UndoManager(RichTextBox editor, EditorSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");
			_editor = editor ?? throw new ArgumentNullException("editor");
			_maxSteps = settings.UndoMaxSteps;
			_typingGroupMs = settings.TypingGroupMs;
			_last = Capture();
		}

		/// <summary>Стереть всю историю. Вызывать после загрузки файла или очистки.</summary>
		public void Reset()
		{
			_undo.Clear();
			_redo.Clear();
			_typingGroupActive = false;
			_last = Capture();
		}

		/// <summary>Снапшот текущего состояния как «до операции».</summary>
		public void BeginChange()
		{
			if (_suspended)
				return;

			var current = Capture();
			if (IsSame(_last, current))
				return;

			PushUndo(_last);
			_redo.Clear();
			_last = current;
			_typingGroupActive = false;
		}

		/// <summary>
		/// Записать снапшот после простого ввода. Если это продолжение
		/// группы набора (быстрое нажатие подряд), снапшот не пишется —
		/// вся группа оформится одним шагом позже.
		/// </summary>
		public void RecordTyping()
		{
			if (_suspended)
				return;

			var current = Capture();

			// Ничего не изменилось.
			if (current.Text == _last.Text)
				return;

			DateTime now = DateTime.Now;
			bool sameGroup = _typingGroupActive
						  && (now - _lastTypingTime).TotalMilliseconds < _typingGroupMs
						  && IsTypingContinuation(_last, current);

			if (!sameGroup)
			{
				// Стартуем новую группу: зафиксировали «до».
				PushUndo(_last);
				_redo.Clear();
				_typingGroupActive = true;
			}

			_lastTypingTime = now;
			_last = current;
		}

		/// <summary>Сбросить группу набора — вызов при навигации/смене строки.</summary>
		public void BreakTypingGroup()
		{
			_typingGroupActive = false;
		}

		public bool CanUndo { get { return _undo.Count > 0; } }
		public bool CanRedo { get { return _redo.Count > 0; } }

		public void Undo()
		{
			if (_undo.Count == 0)
				return;

			var last = _undo.Last.Value;
			_undo.RemoveLast();

			PushRedo(_last);
			_last = last;

			_suspended = true;
			_typingGroupActive = false;
			try
			{ Apply(last); }
			finally { _suspended = false; }
		}

		public void Redo()
		{
			if (_redo.Count == 0)
				return;

			var next = _redo.Last.Value;
			_redo.RemoveLast();

			PushUndo(_last);
			_last = next;

			_suspended = true;
			_typingGroupActive = false;
			try
			{ Apply(next); }
			finally { _suspended = false; }
		}

		/// <summary>
		/// Обновить записанную позицию каретки в _last, не трогая текст.
		/// Вызывать при перемещении каретки пользователем (стрелки, клики),
		/// чтобы следующий снапшот знал актуальную позицию.
		/// </summary>
		public void SyncCaret()
		{
			if (_suspended)
				return;
			_last.SelectionStart = _editor.SelectionStart;
			_last.SelectionLength = _editor.SelectionLength;
		}

		// ---------------- internals ----------------

		private void PushUndo(Snapshot s)
		{
			_undo.AddLast(s);
			while (_undo.Count > _maxSteps)
				_undo.RemoveFirst();
		}

		private void PushRedo(Snapshot s)
		{
			_redo.AddLast(s);
			while (_redo.Count > _maxSteps)
				_redo.RemoveFirst();
		}

		private Snapshot Capture()
		{
			return new Snapshot
			{
				Text = _editor.Text,
				SelectionStart = _editor.SelectionStart,
				SelectionLength = _editor.SelectionLength
			};
		}

		private void Apply(Snapshot s)
		{
			// Временно отключаем TextChanged, чтобы не записать самим же
			// применение снапшота как новую правку.
			ScrollRichTextBox scroll = _editor as ScrollRichTextBox;
			bool oldSuppress = scroll != null && scroll.SuppressTextChanged;
			if (scroll != null)
				scroll.SuppressTextChanged = true;

			try
			{
				_editor.Text = s.Text;

				int start = s.SelectionStart;
				if (start > _editor.TextLength)
					start = _editor.TextLength;
				int length = s.SelectionLength;
				if (start + length > _editor.TextLength)
					length = _editor.TextLength - start;

				_editor.SelectionStart = start;
				_editor.SelectionLength = length;
				_editor.ScrollToCaret();
			}
			finally
			{
				if (scroll != null)
				{
					scroll.SuppressTextChanged = oldSuppress;
					if (!oldSuppress)
						scroll.RaiseTextChanged();
				}
			}
		}

		private static bool IsSame(Snapshot a, Snapshot b)
		{
			return a.Text == b.Text
				&& a.SelectionStart == b.SelectionStart
				&& a.SelectionLength == b.SelectionLength;
		}

		/// <summary>
		/// Грубая эвристика «это продолжение набора, а не другая операция»:
		/// новый текст длиннее на 1 символ, старая каретка была в конце,
		/// новая — на 1 правее.
		/// </summary>
		private static bool IsTypingContinuation(Snapshot prev, Snapshot curr)
		{
			if (curr.Text.Length != prev.Text.Length + 1)
				return false;
			// Каретка старого снапшота должна быть ПОСЛЕ того, что было — то есть
			// на позиции curr.SelectionStart - 1. Это всегда верно при вставке.
			if (curr.SelectionStart != prev.SelectionStart + 1)
				return false;
			// И не должно быть выделения в момент вставки.
			if (prev.SelectionLength != 0)
				return false;
			return true;
		}
	}
}
