using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Coordinator.Interface.Instruction;
using Parallel.Worker.Interface;

namespace Parallel.Coordinator.Instruction
{
    internal static class ParallelInstruction
    {
        internal static Task RunInstruction<TArgument, TResult>(CoordinatedInstruction<TArgument, TResult> instruction,
                                                                 Action progress, TArgument argument,
                                                                 CancellationTokenSource ownCts,
                                                                 IEnumerable<CancellationTokenSource> otherCts,
                                                                 Action<TaskCanceledException>
                                                                     handleCancellationException,
                                                                 Action<Exception> handleOperationalException,
                                                                 Action<Future<TResult>> handleFuture)
            where TArgument : class
            where TResult : class
        {
            Task task = Task.Factory.StartNew(() =>
                {
                    progress();
                    try
                    {
                        var future = instruction.InvokeAndWait(ownCts.Token, progress, argument);
                        handleFuture(future);
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException is TaskCanceledException)
                            handleCancellationException(e.InnerException as TaskCanceledException);
                        else
                            handleOperationalException(e.InnerException);
                        CancelAllButOne(ownCts, otherCts);
                    }
                });
            return task;
        }

        internal static void ProcessParallelExecutionErrors(ConcurrentQueue<Exception> operationalExceptions,
                                                            ConcurrentQueue<TaskCanceledException>
                                                                cancellationExceptions,
                                                            CancellationToken cancellationToken)
        {
            Debug.WriteLine("processing exceptions");
            if (operationalExceptions.Any())
                throw operationalExceptions.First();
            if (cancellationExceptions.Any())
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
                else
                    throw cancellationExceptions.First();
            }
        }

        internal static CancellationTokenSource[] GenerateCancellationTokenSources(int count)
        {
            return Enumerable.Repeat<object>(null, count).Select(_ => new CancellationTokenSource()).ToArray();
        }

        private static void CancelAllButOne(CancellationTokenSource one, IEnumerable<CancellationTokenSource> all)
        {
            CancelAll(all.Where(a => a != one));
        }

        internal static void CancelAll(IEnumerable<CancellationTokenSource> toCancel)
        {
            foreach (var cts in toCancel)
                cts.Cancel();
        }
    }
}
