using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Communication.SingleChannelCallback
{
    public interface IServer<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        void Run(Guid operationId, SafeInstruction<TArgument, TResult> operation, IClient<TResult> callback);
    }
}
