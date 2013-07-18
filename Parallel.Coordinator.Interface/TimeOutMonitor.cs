using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Events;

namespace Parallel.Coordinator.Interface
{
    public class TimeOutMonitor
    {
        public static bool MonitoredWait(Future target, int timeoutMS)
        {
            return MonitoredWait(TimeOutable.FromFuture(target), timeoutMS);
        }

        public static bool MonitoredWait<TResult>(Future<TResult> target, int timeoutMS)
        {
            return MonitoredWait(TimeOutable.FromFuture(target), timeoutMS);
        }

        public static bool MonitoredWait(ITimeOutable target, int timeoutMS)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            return MonitoredWait(target, timeoutMS, cts.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="timeoutMS"></param>
        /// <param name="cancellationToken">for cancelling the MonitoredWait operation</param>
        /// <returns></returns>
        public static bool MonitoredWait(Future target, int timeoutMS, CancellationToken cancellationToken)
        {
            return MonitoredWait(TimeOutable.FromFuture(target), timeoutMS, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="target"></param>
        /// <param name="timeoutMS"></param>
        /// <param name="cancellationToken">for cancelling the MonitoredWait operation</param>
        /// <returns></returns>
        public static bool MonitoredWait<TResult>(Future<TResult> target, int timeoutMS, CancellationToken cancellationToken)
        {
            return MonitoredWait(TimeOutable.FromFuture(target), timeoutMS, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="timeoutMS"></param>
        /// <param name="cancellationToken">for cancelling the MonitoredWait operation</param>
        /// <returns></returns>
        public static bool MonitoredWait(ITimeOutable target, int timeoutMS, CancellationToken cancellationToken)
        {
            if (target.Wait(0, cancellationToken))
                return true;

            DateTime begin = DateTime.Now;
            EventHandler<ProgressEventArgs> handler = (sender, args) => begin = DateTime.Now;
            target.SubscribeProgress(handler);

            Task<bool> monitoringTask = Task.Factory.StartNew(() =>
                {
                    while (begin.AddMilliseconds(timeoutMS) > DateTime.Now)
                    {
                        //if the target produces an exception, that's considered completedInTime
                        var completedInTime = true;
                        try
                        {
                            completedInTime = target.Wait(timeoutMS, cancellationToken);
                        }
                        catch
                        {
                            //swallow exceptions
                            // they can be reproduced by waiting on the target again
                        }
                        if (completedInTime)
                            return true;
                    }
                    return false;
                });
            try
            {
                //no cancellation token here, since this task should terminate quickly anyway
                // (when the cancellation has been requested)
                monitoringTask.Wait();
            }
            catch
            {
                //swallow exceptions
                // this can/should never happen
            }
            //cleanup
            target.UnsubscribeProgress(handler);
            
            if (monitoringTask.IsCompleted)
                return monitoringTask.Result;
            else
                return false;
        }
    }
}
