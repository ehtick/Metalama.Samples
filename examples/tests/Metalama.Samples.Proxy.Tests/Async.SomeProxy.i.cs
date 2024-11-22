using System.Reflection;
using Metalama.Samples.Proxy.Tests.Async;

namespace Metalama.Samples.Proxy.Tests
{
    public class SomeProxy : ISomeInterface
    {
        private ISomeInterface _intercepted;
        private IInterceptor _interceptor;
        private static InterceptionMetadata _metadata1;
        private static InterceptionMetadata _metadata2;
        private static InterceptionMetadata _metadata3;
        private static InterceptionMetadata _metadata4;

        static SomeProxy()
        {
            _metadata1 = new InterceptionMetadata(
                typeof(ISomeInterface).GetMethod("TaskMethodAsync",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(int), typeof(string) }, null), true);
            _metadata2 = new InterceptionMetadata(
                typeof(ISomeInterface).GetMethod("TaskOfIntMethodAsync",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(int), typeof(string) }, null), true);
            _metadata3 = new InterceptionMetadata(
                typeof(ISomeInterface).GetMethod("ValueTaskMethodAsync",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(int), typeof(string) }, null), true);
            _metadata4 = new InterceptionMetadata(
                typeof(ISomeInterface).GetMethod("ValueTaskOfIntMethodAsync",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(int), typeof(string) }, null), true);
        }

        public SomeProxy(IInterceptor interceptor, ISomeInterface intercepted)
        {
            _interceptor = interceptor;
            _intercepted = intercepted;
        }

        public async Task TaskMethodAsync(int a, string b)
        {
            var args = (a, b);
            await _interceptor.InvokeAsync(args, _metadata1, InvokeAsync);
            return;

            async Task<ValueTuple> InvokeAsync((int, string) receivedArgs)
            {
                await _intercepted.TaskMethodAsync(receivedArgs.Item1, receivedArgs.Item2);
                return default;
            }
        }

        public async Task<int> TaskOfIntMethodAsync(int a, string b)
        {
            var args = (a, b);
            return await _interceptor.InvokeAsync(args, _metadata2, InvokeAsync);

            Task<int> InvokeAsync((int, string) receivedArgs)
            {
                return _intercepted.TaskOfIntMethodAsync(receivedArgs.Item1, receivedArgs.Item2);
            }
        }

        public async ValueTask ValueTaskMethodAsync(int a, string b)
        {
            var args = (a, b);
            await _interceptor.InvokeAsync(args, _metadata3, InvokeAsync);
            return;

            async ValueTask<ValueTuple> InvokeAsync((int, string) receivedArgs)
            {
                await _intercepted.ValueTaskMethodAsync(receivedArgs.Item1, receivedArgs.Item2);
                return default;
            }
        }

        public async ValueTask<int> ValueTaskOfIntMethodAsync(int a, string b)
        {
            var args = (a, b);
            return await _interceptor.InvokeAsync(args, _metadata4, InvokeAsync);

            ValueTask<int> InvokeAsync((int, string) receivedArgs)
            {
                return _intercepted.ValueTaskOfIntMethodAsync(receivedArgs.Item1,
                    receivedArgs.Item2);
            }
        }
    }
}