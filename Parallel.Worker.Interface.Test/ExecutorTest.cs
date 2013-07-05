﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface.Test
{
    [TestFixture]
    public class ExecutorTest
    {
        private Executor _executor;
        private Func<object, object> _identity;
        private object _argumentSuccessful;
        private Func<Exception, object> _throw;
        private Exception _argumentFailure;
        
        [SetUp]
        public void Setup()
        {
            _executor = new Executor();
            _identity = a => a;
            _argumentSuccessful = new object();
            _throw = e => { throw e; };
            _argumentFailure = new Exception("Expected");
        }

        [Test]
        public void ExecuteProducesCompleteFuture()
        {
            var future = _executor.Execute(_identity, _argumentSuccessful);
            //no wait
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
        }

        [Test]
        public void ExecuteSuccessful()
        {
            var expected = _argumentSuccessful;
            var future = _executor.Execute(_identity, _argumentSuccessful);
            future.Wait();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(future.Result.Value, Is.SameAs(expected));
        }

        [Test]
        public void ExecuteFailure()
        {
            var expected = _argumentFailure;
            var future = _executor.Execute(_throw, _argumentFailure);
            future.Wait();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
            Assert.That(future.Result.State, Is.EqualTo(SafeInstructionResult.ResultState.Failed));
            Assert.That(future.Result.Exception, Is.SameAs(expected));
        }
    }
}
