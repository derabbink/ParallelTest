using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private Func<CancellationToken, Action, object, object> _identity;
        private Func<CancellationToken, Action, Exception, object> _throw;

        #region setup

        [SetUp]
        public void Setup()
        {
            _identity = (_, p, a) => a;
            _throw = (_, p, e) => { throw e; };
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
            Future<object> future = _successExecutor.Execute(_identity, argument);
            future.Wait();
            Assert.That(future.IsCompleted, Is.True);
            Assert.That(future.Result, Is.SameAs(argument));
        }

        [Test]
        public void ExecuteFailure()
        {
            var expectedException = new Exception("Expected");
            Future<object> future = _failureExecutor.Execute(_throw, expectedException);
            try
            {
                future.Wait();
            }
            catch (AggregateException e)
            {
                //double layer of AggregateException, since between (remote)
                // invoke and callback, there is a nested future
                Assert.That(e.InnerException.InnerException, Is.SameAs(expectedException));
            }
            Assert.That(future.IsFaulted, Is.True);
            Assert.That(future.Exception.InnerException.InnerException, Is.SameAs(expectedException));
        }

        #endregion
    }
}
