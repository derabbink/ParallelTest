using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Instruction;
using Parallel.Worker.Interface;

namespace Parallel.Worker
{
    public static class CallbackExecutor
    {
        public static Future<TResult> Execute<TArgument, TResult>(this Executor executor, Action<TArgument, Action<TResult>> instruction, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            return executor.Execute(CallbackInstruction<TArgument, TResult>.CallbackToReturn(instruction), argument);
        }
    }
}
