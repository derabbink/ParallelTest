using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface.Communication.SingleChannelCallback
{
    public interface IServer<TArgument, TResult>
        where TArgument : class
        where TResult : class
    {
        void Run(Guid operationId, Func<CancellationToken, Action, TArgument, TResult> instruction, TArgument argument, IClient<TResult> callback);

        void Cancel(Guid operationId);
    }
}
