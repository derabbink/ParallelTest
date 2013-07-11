using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Worker.Communication.SingleChannelCallback;
using Parallel.Worker.Events.SingleChannelCallback;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Communication.SingleChannelCallback;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker
{
    /// <summary>
    /// Executes instructions who's result is returend through raising a (shared) callback event.
    /// callback event listeners pick up the incoming result and complete the associated future.
    /// Executes instructions sequentially.
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class SingleChannelCallbackExecutor<TArgument, TResult> : Executor<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        protected IClient<TResult> _client;
        protected IServer<TArgument, TResult> _server;

        public SingleChannelCallbackExecutor(IClient<TResult> client, IServer<TArgument, TResult> server)
        {
            _client = client;
            _server = server;
        }

        protected override SafeInstruction<TArgument, TResult> CreateSafeInstruction(Func<TArgument, TResult> instruction, TArgument argument)
        {
            return new SingleChannelCallbackInstruction<TArgument, TResult>(instruction, argument, _server, _client);
        }
    }
}
