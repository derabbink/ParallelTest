﻿using System;
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
    public class CallbackExecutorTest
    {
        private IExecutor _executor;
        private Action<object, Action<object>> _identity;
        private object _argumentSuccessful;
        private Action<Exception, Action<object>> _throw;
        private Action<object, Action<object>> _identityBlocking;
        private ManualResetEvent _instructionBlockingResetEvent;
        private Exception _argumentFailure;

        #region setup

        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _identity = (arg, callback) => callback(arg);
            _instructionBlockingResetEvent = new ManualResetEvent(false);
            _identityBlocking = (arg, callback) =>
                {
                    _instructionBlockingResetEvent.WaitOne();
                    callback(arg);
                };
            _argumentSuccessful = new object();
            _throw = (e, _) => { throw e; };
            _argumentFailure = new Exception("Expected");
        }

        #endregion

        #region tests

        [Test]
        public void ExecuteProducesIncompleteFuture()
        {
            var future = _executor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.IsDone, Is.False);
            //cleanup
            _instructionBlockingResetEvent.Set();
        }

        [Test]
        public void ExecuteProducesIncompleteFutureThatCompletesEventually()
        {
            var future = _executor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.IsDone, Is.False);
            _instructionBlockingResetEvent.Set();
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
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(future.Result.Value, Is.SameAs(expected));
        }

        [Test]
        public void ExecuteFailure()
        {
            var expected = _argumentFailure;
            var future = _executor.Execute(_throw, _argumentFailure);
            future.Wait();
            Assert.That(future.IsCompleted, Is.True);
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Failed));
            Assert.That(future.Result.Exception, Is.SameAs(expected));
        }

        #endregion
    }
}
