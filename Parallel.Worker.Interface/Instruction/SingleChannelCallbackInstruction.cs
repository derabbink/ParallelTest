using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Parallel.Worker.Interface.Communication.SingleChannelCallback;
using Parallel.Worker.Interface.Events.SingleChannelCallback;

namespace Parallel.Worker.Interface.Instruction
{
    public class SingleChannelCallbackInstruction<TArgument, TResult> : SafeInstruction<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        public SingleChannelCallbackInstruction(Func<TArgument, TResult> instruction, TArgument argument, IServer<TArgument, TResult> invocationChannel, IClient<TResult> callbackChannel)
            : base(SingleChannelCallbackToReturn(instruction, invocationChannel, callbackChannel), argument)
        {
        }

        public static Func<TArgument, TResult> SingleChannelCallbackToReturn(Func<TArgument, TResult> instruction, IServer<TArgument, TResult> invocationChannel, IClient<TResult> callbackChannel)
        {
            Func<TArgument, TResult> wrapped = argument =>
                {
                    SafeInstructionResult<TResult> wrappedResult = null;
                    ManualResetEvent wrappedResultBlock = new ManualResetEvent(false);

                    Guid operationId = SetupCallbackListener(callbackChannel, result =>
                        {
                            wrappedResult = result;
                            wrappedResultBlock.Set();
                        });
                    invocationChannel.Run(operationId, new SafeInstruction<TArgument, TResult>(instruction, argument), callbackChannel);
                    wrappedResultBlock.WaitOne();
                    
                    //unwrap (possible exception) in caller thread, so event-raiser's exception catcher does not interfere
                    return wrappedResult.Unwrap();
                };
            return wrapped;
        }

        private static Guid SetupCallbackListener(IClient<TResult> callbackChannel, Action<SafeInstructionResult<TResult>> callback)
        {
            Guid operationId = new Guid();
            EventHandler<CallbackEventArgs<TResult>> callbackListener = null;
            callbackListener = (sender, args) =>
            {
                if (args.OperationId == operationId)
                {
                    callbackChannel.UnsubscribeCallbackEvent(callbackListener);
                    callback(args.Result);
                }
            };
            callbackChannel.SubscribeCallbackEvent(callbackListener);
            return operationId;
        }
    }
}
