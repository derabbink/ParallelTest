using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Parallel.Worker.Interface.Instruction
{
    public static class Wrapper
    {
        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation that takes no arguments</param>
        /// <returns>operation that takes a bogus argument. supplied argument will discarded</returns>
        public static Func<CancellationToken, object, TResult> Wrap<TResult>(Func<CancellationToken, TResult> operation) where TResult : class
        {
            return (ct, _) => operation(ct);
        }

        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation without return value</param>
        /// <returns>operation that returns null</returns>
        public static Func<CancellationToken, TArgument, object> Wrap<TArgument>(Action<CancellationToken, TArgument> operation) where TArgument : class
        {
            return (ct, arg) =>
            {
                operation(ct, arg);
                return null;
            };
        }

        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation without argument or return value</param>
        /// <returns>operation that takes bogus argument and returns null. supplied argument will be discarded</returns>
        public static Func<CancellationToken, object, object> Wrap(Action<CancellationToken> operation)
        {
            return (ct, _) =>
            {
                operation(ct);
                return null;
            };
        }
    }
}
