using Avalonia;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.Markup;

public class ConnectorLocationExtension : MarkupExtension
{
    private readonly BindingBase _connectorBinding;

    public ConnectorLocationExtension(IConnector connector)
    {
        _connectorBinding = connector.AsStaticBinding();
    }
    
    public ConnectorLocationExtension(BindingBase connectorBinding)
    {
        _connectorBinding = connectorBinding;
    }

    public override BindingBase ProvideValue(IServiceProvider serviceProvider) =>
        serviceProvider.UsingStaticResource<ConnectorRegistry>(ConnectorRegistry.Key, (registry, target) =>
        {
            if (target is not Visual targetVisual)
            {
                throw new InvalidOperationException("ConnectorLocationExtension can only target visuals");
            }

            return DependentObservableBinding.Create(_connectorBinding, new TransformedCenterObservable(targetVisual, registry));
        });
}

public class TransformedCenterObservable(Visual owner, ConnectorRegistry registry) : IDependentObservable<IConnector, Point>
{
    private readonly List<Subscription> _subscriptions = [];
    
    private Visual? _trackedVisual;
    
    public IDisposable Subscribe(IObserver<Point> observer) => new Subscription(this, observer, owner, _trackedVisual);

    public IConnector? Dependency
    {
        get;
        set
        {
            field = value;
            _trackedVisual = field is not null ? registry.GetVisualForConnector(field) : null;
            foreach (var subscription in _subscriptions)
            {
                subscription.UpdateTrackedVisual(_trackedVisual);
            }
        }
    }
    
    private class Subscription : IDisposable
    {
        private readonly TransformedCenterObservable _root;
        private readonly IObserver<Point> _observer;
        private readonly Visual _ownerVisual;

        private Visual? _trackedVisual;
        private Layoutable? _commonAncestor;
        private bool _disposed;
        
        public Subscription(TransformedCenterObservable root, IObserver<Point> observer, Visual ownerVisual, Visual? trackedVisual)
        {
            _root = root;
            _observer = observer;
            _ownerVisual = ownerVisual;
            _trackedVisual = trackedVisual;
            
            _ownerVisual.DetachedFromVisualTree += OwnerDetachedFromVisualTree; 
            _root._subscriptions.Add(this);
            if (_trackedVisual is null)
            {
                _observer.OnNext(default);
            }
            else
            {
                UpdateTrackedVisual(_trackedVisual);
            }
        }

        private void OwnerDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) => Dispose();

        public void UpdateTrackedVisual(Visual? visualToTrack)
        {
            _trackedVisual?.AttachedToVisualTree -= TrackedVisualMoved;
            _trackedVisual?.DetachedFromVisualTree -= CommonAncestorOnLayoutUpdated;
            _trackedVisual = visualToTrack;
            _trackedVisual?.AttachedToVisualTree += CommonAncestorOnLayoutUpdated;
            _trackedVisual?.DetachedFromVisualTree += TrackedVisualMoved;
            UpdateCommonAncestor();
        }

        private void TrackedVisualMoved(object? sender, VisualTreeAttachmentEventArgs e) => UpdateCommonAncestor();

        private void UpdateCommonAncestor()
        {
            var newCommonAncestor = _trackedVisual is null
                ? null
                : FindLayoutableCommonAncestor(_ownerVisual, _trackedVisual);

            if (Equals(_commonAncestor, newCommonAncestor)) return;
            _commonAncestor?.LayoutUpdated -= CommonAncestorOnLayoutUpdated;
            _commonAncestor = newCommonAncestor;
            _commonAncestor?.LayoutUpdated += CommonAncestorOnLayoutUpdated;
            PublishCurrentPosition();
        }
        
        private void CommonAncestorOnLayoutUpdated(object? sender, EventArgs e) => PublishCurrentPosition();

        private void PublishCurrentPosition()
        {
            if (_disposed || _trackedVisual?.TransformToVisual(_ownerVisual) is not { } transform) 
                return;

            var next = new Point(_trackedVisual.Bounds.Width / 2, _trackedVisual.Bounds.Height / 2).Transform(transform);
            
            _observer.OnNext(next);
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _ownerVisual.DetachedFromVisualTree -= OwnerDetachedFromVisualTree;
            _root._subscriptions.Remove(this);
            _observer.OnCompleted();
        }
        
        private static Layoutable? FindLayoutableCommonAncestor(Visual visualOne, Visual visualTwo)
        {
            var common = visualOne.FindCommonVisualAncestor(visualTwo);

            while (common is not null)
            {
                if (common is Layoutable layoutable)
                    return layoutable;

                common = common.GetVisualParent();
            }

            return null;
        }
    }
}