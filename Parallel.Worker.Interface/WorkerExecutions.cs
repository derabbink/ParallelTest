using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface
{
    public static class WorkerExecutions
    {
        /// <summary>
        /// Executes an operation
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static Guid Execute(this Worker worker, Func<object> operation)
        {
            return worker.Execute(Wrap(operation), null);
        }

        /// <summary>
        /// Executes an operation
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="operation"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static Guid Execute(this Worker worker, Action<object> operation, object arg)
        {
            return worker.Execute(Wrap(operation), arg);
        }

        /// <summary>
        /// Executes an operation
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static Guid Execute(this Worker worker, Action operation)
        {
            return worker.Execute(Wrap(operation), null);
        }

        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation that takes no arguments</param>
        /// <returns>operation that takes a bogus argument. supplied argument will discarded</returns>
        private static Func<object, object> Wrap(Func<object> operation)
        {
            return _ => operation();
        }

        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation without return value</param>
        /// <returns>operation that returns null</returns>
        private static Func<object, object> Wrap(Action<object> operation)
        {
            return arg =>
                {
                    operation(arg);
                    return null;
                };
        }

        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation without argument or return value</param>
        /// <returns>operation that takes bogus argument and returns null. supplied argument will be discarded</returns>
        private static Func<object, object> Wrap(Action operation)
        {
            return _ =>
                {
                    operation();
                    return null;
                };
        }
    }
}
