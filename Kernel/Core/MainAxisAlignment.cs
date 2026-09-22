namespace DisplayNodes.Core
{
	/// <summary>
	/// Определяет способ распределения свободного пространства вдоль главной оси в StackLayoutNode.
	/// </summary>
	public enum MainAxisAlignment
	{
		/// <summary>Прижать все элементы к началу оси.</summary>
		Start,

		/// <summary>Сгруппировать элементы по центру оси.</summary>
		Center,

		/// <summary>Прижать все элементы к концу оси.</summary>
		End,

		/// <summary>Равномерно распределить свободное пространство между элементами.</summary>
		SpaceBetween,
	}
}