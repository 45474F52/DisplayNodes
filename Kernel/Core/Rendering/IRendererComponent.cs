namespace DisplayNodes.Core.Rendering
{
    /// <summary>
    /// Базовый контракт компонента рендерера
    /// </summary>
    /// <remarks>
    /// Установка <see cref="Parent"/> в адаптере автоматически синхронизируется
    /// с коллекцией детей родительского компонента.
    /// Установка <c>Parent = null</c> удаляет компонент из старого родителя.
    /// </remarks>
    public interface IRenderComponent
    {
        /// <summary>Родительский компонент. Setter автоматически управляет вхождением в Controls родителя.</summary>
        IRenderComponent Parent { get; set; }

        /// <summary>Позиция компонента относительно родителя.</summary>
        Point Location { get; set; }

        /// <summary>Размер компонента.</summary>
        Size Size { get; set; }

        /// <summary>Видимость компонента.</summary>
        bool Visible { get; set; }
    }
}