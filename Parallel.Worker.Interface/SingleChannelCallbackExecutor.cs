using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Communication.SingleChannelCallback;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface
{
    public static class SingleChannelCallbackExecutor
    {
        public static Future<SafeInstructionResult<TResult>> Execute<TArgument, TResult>(this IExecutor executor, Func<TArgument, TResult> instruction, TArgument argument, IServer<TArgument, TResult> invocationChannel, IClient<TResult> callbackChannel)
            where TArgument : class
            where TResult : class
        {
            return executor.Execute(SingleChannelCallbackInstruction<TArgument, TResult>.SingleChannelCallbackToReturn(instruction, invocationChannel, callbackChannel), argument);
        }
    }
}
