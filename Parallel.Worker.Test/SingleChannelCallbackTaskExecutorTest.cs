using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Parallel.Worker.Communication.SingleChannelCallback;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Test
{
    [TestFixture]
    public class SingleChannelCallbackTaskExecutorTest
    {
        private Channel<object, object> _successChannel;
        private Channel<Exception, object> _failureChannel;
        private IExecutor<object, object> _successExecutor;
        private IExecutor<Exception, object> _failureExecutor;
        private Func<CancellationToken, Action, object, object> _identity;
        private object _argumentSuccessful;
        private Func<CancellationToken, Action, Exception, object> _throw;
        private Func<CancellationToken, Action, object, object> _identityBlocking;
        private ManualResetEventSlim _instructionBlockingResetEvent;
        private Exception _argumentFailure;

        #region setup

        [SetUp]
        public void Setup()
        {
            _successChannel = new Channel<object, object>();
            _failureChannel = new Channel<Exception, object>();
            _successExecutor = new SingleChannelCallbackTaskExecutor<object, object>(_successChannel, _successChannel);
            _failureExecutor = new SingleChannelCallbackTaskExecutor<Exception, object>(_failureChannel, _failureChannel);
            _identity = (_, p, a) => a;
            _instructionBlockingResetEvent = new ManualResetEventSlim(false);
            _identityBlocking = (ct, p, a) =>
                {
                    _instructionBlockingResetEvent.Wait(ct);
                    return a;
                };
            _argumentSuccessful = new object();
            _throw = (_, p, e) => { throw e; };
            _argumentFailure = new Exception("Expected");
        }

        #endregion

        #region tests

        [Test]
        public void ExecuteProducesIncompleteFuture()
        {
            var future = _successExecutor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.IsDone, Is.False);
            //cleanup
            _instructionBlockingResetEvent.Set();
        }

        [Test]
        public void ExecuteProducesIncompleteFutureThatCompletesEventually()
        {
            var future = _successExecutor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.IsDone, Is.False);
            _instructionBlockingResetEvent.Set();
            future.Wait();
            Assert.That(future.IsCompleted, Is.True);
        }

        [Test]
        public void ExecuteSuccessful()
        {
            var expected = _argumentSuccessful;
            var future = _successExecutor.Execute(_identity, _argumentSuccessful);
            future.Wait();
            Assert.That(future.IsCompleted, Is.True);
            Assert.That(future.Result, Is.SameAs(expected));
        }

        [Test]
        public void ExecuteFailure()
        {
            var expected = _argumentFailure;
            var future = _failureExecutor.Execute(_throw, _argumentFailure);
            future.Wait();
            Assert.That(future.IsFaulted, Is.True);
            //double layer of AggregateException, since between (remote)
            // invoke and callback, there is a nested future
            Assert.That(future.Exception.InnerException.InnerException, Is.SameAs(expected));
        }

        #endregion
    }
}
