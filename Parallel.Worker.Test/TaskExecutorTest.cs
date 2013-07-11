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
    public class TaskExecutorTest
    {
        private IExecutor _executor;
        private Func<object, object> _identity;
        private object _argumentSuccessful;
        private Func<Exception, object> _throw;
        private Func<object, object> _identityBlocking;
        private ManualResetEvent _instructionHoldingEvent;
        private ManualResetEvent _instructionNotifyingEvent;
        private Exception _argumentFailure;

        #region setup

        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _identity = a => a;
            _instructionNotifyingEvent = new ManualResetEvent(false);
            _instructionHoldingEvent = new ManualResetEvent(false);
            _identityBlocking = a =>
                {
                    _instructionNotifyingEvent.Set();
                    _instructionHoldingEvent.WaitOne();
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
            var future = _executor.Execute(_identityBlocking, _argumentSuccessful);
            //_instructionNotifyingEvent.WaitOne(); //makes sure future is in Executing state
            Assert.That(future.State, Is.EqualTo(Future.FutureState.PreExecution).Or.EqualTo(Future.FutureState.Executing));
            //cleanup
            _instructionHoldingEvent.Set();
        }

        [Test]
        public void ExecuteProducesIncompleteFutureThatCompletesEventually()
        {
            var future = _executor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.State, Is.EqualTo(Future.FutureState.PreExecution).Or.EqualTo(Future.FutureState.Executing));
            _instructionHoldingEvent.Set();
            future.Wait();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
        }

        [Test]
        public void ExecuteSuccessful()
        {
            var expected = _argumentSuccessful;
            var future = _executor.Execute(_identity, _argumentSuccessful);
            future.Wait();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(future.Result.Value, Is.SameAs(expected));
        }

        [Test]
        public void ExecuteFailure()
        {
            var expected = _argumentFailure;
            var future = _executor.Execute(_throw, _argumentFailure);
            future.Wait();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Failed));
            Assert.That(future.Result.Exception, Is.SameAs(expected));
        }

        #region multiple parallel tests
        [Test]
        public void ExecuteMultiSuccessful()
        {
            var expected = _argumentSuccessful;
            var future1 = _executor.Execute(_identityBlocking, _argumentSuccessful);
            var future2 = _executor.Execute(_identityBlocking, _argumentSuccessful);
            _instructionNotifyingEvent.WaitOne();

            Assert.That(future1.State, Is.EqualTo(Future.FutureState.PreExecution).Or.EqualTo(Future.FutureState.Executing));
            Assert.That(future2.State, Is.EqualTo(Future.FutureState.PreExecution).Or.EqualTo(Future.FutureState.Executing));
            
            Task.Factory.StartNew(() => _instructionHoldingEvent.Set());
            Future.WaitAll(new[] {future1, future2});
            
            Assert.That(future1.State, Is.EqualTo(Future.FutureState.Completed));
            Assert.That(future2.State, Is.EqualTo(Future.FutureState.Completed));
            Assert.That(future1.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(future2.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(future1.Result.Value, Is.SameAs(expected));
            Assert.That(future2.Result.Value, Is.SameAs(expected));
        }
        #endregion

        #endregion
    }
}
