using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parallel.Worker.Interface.Instruction
{
    public static class Wrapper
    {
        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation that takes no arguments</param>
        /// <returns>operation that takes a bogus argument. supplied argument will discarded</returns>
        public static Func<object, TResult> Wrap<TResult>(Func<TResult> operation) where TResult : class
        {
            return _ => operation();
        }

        /// <summary>
        /// Wraps an operation in a lambda that matches the Worker.Execute argument
        /// </summary>
        /// <param name="operation">operation without return value</param>
        /// <returns>operation that returns null</returns>
        public static Func<TArgument, object> Wrap<TArgument>(Action<TArgument> operation) where TArgument : class
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
        public static Func<object, object> Wrap(Action operation)
        {
            return _ =>
            {
                operation();
                return null;
            };
        }
    }
}
