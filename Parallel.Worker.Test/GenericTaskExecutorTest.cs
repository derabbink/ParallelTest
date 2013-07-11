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
    public class GenericTaskExecutorTest
    {
        private IExecutor<object, object> _successExecutor;
        private IExecutor<Exception, object> _failureExecutor;
        private Func<object, object> _identity;
        private object _argumentSuccessful;
        private Func<Exception, object> _throw;
        private Func<object, object> _identityBlocking;
        private ManualResetEvent _instructionHoldingEvent;
        private ManualResetEvent _instructionNotifyingEvent;
        private Exception _argumentFailure;

        #region setup

        [SetUp]
        public void Setup()
        {
            _successExecutor = new TaskExecutor<object, object>();
            _failureExecutor = new TaskExecutor<Exception, object>();
            _identity = a => a;
            _instructionNotifyingEvent = new ManualResetEvent(false);
            _instructionHoldingEvent = new ManualResetEvent(false);
            _identityBlocking = a =>
                {
                    _instructionNotifyingEvent.Set();
                    _instructionHoldingEvent.WaitOne();
                    return a;
                };
            _argumentSuccessful = new object();
            _throw = e => { throw e; };
            _argumentFailure = new Exception("Expected");
        }

        #endregion

        #region tests

        [Test]
        public void ExecuteProducesIncompleteFuture()
        {
            var future = _successExecutor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.Status, Is.Not.EqualTo(TaskStatus.RanToCompletion).And.Not.EqualTo(TaskStatus.Faulted).And.Not.EqualTo(TaskStatus.Canceled));
            //cleanup
            _instructionHoldingEvent.Set();
        }

        [Test]
        public void ExecuteProducesIncompleteFutureThatCompletesEventually()
        {
            var future = _successExecutor.Execute(_identityBlocking, _argumentSuccessful);
            Assert.That(future.Status, Is.Not.EqualTo(TaskStatus.RanToCompletion).And.Not.EqualTo(TaskStatus.Faulted).And.Not.EqualTo(TaskStatus.Canceled));
            _instructionHoldingEvent.Set();
            future.Wait();
            Assert.That(future.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }

        [Test]
        public void ExecuteSuccessful()
        {
            var expected = _argumentSuccessful;
            var future = _successExecutor.Execute(_identity, _argumentSuccessful);
            future.Wait();
            Assert.That(future.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(future.Result.Value, Is.SameAs(expected));
        }

        [Test]
        public void ExecuteFailure()
        {
            var expected = _argumentFailure;
            var future = _failureExecutor.Execute(_throw, _argumentFailure);
            future.Wait();
            Assert.That(future.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Failed));
            Assert.That(future.Result.Exception, Is.SameAs(expected));
        }

        #region multiple parallel tests
        [Test]
        public void ExecuteMultiSuccessful()
        {
            var expected = _argumentSuccessful;
            var future1 = _successExecutor.Execute(_identityBlocking, _argumentSuccessful);
            var future2 = _successExecutor.Execute(_identityBlocking, _argumentSuccessful);
            _instructionNotifyingEvent.WaitOne();

            Assert.That(future1.Status, Is.Not.EqualTo(TaskStatus.RanToCompletion).And.Not.EqualTo(TaskStatus.Faulted).And.Not.EqualTo(TaskStatus.Canceled));
            Assert.That(future2.Status, Is.Not.EqualTo(TaskStatus.RanToCompletion).And.Not.EqualTo(TaskStatus.Faulted).And.Not.EqualTo(TaskStatus.Canceled));

            Task.Factory.StartNew(() => _instructionHoldingEvent.Set());
            Future.WaitAll(new[] { future1, future2 });

            Assert.That(future1.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            Assert.That(future2.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            Assert.That(future1.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(future2.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(future1.Result.Value, Is.SameAs(expected));
            Assert.That(future2.Result.Value, Is.SameAs(expected));
        }
        #endregion

        #endregion
    }
}
