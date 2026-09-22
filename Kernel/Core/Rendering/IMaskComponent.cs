using DisplayNodes.Widgets;

namespace DisplayNodes.Core.Rendering
{
    /// <summary>
    /// Контракт компонента-маски для обрезки содержимого.
    /// </summary>
    /// <remarks>
    /// <para>Реализуется адаптерами рендерера (например, <c>GdiRectMask</c>, <c>GdiCircleMask</c>)
    /// и используется виджетом <see cref="ClipNode"/> для ограничения области отрисовки дочерних элементов.</para>
    /// <para>Маска является <see cref="IRenderComponent"/> и участвует в дереве компонентов:
    /// дети <see cref="ClipNode"/> привязываются к маске как к родителю, а не к внешнему контейнеру.</para>
    /// </remarks>
    public interface IMaskComponent : IRenderComponent
    {
    }
}