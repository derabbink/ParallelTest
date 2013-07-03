using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Util
{
    /// <summary>
    /// Helper class for translating different types of Finc and Action
    /// into <code>Func<object, object></code>
    /// </summary>
    internal static class OperationWrapper
    {
        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation that takes no arguments</param>
        /// <returns>operation that takes a bogus argument. supplied argument will discarded</returns>
        internal static Func<object, object> Wrap(Func<object> operation)
        {
            return _ => operation();
        }

        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation without return value</param>
        /// <returns>operation that returns null</returns>
        internal static Func<object, object> Wrap(Action<object> operation)
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
        internal static Func<object, object> Wrap(Action operation)
        {
            return _ =>
            {
                operation();
                return null;
            };
        }
    }
}
