using System.IO;

namespace DisplayNodes.Core.Rendering
{
    /// <summary>
    /// Фабрика изображений. Абстрагирует создание изображений от бэкенда.
    /// </summary>
    public interface IImageFactory
    {
        /// <summary>Создаёт изображение из файла.</summary>
        IImage CreateFromFile(string path);

        /// <summary>Создаёт изображение из потока.</summary>
        IImage CreateFromStream(Stream stream);

        /// <summary>Создаёт изображение из массива байт.</summary>
        IImage CreateFromBytes(byte[] bytes);
    }
}
