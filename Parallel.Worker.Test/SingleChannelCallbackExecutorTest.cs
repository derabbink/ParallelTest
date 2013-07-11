using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Parallel.Worker.Communication.SingleChannelCallback;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Test
{
    [TestFixture]
    public class SingleChannelCallbackExecutorTest
    {
        private SingleChannelCallbackExecutor<object, object> _successExecutor;
        private SingleChannelCallbackExecutor<Exception, object> _failureExecutor;
        private Channel<object, object> _successChannel;
        private Channel<Exception, object> _failureChannel;
        private Func<object, object> _identity;
        private Func<Exception, object> _throw;

        #region setup

        [SetUp]
        public void Setup()
        {
            _identity = a => a;
            _throw = e => { throw e; };
            _successChannel = new Channel<object, object>();
            _failureChannel = new Channel<Exception, object>();
            _successExecutor = new SingleChannelCallbackExecutor<object, object>(_successChannel, _successChannel);
            _failureExecutor = new SingleChannelCallbackExecutor<Exception, object>(_failureChannel, _failureChannel);
        }

        #endregion

        #region tests

        [Test]
        public void ExecuteSuccessful()
        {
            var argument = new object();
            Future<SafeInstructionResult<object>> future = _successExecutor.Execute(_identity, argument);
            future.Wait();
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Failed));
            Assert.That(future.Result.Value, Is.SameAs(argument));
        }

        [Test]
        public void ExecuteFailure()
        {
            var expectedException = new Exception("Expected");
            Future<SafeInstructionResult<object>> future = _failureExecutor.Execute(_throw, expectedException);
            future.Wait();
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Failed));
            Assert.That(future.Result.Exception, Is.SameAs(expectedException));
        }

        #endregion
    }
}
