using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Worker.Interface.Events;

namespace Parallel.Coordinator.Interface
{
    public class TimeOutMonitor
    {
        public static bool MonitoredWait(ITimeOutable target, int timeoutMS)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            return MonitoredWait(target, timeoutMS, cts.Token);
        }

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
                        if (target.Wait(timeoutMS, cancellationToken))
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
            catch (OperationCanceledException)
            {
                //the monitoringTask was cancelled
                // waiting for target did not complete
                throw;
            }
            catch {/*should not occur*/}
            //cleanup
            target.UnsubscribeProgress(handler);
            
            if (monitoringTask.IsCompleted)
                return monitoringTask.Result;
            else
                return false;
        }
    }
}
