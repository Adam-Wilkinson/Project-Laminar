using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Laminar.Avalonia.ViewModels.Services;

public class FileExplorerLoadingQueue : IDisposable
{
    private readonly Channel<LoadRequest> _channel =
        Channel.CreateUnbounded<LoadRequest>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

    private readonly CancellationTokenSource _cts = new();
    private readonly Task _worker;

    public FileExplorerLoadingQueue()
    {
        _worker = Task.Run(ProcessLoopAsync);
    }

    public void Queue(FileNavigatorItemViewModel vm)
    {
        _channel.Writer.TryWrite(new LoadRequest(vm));
    }

    private async Task ProcessLoopAsync()
    {
        try
        {
            await foreach (var request in _channel.Reader.ReadAllAsync(_cts.Token))
            {
                try
                {
                    if (request.ViewModel.InitializationState is TreeViewInitializationState.Uninitialized)
                    {
                        await request.ViewModel.LoadContentsAsync();
                    }

                    if (request.ViewModel.InitializationState is TreeViewInitializationState.ChildrenContentsUnloaded)
                    {
                        await request.ViewModel.LoadChildrenContentsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    request.ViewModel.ResetLoadState();
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _channel.Writer.TryComplete();
        _worker.Dispose();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed record LoadRequest(FileNavigatorItemViewModel ViewModel);
}