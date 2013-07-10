using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private Executor<object, object> _successExecutor;
        private Executor<Exception, object> _failureExecutor;
        private Func<object, object> _identity;
        private object _argumentSuccessful;
        private Func<Exception, object> _throw;
        private Func<object, object> _identityBlocking;
        private ManualResetEvent _instructionBlockingResetEvent;
        private Exception _argumentFailure;

        #region setup

        [SetUp]
        public void Setup()
        {
            _successChannel = new Channel<object, object>();
            _failureChannel = new Channel<Exception, object>();
            _successExecutor = new SingleChannelCallbackTaskExecutor<object, object>(_successChannel, _successChannel);
            _failureExecutor = new SingleChannelCallbackTaskExecutor<Exception, object>(_failureChannel, _failureChannel);
            _identity = a => a;
            _instructionBlockingResetEvent = new ManualResetEvent(false);
            _identityBlocking = a =>
                {
                    _instructionBlockingResetEvent.WaitOne();
                    return a;
                };
            _argumentSuccessful = new object();
            _throw = e => { throw e; };
            _argumentFailure = new Exception("Expected");
        }

        #endregion

        #region tests

        [Test]
        public void ExecuteProducesIncompleteFuture()
        {
            var future = _successExecutor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.State, Is.EqualTo(Future.FutureState.PreExecution).Or.EqualTo(Future.FutureState.Executing));
            //cleanup
            _instructionBlockingResetEvent.Set();
        }

        [Test]
        public void ExecuteProducesIncompleteFutureThatCompletesEventually()
        {
            var future = _successExecutor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.State, Is.EqualTo(Future.FutureState.PreExecution).Or.EqualTo(Future.FutureState.Executing));
            _instructionBlockingResetEvent.Set();
            future.Wait();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
        }

        [Test]
        public void ExecuteSuccessful()
        {
            var expected = _argumentSuccessful;
            var future = _successExecutor.Execute(_identity, _argumentSuccessful);
            future.Wait();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(future.Result.Value, Is.SameAs(expected));
        }

        [Test]
        public void ExecuteFailure()
        {
            var expected = _argumentFailure;
            var future = _failureExecutor.Execute(_throw, _argumentFailure);
            future.Wait();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Failed));
            Assert.That(future.Result.Exception, Is.SameAs(expected));
        }

        #endregion
    }
}
