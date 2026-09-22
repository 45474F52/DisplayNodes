using DisplayNodes.Fluent;

namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Контракт компонента-контейнера, поддерживающего принудительное обновление отрисовки.
	/// </summary>
	/// <remarks>
	/// <para>Реализуется адаптерами рендерера для корневых контейнеров.
	/// Используется классом <see cref="DisplayRoot"/> для управления жизненным циклом UI-дерева.</para>
	/// <para>В отличие от базового <see cref="IRenderComponent"/>, добавляет возможность
	/// сигнализировать рендереру о необходимости перерисовки содержимого.</para>
	/// </remarks>
	public interface ILayoutComponent : IRenderComponent
	{
		/// <summary>
		/// Запрашивает принудительное обновление отрисовки контейнера и его содержимого.
		/// </summary>
		/// <remarks>
		/// Вызов метода не гарантирует мгновенную перерисовку.
		/// </remarks>
		void Refresh();
	}
}