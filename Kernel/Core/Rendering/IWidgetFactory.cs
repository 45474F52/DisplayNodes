using System;

namespace DisplayNodes.Core.Rendering
{
	/// <summary>
	/// Фабрика компонентов рендерера. Создаёт компоненты без родителя — Parent устанавливается позже.
	/// </summary>
	/// <remarks>
	/// Реализации фабрики должны клонировать ресурсы при установке в компонент, 
	/// чтобы consumer-код сохранял владение оригиналами.
	/// </remarks>
	public interface IWidgetFactory
	{
		/// <summary>Создаёт компонент текстовой метки.</summary>
		ILabelComponent CreateLabel();

		/// <summary>Создаёт компонент изображения.</summary>
		IImageComponent CreateImage();

		/// <summary>Создаёт прямоугольную маску.</summary>
		IMaskComponent CreateRectMask();

		/// <summary>Создаёт круговую маску.</summary>
		IMaskComponent CreateCircleMask();

		/// <summary>Создаёт эллиптическую маску.</summary>
		IMaskComponent CreateEllipseMask();

		/// <summary>Создаёт маску со скруглёнными углами.</summary>
		IMaskComponent CreateRoundedRectMask(float cornerRadius);

		/// <summary>Создаёт маску с произвольной формой.</summary>
		IMaskComponent CreatePathMask(Func<Rect, IGraphicsPath> pathBuilder);
	}
}