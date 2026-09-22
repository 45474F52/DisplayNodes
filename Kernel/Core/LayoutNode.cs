using System;
using System.Collections.Generic;

using DisplayNodes.Core.Rendering;

namespace DisplayNodes.Core
{
	/// <summary>
	/// Базовый абстрактный класс для всех узлов layout-системы.
	/// Реализует двухпроходный алгоритм: Measure (измерение) → Arrange (размещение).
	/// </summary>
	public abstract class LayoutNode
	{
		/// <summary>
		/// Список компонентов рендерера, созданных при Apply/Rebuild.
		/// Используется для корректной очистки при Dispose/Rebuild.
		/// </summary>
		internal IList<IRenderComponent> AppliedComponents { get; set; }

		/// <summary>Внешние отступы узла. Не влияют на <see cref="DesiredSize"/>, но уменьшают слот при Arrange.</summary>
		public Thickness Margin { get; set; }

		/// <sumarry>Внутренние отступы узла.</sumarry>
		public Thickness Padding { get; set; }

		/// <summary>Выравнивание узла по горизонтали внутри выделенного слота.</summary>
		public Alignment HAlignment { get; set; } = Alignment.Stretch;

		/// <summary>Выравнивание узла по вертикали внутри выделенного слота.</summary>
		public Alignment VAlignment { get; set; } = Alignment.Start;

		/// <summary>Желаемый размер узла, вычисленный на этапе Measure.</summary>
		public Size DesiredSize { get; protected set; }

		/// <summary>Фактические границы узла после Arrange.</summary>
		public Rect Bounds { get; protected set; }

		/// <summary>Список дочерних узлов.</summary>
		public List<LayoutNode> Children { get; } = new List<LayoutNode>();

		/// <summary>
		/// Вычисляет <see cref="DesiredSize"/> на основе доступного пространства.
		/// </summary>
		/// <param name="available">Доступное пространство от родителя</param>
		/// <returns>Возвращает <see cref="DesiredSize"/> с учётом <see cref="Margin"/></returns>
		public Size Measure(Size available)
		{
			var inner = new Size(
				Math.Max(0, available.Width - Margin.Horizontal),
				Math.Max(0, available.Height - Margin.Vertical)
			);
			DesiredSize = MeasureOverride(inner);
			return new Size(DesiredSize.Width + Margin.Horizontal, DesiredSize.Height + Margin.Vertical);
		}

		/// <summary>
		/// Размещает узел и его детей в пределах <paramref name="finalRect"/>.
		/// </summary>
		/// <param name="finalRect">Слот, выделенный родителем</param>
		public virtual void Arrange(Rect finalRect)
		{
			Rect inner = finalRect.Deflate(Margin);
			Bounds = AlignRect(DesiredSize, inner, HAlignment, VAlignment);
			ArrangeOverride(Bounds);
		}

		/// <summary>Переопределяется в наследниках для вычисления <see cref="DesiredSize"/>.</summary>
		/// <param name="available">Доступное пространство (без <see cref="Margin"/>).</param>
		/// <returns>Желаемый размер контента.</returns>
		protected abstract Size MeasureOverride(Size available);

		/// <summary>Переопределяется в наследниках для размещения детей.</summary>
		/// <param name="finalRect">Inner-слот (с учётом <see cref="Margin"/>, но без выравнивания).</param>
		protected abstract void ArrangeOverride(Rect finalRect);

		/// <summary>
		/// Вычисляет выровненный прямоугольник внутри слота.
		/// </summary>
		private static Rect AlignRect(Size desired, Rect slot, Alignment horizontal, Alignment vertical)
		{
			int w = horizontal == Alignment.Stretch ? slot.Size.Width : Math.Min(desired.Width, slot.Size.Width);
			int h = vertical == Alignment.Stretch ? slot.Size.Height : Math.Min(desired.Height, slot.Size.Height);

			int x;
			switch (horizontal)
			{
				case Alignment.Center:
				x = slot.Point.X + (slot.Size.Width - w) / 2;
				break;
				case Alignment.End:
				x = slot.Point.X + slot.Size.Width - w;
				break;
				default:
				x = slot.Point.X;
				break;
			}

			int y;
			switch (vertical)
			{
				case Alignment.Center:
				y = slot.Point.Y + (slot.Size.Height - h) / 2;
				break;
				case Alignment.End:
				y = slot.Point.Y + slot.Size.Height - h;
				break;
				default:
				y = slot.Point.Y;
				break;
			}

			return new Rect(x, y, w, h);
		}
	}
}
