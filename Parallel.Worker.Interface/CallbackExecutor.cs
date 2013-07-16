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
        public static Future<TResult> Execute<TArgument, TResult>(this IExecutor executor, Action<CancellationToken, Action, TArgument, Action<TResult>> instruction, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            return executor.Execute(CallbackToReturn(instruction), argument);
        }

        public static Func<CancellationToken, Action, TArgument, TResult> CallbackToReturn<TArgument, TResult>(Action<CancellationToken, Action, TArgument, Action<TResult>> instruction)
            where TArgument : class
            where TResult : class
        {
            Func<CancellationToken, Action, TArgument, TResult> wrapped = (ct, p, argument) =>
            {
                ManualResetEventSlim callbackCompleted = new ManualResetEventSlim(false);
                TResult callbackResult = null;

                instruction(ct, p, argument, result =>
                {
                    callbackResult = result;
                    callbackCompleted.Set();
                });
                callbackCompleted.Wait();
                return callbackResult;
            };
            return wrapped;
        }
    }
}
