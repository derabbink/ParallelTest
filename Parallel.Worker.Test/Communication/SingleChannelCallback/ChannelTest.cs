using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Parallel.Worker.Communication.SingleChannelCallback;
using Parallel.Worker.Events.SingleChannelCallback;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Test.Communication.SingleChannelCallback
{
    [TestFixture]
    public class ChannelTest
    {
        private Channel<object, object> _channelSuccess;
        private Channel<Exception, object> _channelFail;
        private Guid _operationId;
        private Func<object, object> _identity;
        private Func<Exception, object> _throw;
        private ManualResetEvent _callbackCompleted;
        private SafeInstructionResult<object> _operationResult;
        private EventHandler<CallbackEventArgs<object>> _callbackHandler;

        #region setup

        [SetUp]
        public void Setup()
        {
            _channelSuccess = new Channel<object, object>();
            _channelFail = new Channel<Exception, object>();
            _operationId = new Guid();
            _identity = a => a;
            _throw = e => { throw e; };
            _callbackCompleted = new ManualResetEvent(false);
            _operationResult = null;
            _callbackHandler = (sender, args) =>
            {
                if (args.OperationId == _operationId)
                {
                    _operationResult = args.Result;
                    _callbackCompleted.Set();
                }
            };

            _channelSuccess.SubscribeCallbackEvent(_callbackHandler);
            _channelFail.SubscribeCallbackEvent(_callbackHandler);
        }

        #endregion

        #region tests

        [Test]
        public void RunSuccessful()
        {
            var argument = new object();
            _channelSuccess.Run(_operationId, new SafeInstruction<object, object>(_identity, argument), _channelSuccess);
            _callbackCompleted.WaitOne();
            Assert.That(_operationResult.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(_operationResult.Value, Is.SameAs(argument));
        }

        [Test]
        public void RunFailure()
        {
            var expectedException = new Exception("Expected");
            _channelFail.Run(_operationId, new SafeInstruction<Exception, object>(_throw, expectedException), _channelFail);
            _callbackCompleted.WaitOne();
            Assert.That(_operationResult.State, Is.EqualTo(SafeInstructionResult.ResultState.Failed));
            Assert.That(_operationResult.Exception, Is.SameAs(expectedException));
        }

        #endregion

        #region teardown

        [TearDown]
        public void Teardown()
        {
            _channelSuccess.UnsubscribeCallbackEvent(_callbackHandler);
            _channelFail.UnsubscribeCallbackEvent(_callbackHandler);
        }

        #endregion
    }
}
