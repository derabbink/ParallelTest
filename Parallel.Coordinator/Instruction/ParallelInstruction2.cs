using System;
using System.Collections.Generic;
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
            : base(ComposeDelegate(instruction1, instruction2))
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
            Func<CancellationToken, Action, TArgument, Tuple<TResult1, TResult2>> result = (ct, p, arg) =>
                {
                    CancellationTokenSource cts1 = new CancellationTokenSource();
                    CancellationTokenSource cts2 = new CancellationTokenSource();
                    Future<TResult1> future1 = null;
                    Future<TResult2> future2 = null;
                    ct.Register(() =>
                        {
                            cts1.Cancel();
                            cts2.Cancel();
                        });

                    Task task1 = Task.Factory.StartNew(() =>
                        {
                            future1 = instruction1.InvokeAndWait(cts1.Token, p, arg);
                            //tease out any exceptions/errors
                            try { future1.Wait(); }
                            catch (AggregateException)
                            {
                                //and abort other instruction
                                cts2.Cancel();
                            }
                        });
                    Task task2 = Task.Factory.StartNew(() =>
                        {
                            future2 = instruction2.InvokeAndWait(cts2.Token, p, arg);
                            //tease out any exceptions/errors
                            try { future2.Wait(); }
                            catch (AggregateException)
                            {
                                //and abort other instruction
                                cts1.Cancel();
                            }
                        });
                    Task[] tasks = new []{task1, task2};
                    
                    //let this throw exception, if it does
                    Task.WaitAll(tasks);
                    //if previous line did not throw, this won't either
                    return new Tuple<TResult1, TResult2>(future1.Result, future2.Result);
                };
            return result;
        }
    }
}
