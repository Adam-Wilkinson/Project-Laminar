using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Laminar.Avalonia.DragDrop;

/// <summary>
/// An implementation of <see cref="IDataTemplate"/> which can be added to one control that searches another control for a valid template
/// </summary>
internal class DataTemplateRedirect : IDataTemplate
{
    readonly IControl _host;

    public DataTemplateRedirect(IControl host)
    {
        _host = host;
    }

    public IControl Build(object param)
    {
        return _host.FindDataTemplate(param).Build(param);
    }

    public bool Match(object data)
    {
        return _host.FindDataTemplate(data) != null;
    }
}
