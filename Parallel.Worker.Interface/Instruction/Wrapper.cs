using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Parallel.Worker.Interface.Instruction
{
    public static class Wrapper
    {
        public static Future<TResult> Execute<TResult>(this IExecutor executor, Func<CancellationToken, Action, TResult> operation)
            where TResult : class
        {
            return executor.Execute(Wrap(operation), null);
        }

        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation that takes no data arguments</param>
        /// <returns>operation that takes a bogus argument. supplied argument will discarded</returns>
        public static Func<CancellationToken, Action, object, TResult> Wrap<TResult>(Func<CancellationToken, Action, TResult> operation) where TResult : class
        {
            return (ct, p, _) => operation(ct, p);
        }

        public static Future<object> Execute<TArgument>(this IExecutor executor, Action<CancellationToken, Action, TArgument> operation, TArgument argument)
            where TArgument : class
        {
            return executor.Execute(Wrap(operation), argument);
        }

        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation without return value</param>
        /// <returns>operation that returns null</returns>
        public static Func<CancellationToken, Action, TArgument, object> Wrap<TArgument>(Action<CancellationToken, Action, TArgument> operation) where TArgument : class
        {
            return (ct, p, arg) =>
            {
                operation(ct, p, arg);
                return null;
            };
        }

        public static Future<object> Execute(this IExecutor executor, Action<CancellationToken, Action> operation)
        {
            return executor.Execute(Wrap(operation), null);
        }

        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation without data argument or return value</param>
        /// <returns>operation that takes bogus argument and returns null. supplied argument will be discarded</returns>
        public static Func<CancellationToken, Action, object, object> Wrap(Action<CancellationToken, Action> operation)
        {
            return (ct, p, _) =>
            {
                operation(ct, p);
                return null;
            };
        }

        /// <summary>
        /// wraps an operation that does not take a CancellationToken or progress reporting action
        /// into one that does
        /// </summary>
        /// <typeparam name="TArgument"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="function"></param>
        /// <returns></returns>
        public static Func<CancellationToken, Action, TArgument, TResult> Wrap<TArgument, TResult>(
                Func<TArgument, TResult> function)
        {
            return (ct, p, a) => function(a);
        }

        public static Func<object, TResult> Wrap<TResult>(Func<TResult> function)
        {
            return _ => function();
        }
    }
}
