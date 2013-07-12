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
    public class GenericExecutorCancelTest
    {
        private Executor<object, object> _executorSuccessful;
        private Executor<Exception, object> _executorFailure;
        private Func<object, object> _identity;
        private Func<Exception, object> _throw;
        private object _argumentSuccessful;
        private Exception _argumentFailure;

        #region setup
        
        [SetUp]
        public void Setup()
        {
            _executorSuccessful = new Executor<object, object>();
            _executorFailure = new Executor<Exception, object>();
            _identity = a => a;
            _throw = e => { throw e; };
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
            //future.Wait() not required
            future.Cancel();
            Assert.That(future.IsCompleted, Is.True);
        }

        [Test]
        public void SequentialCancellationFailure()
        {
            Future<SafeInstructionResult<object>> future = _executorFailure.Execute(_throw, _argumentFailure);
            //future.Wait() not required
            future.Cancel();
            Assert.That(future.IsCompleted, Is.True);
        }
        #endregion

        #endregion
    }
}
