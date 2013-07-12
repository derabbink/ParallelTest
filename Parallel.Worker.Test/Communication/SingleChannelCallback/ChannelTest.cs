using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Parallel.Worker.Communication.SingleChannelCallback;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Events.SingleChannelCallback;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Test.Communication.SingleChannelCallback
{
    [TestFixture]
    public class ChannelTest
    {
        private Channel<object, object> _channelSuccess;
        private Channel<Exception, object> _channelFail;
        private Guid _operationId;
        private Func<CancellationToken, object, object> _identity;
        private Func<CancellationToken, Exception, object> _throw;
        private ManualResetEvent _callbackCompleted;
        private Future<object> _operationResult;
        private EventHandler<CallbackEventArgs<object>> _callbackHandler;

        #region setup

        [SetUp]
        public void Setup()
        {
            _channelSuccess = new Channel<object, object>();
            _channelFail = new Channel<Exception, object>();
            _operationId = new Guid();
            _identity = (_, a) => a;
            _throw = (_, e) => { throw e; };
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
            _channelSuccess.Run(_operationId, _identity, argument, _channelSuccess);
            _callbackCompleted.WaitOne();
            Assert.That(_operationResult.IsCompleted, Is.True);
            Assert.That(_operationResult.Result, Is.SameAs(argument));
        }

        [Test]
        public void RunFailure()
        {
            var expectedException = new Exception("Expected");
            _channelFail.Run(_operationId, _throw, expectedException, _channelFail);
            _callbackCompleted.WaitOne();
            Assert.That(_operationResult.IsFaulted, Is.True);
            Assert.That(_operationResult.Exception.InnerException, Is.SameAs(expectedException));
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
