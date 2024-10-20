using Avalonia.Controls.Templates;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Laminar.Avalonia.DataTemplates;

using AvaloniaDataTemplates = global::Avalonia.Controls.Templates.DataTemplates;

public class DataTemplateInclude : IDataTemplate
{
    private readonly Uri _baseUri;
    private AvaloniaDataTemplates _loaded;
    private bool _isLoading;

    public Uri Source { get; set; }

    public AvaloniaDataTemplates Loaded
    {
        get
        {
            if (_loaded == null)
            {
                _isLoading = true;
                _loaded = (AvaloniaDataTemplates)AvaloniaXamlLoader.Load(Source, _baseUri);
                _isLoading = false;
            }

            return _loaded;
        }
    }

    public DataTemplateInclude(Uri baseUri)
    {
        _baseUri = baseUri;
    }

    public DataTemplateInclude(IServiceProvider serviceProvider)
    {
        _baseUri = serviceProvider.GetService<IUriContext>().BaseUri;
    }


    public bool Match(object data)
    {
        if (_isLoading || Loaded == null)
            return false;

        return Loaded.Any(dt => dt.Match(data));
    }

    public IControl Build(object data)
    {
        if (_isLoading || Loaded == null)
            return null;

        return Loaded.FirstOrDefault(dt => dt.Match(data))?.Build(data);
    }
}