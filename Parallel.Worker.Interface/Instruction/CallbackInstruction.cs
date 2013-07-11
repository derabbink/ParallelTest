using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface.Instruction
{
    /// <summary>
    /// Wraps a callback pattern like it is a normal call-return method
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class CallbackInstruction<TArgument, TResult> : SafeInstruction<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        public CallbackInstruction(Action<TArgument, Action<TResult>> instruction, TArgument argument)
            : base(CallbackToReturn(instruction), argument)
        {
        }

        /// <summary>
        /// Wraps a callback pattern as a value returning function
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public static Func<TArgument, TResult> CallbackToReturn(Action<TArgument, Action<TResult>> instruction)
        {
            Func<TArgument, TResult> wrapped = argument =>
                {
                    ManualResetEvent callbackCompleted = new ManualResetEvent(false);
                    TResult callbackResult = null;

                    instruction(argument, result =>
                        {
                            callbackResult = result;
                            callbackCompleted.Set();
                        });
                    callbackCompleted.WaitOne();
                    return callbackResult;
                };
            return wrapped;
        }

        public static Action<TArgument, Action<TResult>> ReturnToCallback(Func<TArgument, TResult> instruction)
        {
            Action<TArgument, Action<TResult>> wrapped = (arg, callback) =>
                {
                    var result = instruction(arg);
                    callback(result);
                };
            return wrapped;
        }
    }
}
