using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Parallel.Worker.Interface;

namespace Parallel.Worker.Test
{
    [TestFixture]
    public class GenericTaskExecutorCancelTest
    {
        private Executor<object, object> _executorSuccessful;
        private Executor<Exception, object> _executorFailure;
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
            Future<object> future = _executorSuccessful.Execute(_identity, _argumentSuccessful);
            future.Wait();
            future.Cancel();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
        }

        [Test]
        public void SequentialCancellationFailure()
        {
            Future<object> future = _executorFailure.Execute(_throw, _argumentFailure);
            future.Wait();
            future.Cancel();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
        }

        [Test]
        public void BlockingCancellationSuccessful()
        {
            Future<object> future = _executorSuccessful.Execute(_identityBlocking, _argumentSuccessful);
            //wait for executor to be in middle of instruction
            _instructionNotifyingEvent.WaitOne();
            future.Cancel();
            //let instruction continue
            _instructionHoldingEvent.Set();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Cancelled));
        }

        [Test]
        public void BlockingCancellationFailure()
        {
            Future<object> future = _executorFailure.Execute(_throwBlocking, _argumentFailure);
            //wait for executor to be in middle of instruction
            _instructionNotifyingEvent.WaitOne();
            future.Cancel();
            //let instruction continue
            _instructionHoldingEvent.Set();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Cancelled));
        }
        #endregion

        #endregion
    }
}
