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
    public class ParallelInstruction<TArgument, TResult1, TResult2> :
        LocalInstruction<TArgument, Tuple<TResult1, TResult2>>
        where TArgument : class
        where TResult1 : class
        where TResult2 : class
    {
        public ParallelInstruction(CoordinatedInstruction<TArgument, TResult1> instruction1,
                                   CoordinatedInstruction<TArgument, TResult2> instruction2)
            : base(
                ComposeDelegate(instruction1, instruction2),
                instruction1.Timeout == -1 || instruction2.Timeout == -1
                    ? -1
                    : Math.Max(instruction1.Timeout, instruction2.Timeout))
        {}

        public ParallelInstruction(CoordinatedInstruction<TArgument, TResult1> instruction1,
                                   CoordinatedInstruction<TArgument, TResult2> instruction2,
                                   int timeoutMS)
            : base(ComposeDelegate(instruction1, instruction2), timeoutMS)
        { }

        private static Func<CancellationToken, Action, TArgument, Tuple<TResult1, TResult2>>
            ComposeDelegate(CoordinatedInstruction<TArgument, TResult1> instruction1,
                            CoordinatedInstruction<TArgument, TResult2> instruction2)
        {
            Contract.Requires(instruction1 != null);
            Contract.Requires(instruction2 != null);

            Func<CancellationToken, Action, TArgument, Tuple<TResult1, TResult2>> result = (ct, progress, arg) =>
                {
                    CancellationTokenSource cts1 = new CancellationTokenSource();
                    CancellationTokenSource cts2 = new CancellationTokenSource();
                    ConcurrentQueue<Exception> operationalExceptions = new ConcurrentQueue<Exception>();
                    ConcurrentQueue<TaskCanceledException> cancellationExceptions = new ConcurrentQueue<TaskCanceledException>();
                    Future<TResult1> future1 = null;
                    Future<TResult2> future2 = null;
                    ct.Register(() =>
                        {
                            cts1.Cancel();
                            cts2.Cancel();
                        });
                    progress();
                    Task task1 = Task.Factory.StartNew(() =>
                        {
                            progress();
                            try
                            {
                                future1 = instruction1.InvokeAndWait(cts1.Token, progress, arg);
                            }
                            catch (AggregateException e)
                            {
                                if (e.InnerException is TaskCanceledException)
                                    cancellationExceptions.Enqueue(e.InnerException as TaskCanceledException);
                                else
                                    operationalExceptions.Enqueue(e.InnerException);
                                cts2.Cancel();
                            }
                            finally
                            {
                                progress();
                            }
                        });
                    progress();
                    Task task2 = Task.Factory.StartNew(() =>
                        {
                            progress();
                            try
                            {
                                future2 = instruction2.InvokeAndWait(cts2.Token, progress, arg);
                            }
                            catch (AggregateException e)
                            {
                                if (e.InnerException is TaskCanceledException)
                                    cancellationExceptions.Enqueue(e.InnerException as TaskCanceledException);
                                else
                                    operationalExceptions.Enqueue(e.InnerException);
                                cts1.Cancel();
                            }
                            finally
                            {
                                progress();
                            }
                        });
                    progress();
                    Task[] tasks = new []{task1, task2};
                    //should not throw
                    Task.WaitAll(tasks);
                    progress();

                    if (operationalExceptions.Any())
                        throw operationalExceptions.First();
                    if (cancellationExceptions.Any())
                    {
                        if (ct.IsCancellationRequested)
                            ct.ThrowIfCancellationRequested();
                        else
                            throw cancellationExceptions.First();
                    }

                    //if previous line did not throw, this won't either
                    return new Tuple<TResult1, TResult2>(future1.Result, future2.Result);
                };
            return result;
        }
    }
}
