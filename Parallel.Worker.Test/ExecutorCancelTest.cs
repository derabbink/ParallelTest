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
    public class ExecutorCancelTest
    {
        private Executor _executor;
        private Func<CancellationToken, Action, object, object> _identity;
        private Func<CancellationToken, Action, Exception, object> _throw;
        private object _argumentSuccessful;
        private Exception _argumentFailure;

        #region setup
        
        [SetUp]
        public void Setup()
        {
            _executor = new Executor();
            _identity = (_, p, a) => a;
            _throw = (_, p, e) => { throw e; };
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
            //future.Wait() not required
            future.Cancel();
            Assert.That(future.IsCanceled, Is.True);
        }

        [Test]
        public void SequentialCancellationFailure()
        {
            Future<object> future = _executor.Execute(_throw, _argumentFailure);
            //future.Wait() not required
            future.Cancel();
            Assert.That(future.IsCanceled, Is.True);
        }
        #endregion

        #endregion
    }
}
