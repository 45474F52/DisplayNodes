namespace DisplayNodes.Core.Rendering
{
    /// <summary>
    /// Фабрика шрифтов. Абстрагирует создание шрифтов от бэкенда.
    /// </summary>
    public interface IFontFactory
    {
        /// <summary>Создаёт шрифт заданного семейства и размера.</summary>
        /// <param name="family">Имя семейства шрифтов.</param>
        /// <param name="size">Размер в пунктах.</param>
        /// <param name="bold">Жирный.</param>
        /// <param name="italic">Курсив.</param>
        IFont Create(string family, float size, bool bold = false, bool italic = false);
    }
}
