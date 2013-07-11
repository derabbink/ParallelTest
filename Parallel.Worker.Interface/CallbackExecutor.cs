﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface
{
    public static class CallbackExecutor
    {
        public static Task<SafeInstructionResult<TResult>> Execute<TArgument, TResult>(this IExecutor executor, Action<TArgument, Action<TResult>> instruction, TArgument argument)
            where TArgument : class
            where TResult : class
        {
            return executor.Execute(CallbackInstruction<TArgument, TResult>.CallbackToReturn(instruction), argument);
        }
    }
}
