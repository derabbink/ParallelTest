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
    public class GenericExecutorTest
    {
        private Executor<object, object> _successExecutor;
        private Executor<Exception, object> _failureExecutor;
        private Func<CancellationToken, Action, object, object> _identity;
        private object _argumentSuccessful;
        private Func<CancellationToken, Action, Exception, object> _throw;
        private Exception _argumentFailure;

        #region setup

        [SetUp]
        public void Setup()
        {
            _successExecutor = new Executor<object, object>();
            _failureExecutor = new Executor<Exception, object>();
            _identity = (_, p, a) => a;
            _argumentSuccessful = new object();
            _throw = (_, p, e) => { throw e; };
            _argumentFailure = new Exception("Expected");
        }

        #endregion

        #region tests

        [Test]
        public void ExecuteProducesCompleteFuture()
        {
            var future = _successExecutor.Execute(_identity, _argumentSuccessful);
            //no wait
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
            Assert.That(future.Exception.InnerException, Is.SameAs(expected));
        }

        #endregion
    }
}
