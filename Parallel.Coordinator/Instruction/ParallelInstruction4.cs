using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Coordinator.Interface.Instruction;
using Parallel.Worker.Interface;

namespace Parallel.Coordinator.Instruction
{
    public class ParallelInstruction<TArgument, TResult1, TResult2, TResult3, TResult4> :
        LocalInstruction<TArgument, Tuple<TResult1, TResult2, TResult3, TResult4>>
        where TArgument : class
        where TResult1 : class
        where TResult2 : class
        where TResult3 : class
        where TResult4 : class
    {
        public ParallelInstruction(CoordinatedInstruction<TArgument, TResult1> instruction1,
                                   CoordinatedInstruction<TArgument, TResult2> instruction2,
                                   CoordinatedInstruction<TArgument, TResult3> instruction3,
                                   CoordinatedInstruction<TArgument, TResult4> instruction4)
            : base(
                ComposeDelegate(instruction1, instruction2, instruction3, instruction4),
                instruction1.Timeout == -1 || instruction2.Timeout == -1 || instruction3.Timeout == -1 || instruction4.Timeout == -1
                    ? -1
                    : Math.Max(Math.Max(Math.Max(instruction1.Timeout, instruction2.Timeout), instruction3.Timeout), instruction4.Timeout))
        {}

        public ParallelInstruction(CoordinatedInstruction<TArgument, TResult1> instruction1,
                                   CoordinatedInstruction<TArgument, TResult2> instruction2,
                                   CoordinatedInstruction<TArgument, TResult3> instruction3,
                                   CoordinatedInstruction<TArgument, TResult4> instruction4,
                                   int timeoutMS)
            : base(ComposeDelegate(instruction1, instruction2, instruction3, instruction4), timeoutMS)
        { }

        private static Func<CancellationToken, Action, TArgument, Tuple<TResult1, TResult2, TResult3, TResult4>>
            ComposeDelegate(CoordinatedInstruction<TArgument, TResult1> instruction1,
                            CoordinatedInstruction<TArgument, TResult2> instruction2,
                            CoordinatedInstruction<TArgument, TResult3> instruction3,
                            CoordinatedInstruction<TArgument, TResult4> instruction4)
        {
            Contract.Requires(instruction1 != null);
            Contract.Requires(instruction2 != null);
            Contract.Requires(instruction3 != null);
            Contract.Requires(instruction4 != null);

            Func<CancellationToken, Action, TArgument, Tuple<TResult1, TResult2, TResult3, TResult4>> result = (ct, progress, arg) =>
                {
                    ConcurrentQueue<Exception> operationalExceptions = new ConcurrentQueue<Exception>();
                    ConcurrentQueue<OperationCanceledException> cancellationExceptions = new ConcurrentQueue<OperationCanceledException>();

                    CancellationTokenSource[] ctss = ParallelInstruction.GenerateCancellationTokenSources(4);
                    ct.Register(() => ParallelInstruction.CancelAll(ctss));
                    progress();
                    
                    Future<TResult1> future1 = null;
                    Future<TResult2> future2 = null;
                    Future<TResult3> future3 = null;
                    Future<TResult4> future4 = null;
                    Task[] tasks = new[]
                        {
                            ParallelInstruction.RunInstruction(instruction1, progress, arg, ctss[0], ctss,
                                                                cancellationExceptions.Enqueue,
                                                                operationalExceptions.Enqueue, f => future1 = f),
                            ParallelInstruction.RunInstruction(instruction2, progress, arg, ctss[1], ctss,
                                                                cancellationExceptions.Enqueue,
                                                                operationalExceptions.Enqueue, f => future2 = f),
                            ParallelInstruction.RunInstruction(instruction3, progress, arg, ctss[2], ctss,
                                                                cancellationExceptions.Enqueue,
                                                                operationalExceptions.Enqueue, f => future3 = f),
                            ParallelInstruction.RunInstruction(instruction4, progress, arg, ctss[3], ctss,
                                                                cancellationExceptions.Enqueue,
                                                                operationalExceptions.Enqueue, f => future4 = f)
                        };
                    progress();

                    try
                    {
                        Task.WaitAll(tasks);
                    }
                    catch (AggregateException e)
                    {
                        //might throw if not all tasks have started execution before their cancellation has been requested
                        if (e.InnerException is OperationCanceledException)
                            cancellationExceptions.Enqueue(e.InnerException as OperationCanceledException);
                    };
                    progress();

                    ParallelInstruction.ProcessParallelExecutionErrors(operationalExceptions, cancellationExceptions, ct);

                    //if previous lines did not throw, this won't either
                    return new Tuple<TResult1, TResult2, TResult3, TResult4>(future1.Result, future2.Result, future3.Result, future4.Result);
                };
            return result;
        }
    }
}
