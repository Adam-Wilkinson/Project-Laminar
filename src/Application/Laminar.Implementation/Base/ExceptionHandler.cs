using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Laminar.Contracts.Base;
using Laminar.Domain;

namespace Laminar.Implementation.Base;

public class ExceptionHandler(IDispatcher dispatcher, IEnumerable<IExceptionSink> sinks) : IExceptionHandler
{
    public async Task OnExceptionAsync(Exception exception)
    {
        foreach (var sink in sinks)
        {
            await sink.OnException(exception);
        }
    }
    
    public void OnException(Exception exception)
    {
        dispatcher.InvokeAsync(async void () =>
        {
            try
            {
                foreach (var sink in sinks)
                {
                    await sink.OnException(exception);
                }
            }
            catch (Exception e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }
        });
    }
}