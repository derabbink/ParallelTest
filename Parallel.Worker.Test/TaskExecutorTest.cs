using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Test
{
    [TestFixture]
    public class TaskExecutorTest
    {
        private Executor _executor;
        private Func<object, object> _identity;
        private object _argumentSuccessful;
        private Func<Exception, object> _throw;
        private Func<object, object> _identityBlocking;
        private ManualResetEvent _instructionBlockingResetEvent;
        private Exception _argumentFailure;
        
        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _identity = a => a;
            _instructionBlockingResetEvent = new ManualResetEvent(false);
            _identityBlocking = Wrapper.Wrap(() => { _instructionBlockingResetEvent.WaitOne(); });
            _argumentSuccessful = new object();
            _throw = e => { throw e; };
            _argumentFailure = new Exception("Expected");
        }

        [Test]
        public void ExecuteProducesIncompleteFuture()
        {
            var future = _executor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.State, Is.EqualTo(Future.FutureState.PreExecution).Or.EqualTo(Future.FutureState.Executing));
            //cleanup
            _instructionBlockingResetEvent.Set();
        }

        [Test]
        public void ExecuteProducesIncompleteFutureThatCompletesEventually()
        {
            var future = _executor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.State, Is.EqualTo(Future.FutureState.PreExecution).Or.EqualTo(Future.FutureState.Executing));
            _instructionBlockingResetEvent.Set();
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
    }
}
