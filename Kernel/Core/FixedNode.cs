namespace DisplayNodes.Core
{
	/// <summary>
	/// Узел с фиксированным размером. Не зависит от доступного пространства и всегда возвращает заданный размер.
	/// Полезен для создания распорок (Spacer) или разделителей.
	/// </summary>
	public class FixedNode : LayoutNode
	{
		/// <summary>Фиксированный размер узла.</summary>
		public Size FixedSize { get; }

		/// <summary>Создает узел с заданной шириной и высотой.</summary>
		public FixedNode(int width, int height)
		{
			FixedSize = new Size(width, height);
		}

		/// <inheritdoc/>
		protected override void ArrangeOverride(Rect finalRect) { }

		/// <inheritdoc/>
		protected override Size MeasureOverride(Size available) => FixedSize;
	}
}