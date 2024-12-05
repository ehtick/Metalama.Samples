using System.Runtime.CompilerServices;

namespace Metalama.Samples.Proxy;

public interface IInterceptor
{
    public TResult Invoke<TArgs, TResult>(
        ref TArgs args,
        InterceptionMetadata metadata,
        InterceptorDelegate<TArgs, TResult> proceed ) where TArgs : struct, ITuple;

    public Task<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, Task<TResult>> proceed ) where TArgs : struct, ITuple;

    public ValueTask<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, ValueTask<TResult>> proceed ) where TArgs : struct, ITuple;
}