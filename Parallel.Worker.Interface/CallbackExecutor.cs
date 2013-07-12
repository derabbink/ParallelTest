using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface
{
    public static class CallbackExecutor
    {
        public static Future<TResult> Execute<TArgument, TResult>(this IExecutor executor, Action<CancellationToken, TArgument, Action<TResult>> instruction, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            return executor.Execute(CallbackToReturn(instruction), argument);
        }

        public static Func<CancellationToken, TArgument, TResult> CallbackToReturn<TArgument, TResult>(Action<CancellationToken, TArgument, Action<TResult>> instruction)
            where TArgument : class
            where TResult : class
        {
            Func<CancellationToken, TArgument, TResult> wrapped = (ct, argument) =>
            {
                ManualResetEvent callbackCompleted = new ManualResetEvent(false);
                TResult callbackResult = null;

                instruction(ct, argument, result =>
                {
                    callbackResult = result;
                    callbackCompleted.Set();
                });
                callbackCompleted.WaitOne();
                return callbackResult;
            };
            return wrapped;
        }
    }
}
