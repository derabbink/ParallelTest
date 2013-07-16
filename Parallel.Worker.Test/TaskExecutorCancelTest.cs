using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Test
{
    [TestFixture]
    public class TaskExecutorCancelTest
    {
        private IExecutor _executor;
        private Func<CancellationToken, Action, object, object> _identity;
        private Func<CancellationToken, Action, object, object> _identityBlocking;
        private Func<CancellationToken, Action, Exception, object> _throw;
        private Func<CancellationToken, Action, Exception, object> _throwBlocking;
        private ManualResetEventSlim _instructionHoldingEvent;
        private ManualResetEventSlim _instructionNotifyingEvent;
        private object _argumentSuccessful;
        private Exception _argumentFailure;

        #region setup

        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _instructionHoldingEvent = new ManualResetEventSlim(false);
            _instructionNotifyingEvent = new ManualResetEventSlim(false);
            _identity = (_, p, a) => a;
            _identityBlocking = (ct, p, a) =>
            {
                _instructionNotifyingEvent.Set();
                _instructionHoldingEvent.Wait(ct);
                return a;
            };
            _throw = (_, p, e) => { throw e; };
            _throwBlocking = (ct, p, e) =>
                {
                    _instructionNotifyingEvent.Set();
                    _instructionHoldingEvent.Wait(ct);
                    throw e;
                };
            _argumentSuccessful = new object();
            _argumentFailure = new Exception("Expected");
        }

        #endregion

        #region tests

        #region successful cancelled state
        [Test]
        public void SequentialCancellationSuccessful()
        {
            Future<object> future = _executor.Execute(_identity, _argumentSuccessful);
            future.Wait();
            future.Cancel();
            Assert.That(future.IsCompleted, Is.True);
        }

        [Test]
        public void SequentialCancellationFailure()
        {
            Future<object> future = _executor.Execute(_throw, _argumentFailure);
            future.Wait();
            future.Cancel();
            Assert.That(future.IsCompleted, Is.True);
        }

        [Test]
        public void BlockingCancellationSuccessful()
        {
            Future<object> future = _executor.Execute(_identityBlocking, _argumentSuccessful);
            //wait for executor to be in middle of instruction
            _instructionNotifyingEvent.Wait();
            future.Cancel();
            //wait for cancel to be processed
            future.Wait();
            Assert.That(future.IsCanceled, Is.True);
        }

        [Test]
        public void BlockingCancellationFailure()
        {
            Future<object> future = _executor.Execute(_throwBlocking, _argumentFailure);
            //wait for executor to be in middle of instruction
            _instructionNotifyingEvent.Wait();
            future.Cancel();
            //wait for cancel to be processed
            future.Wait();
            Assert.That(future.IsCanceled, Is.True);
        }
        #endregion

        #region multiple parallel tests
        [Test]
        public void CancelMultiSuccessful()
        {
            var future1 = _executor.Execute(_identityBlocking, _argumentSuccessful);
            var future2 = _executor.Execute(_identityBlocking, _argumentSuccessful);
            _instructionNotifyingEvent.Wait();

            Assert.That(future1.IsDone, Is.False);
            Assert.That(future2.IsDone, Is.False);

            var futures = new[] {future1, future2};
            Future<object>.CancelAll(futures);
            //wait for cancel to be processed
            Future<object>.WaitAll(futures);

            Assert.That(future1.IsCompleted, Is.True);
            Assert.That(future2.IsCompleted, Is.True);
        }
        #endregion

        #endregion
    }
}
