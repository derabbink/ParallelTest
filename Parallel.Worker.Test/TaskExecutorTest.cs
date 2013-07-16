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
        private Func<CancellationToken, Action, object, object> _identity;
        private object _argumentSuccessful;
        private Func<CancellationToken, Action, Exception, object> _throw;
        private Func<CancellationToken, Action, object, object> _identityBlocking;
        private ManualResetEventSlim _instructionHoldingEvent;
        private ManualResetEventSlim _instructionNotifyingEvent;
        private Exception _argumentFailure;

        #region setup

        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _identity = (_, p, a) => a;
            _instructionNotifyingEvent = new ManualResetEventSlim(false);
            _instructionHoldingEvent = new ManualResetEventSlim(false);
            _identityBlocking = (ct, p, a) =>
                {
                    _instructionNotifyingEvent.Set();
                    _instructionHoldingEvent.Wait(ct);
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
            var future = _executor.Execute(_identityBlocking, _argumentSuccessful);
            //_instructionNotifyingEvent.WaitOne(); //makes sure future is in Executing state
            Assert.That(future.IsDone, Is.False);
            //cleanup
            _instructionHoldingEvent.Set();
        }

        [Test]
        public void ExecuteProducesIncompleteFutureThatCompletesEventually()
        {
            var future = _executor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.IsDone, Is.False);
            _instructionHoldingEvent.Set();
            future.Wait();
            Assert.That(future.IsCompleted, Is.True);
        }

        [Test]
        public void ExecuteSuccessful()
        {
            var expected = _argumentSuccessful;
            var future = _executor.Execute(_identity, _argumentSuccessful);
            future.Wait();
            Assert.That(future.IsCompleted, Is.True);
            Assert.That(future.Result, Is.SameAs(expected));
        }

        [Test]
        public void ExecuteFailure()
        {
            var expected = _argumentFailure;
            var future = _executor.Execute(_throw, _argumentFailure);
            future.Wait();
            Assert.That(future.IsFaulted, Is.True);
            Assert.That(future.Exception.InnerException, Is.SameAs(expected));
        }

        #region multiple parallel tests
        [Test]
        public void ExecuteMultiSuccessful()
        {
            var expected = _argumentSuccessful;
            var future1 = _executor.Execute(_identityBlocking, _argumentSuccessful);
            var future2 = _executor.Execute(_identityBlocking, _argumentSuccessful);
            _instructionNotifyingEvent.Wait();

            Assert.That(future1.IsDone, Is.False);
            Assert.That(future2.IsDone, Is.False);
            
            Task.Factory.StartNew(() => _instructionHoldingEvent.Set());
            Future.WaitAll(new[] {future1, future2});
            
            Assert.That(future1.IsCompleted, Is.True);
            Assert.That(future2.IsCompleted, Is.True);
            Assert.That(future1.Result, Is.SameAs(expected));
            Assert.That(future2.Result, Is.SameAs(expected));
        }
        #endregion

        #endregion
    }
}
