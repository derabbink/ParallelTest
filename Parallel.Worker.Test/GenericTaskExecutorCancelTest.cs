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
    public class GenericTaskExecutorCancelTest
    {
        private IExecutor<object, object> _executorSuccessful;
        private IExecutor<Exception, object> _executorFailure;
        private Func<CancellationToken, IProgress, object, object> _identity;
        private Func<CancellationToken, IProgress, object, object> _identityBlocking;
        private Func<CancellationToken, IProgress, Exception, object> _throw;
        private Func<CancellationToken, IProgress, Exception, object> _throwBlocking;
        private ManualResetEventSlim _instructionHoldingEvent;
        private ManualResetEventSlim _instructionNotifyingEvent;
        private object _argumentSuccessful;
        private Exception _argumentFailure;

        #region setup

        [SetUp]
        public void Setup()
        {
            _executorSuccessful = new TaskExecutor<object, object>();
            _executorFailure = new TaskExecutor<Exception, object>();
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

        #region cancelling completed future has no effect
        [Test]
        public void SequentialCancellationSuccessfulNoEffect()
        {
            Future<object> future = _executorSuccessful.Execute(_identity, _argumentSuccessful);
            future.Wait();
            future.Cancel();
            Assert.That(future.IsCanceled, Is.False);
        }

        [Test]
        public void SequentialCancellationFailureHasNoEffect()
        {
            Future<object> future = _executorFailure.Execute(_throw, _argumentFailure);
            future.Wait();
            future.Cancel();
            Assert.That(future.IsCanceled, Is.False);
        }
        #endregion

        #region successful cancelled state
        [Test]
        public void BlockingCancellationSuccessful()
        {
            Future<object> future = _executorSuccessful.Execute(_identityBlocking, _argumentSuccessful);
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
            Future<object> future = _executorFailure.Execute(_throwBlocking, _argumentFailure);
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
            var future1 = _executorSuccessful.Execute(_identityBlocking, _argumentSuccessful);
            var future2 = _executorSuccessful.Execute(_identityBlocking, _argumentSuccessful);
            _instructionNotifyingEvent.Wait();

            Assert.That(future1.IsDone, Is.False);
            Assert.That(future1.IsDone, Is.False);

            var futures = new[] {future1, future2};
            Future<object>.CancelAll(futures);
            //wait for cancel to be processed
            Future<object>.WaitAll(futures);
            Assert.That(future1.IsCanceled, Is.True);
            Assert.That(future2.IsCanceled, Is.True);
        }
        #endregion

        #endregion
    }
}
