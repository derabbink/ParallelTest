using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private Channel<object, object> _successChannel;
        private Func<object, object> _identity;
        private Func<Exception, object> _throw;

        #region setup

        [SetUp]
        public void Setup()
        {
            _identity = a => a;
            _throw = e => { throw e; };
            _successChannel = new Channel<object, object>();
            _successExecutor = new SingleChannelCallbackExecutor<object, object>(_successChannel, _successChannel);
        }

        #endregion

        #region tests

        [Test]
        public void ExecuteSuccessful()
        {
            var argument = new object();
            Future<object> future = _successExecutor.Execute(_identity, argument);
            future.Wait();
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(future.Result.Value, Is.SameAs(argument));
        }

        #endregion

        #region teardown


        #endregion
    }
}
