namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Фабрика кистей. Абстрагирует создание кистей от конкретного бэкенда.
	/// </summary>
	public interface IBrushFactory
	{
		/// <summary>Создаёт сплошную кисть заданного цвета.</summary>
		IBrush CreateSolidBrush(Color color);
	}
}