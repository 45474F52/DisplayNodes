namespace DisplayNodes.Core.Rendering
{
    /// <summary>
    /// Фабрика <see cref="IRenderRoot"/>. Инкапсулирует знание о родительском компоненте и специфике бэкенда.
    /// </summary>
    public interface IRenderRootFactory
    {
        /// <summary>Создаёт корневой контейнер. Parent задаётся в конструкторе фабрики.</summary>
        IRenderRoot Create();
    }
}
