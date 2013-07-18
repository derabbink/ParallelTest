using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Parallel.Coordinator.Interface;
using Parallel.Worker;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Prallel.Coordinator.Interface.Test
{
    [TestFixture]
    public class TimeOutMonitorTest
    {
        private IExecutor _executor;
        private Action<CancellationToken, Action>  _passThroughOperation;
        private Action<CancellationToken, Action, object> _repeatingProgressOperation;
        private ManualResetEventSlim _holdPassThroughOperation;
        private ManualResetEventSlim _notifyPassThroughOperation;
        private AutoResetEvent _notifyRepeatingOperation;
        private AutoResetEvent _holdRepeatingOperation;
        
        #region setup
        [SetUp]
        public void Setup()
        {
            _holdPassThroughOperation = new ManualResetEventSlim(false);
            _notifyPassThroughOperation = new ManualResetEventSlim(false);
            _notifyRepeatingOperation = new AutoResetEvent(false);
            _holdRepeatingOperation = new AutoResetEvent(false);
            _executor = new TaskExecutor();
            _passThroughOperation = (ct, p) =>
                {
                    _notifyPassThroughOperation.Set();
                    _holdPassThroughOperation.Wait(ct);
                };
            _repeatingProgressOperation = (ct, progress, count) =>
                {
                    int icount = (int) count;
                    int i = 0;
                    while (i<icount && !ct.IsCancellationRequested)
                    {
                        _notifyRepeatingOperation.Set();
                        _holdRepeatingOperation.WaitOne();
                        progress();
                        i++;
                    }
                    ct.ThrowIfCancellationRequested();
                };
        }
        #endregion

        #region tests
        
        [Test]
        public void TooLongOperationIsDiscovered()
        {
            int anyTimeOut = 0;
            //_passThroughOperation will not continue
            var monitorTarget = TimeOutable.FromFuture(_executor.Execute(_passThroughOperation));
            Task<bool> checkWaitTask = Task.Factory.StartNew(() =>
                {
                    return TimeOutMonitor.MonitoredWait(monitorTarget, anyTimeOut);
                });
            checkWaitTask.Wait();
            
            Assert.That(checkWaitTask.IsCompleted, Is.True);
            Assert.That(checkWaitTask.Result, Is.False);
        }

        [Test]
        public void CompletedOperationPassesThrough()
        {
            int anyTimeOut = 0;
            Future<object> future = _executor.Execute(_passThroughOperation);
            var monitorTarget = TimeOutable.FromFuture(future);
            _holdPassThroughOperation.Set();
            future.Wait();
            Assert.That(future.IsCompleted, Is.True);

            Task<bool> checkWaitTask = Task.Factory.StartNew(() =>
            {
                return TimeOutMonitor.MonitoredWait(monitorTarget, anyTimeOut);
            });
            checkWaitTask.Wait();

            Assert.That(checkWaitTask.IsCompleted, Is.True);
            Assert.That(checkWaitTask.Result, Is.True);
        }

        [Test]
        public void HeartbeatsExtendTimeout([Values(0, 1, 2, 3)] int heartbeatCount)
        {
            int iterationWait = 500;
            Future<object> future = _executor.Execute(_repeatingProgressOperation, heartbeatCount);
            var monitorTarget = TimeOutable.FromFuture(future);
            Task<bool> checkWaitTask = Task.Factory.StartNew(() =>
                {
                    return TimeOutMonitor.MonitoredWait(monitorTarget, iterationWait+50);
                });
            for (int i = 0; i < heartbeatCount; i++)
            {
                _notifyRepeatingOperation.WaitOne();
                Thread.Sleep(iterationWait);
                _holdRepeatingOperation.Set();
            }
            checkWaitTask.Wait();

            Assert.That(checkWaitTask.IsCompleted, Is.True);
            Assert.That(checkWaitTask.Result, Is.True);
        }

        #endregion
    }
}
