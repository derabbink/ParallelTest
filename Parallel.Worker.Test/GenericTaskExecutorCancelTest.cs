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
        private Func<object, object> _identity;
        private Func<object, object> _identityBlocking;
        private Func<Exception, object> _throw;
        private Func<Exception, object> _throwBlocking;
        private ManualResetEvent _instructionHoldingEvent;
        private ManualResetEvent _instructionNotifyingEvent;
        private object _argumentSuccessful;
        private Exception _argumentFailure;

        #region setup

        [SetUp]
        public void Setup()
        {
            _executorSuccessful = new TaskExecutor<object, object>();
            _executorFailure = new TaskExecutor<Exception, object>();
            _instructionHoldingEvent = new ManualResetEvent(false);
            _instructionNotifyingEvent = new ManualResetEvent(false);
            _identity = a => a;
            _identityBlocking = a =>
                {
                    _instructionNotifyingEvent.Set();
                    _instructionHoldingEvent.WaitOne();
                    return a;
                };
            _throw = e => { throw e; };
            _throwBlocking = e =>
                {
                    _instructionNotifyingEvent.Set();
                    _instructionHoldingEvent.WaitOne();
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
            Future<SafeInstructionResult<object>> future = _executorSuccessful.Execute(_identity, _argumentSuccessful);
            future.Wait();
            future.Cancel();
            Assert.That(future.IsCanceled, Is.True);
        }

        [Test]
        public void SequentialCancellationFailure()
        {
            Future<SafeInstructionResult<object>> future = _executorFailure.Execute(_throw, _argumentFailure);
            future.Wait();
            future.Cancel();
            Assert.That(future.IsCanceled, Is.True);
        }

        [Test]
        public void BlockingCancellationSuccessful()
        {
            Future<SafeInstructionResult<object>> future = _executorSuccessful.Execute(_identityBlocking, _argumentSuccessful);
            //wait for executor to be in middle of instruction
            _instructionNotifyingEvent.WaitOne();
            future.Cancel();
            //let instruction continue
            _instructionHoldingEvent.Set();
            Assert.That(future.IsCanceled, Is.True);
        }

        [Test]
        public void BlockingCancellationFailure()
        {
            Future<SafeInstructionResult<object>> future = _executorFailure.Execute(_throwBlocking, _argumentFailure);
            //wait for executor to be in middle of instruction
            _instructionNotifyingEvent.WaitOne();
            future.Cancel();
            //let instruction continue
            _instructionHoldingEvent.Set();
            Assert.That(future.IsCanceled, Is.True);
        }
        #endregion

        #region multiple parallel tests
        [Test]
        public void CancelMultiSuccessful()
        {
            var future1 = _executorSuccessful.Execute(_identityBlocking, _argumentSuccessful);
            var future2 = _executorSuccessful.Execute(_identityBlocking, _argumentSuccessful);
            _instructionNotifyingEvent.WaitOne();

            Assert.That(future1.IsDone, Is.False);
            Assert.That(future1.IsDone, Is.False);

            Future<SafeInstructionResult<object>>.CancelAll(new[] { future1, future2 });

            Assert.That(future1.IsCanceled, Is.True);
            Assert.That(future2.IsCanceled, Is.True);
        }
        #endregion

        #endregion
    }
}
