using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface.Test
{
    [TestFixture]
    public class FutureStateTest
    {
        #region tests

        #region valid state transitions
        [Test]
        public void InitialStateCorrect()
        {
            Future future = new Future();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.PreExecution));
        }

        [Test]
        public void TransitionToExecutingStateCorrect()
        {
            Future future = new Future();
            future.SetExecuting();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Executing));
        }

        [Test]
        public void TransitionToCompletedStateCorrect()
        {
            Future future = new Future();
            future.SetExecuting();
            future.SetCompleted();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
        }
        #endregion

        #region invalid state transitions
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidTanstionToCompletedFails()
        {
            Future future = new Future();
            future.SetCompleted();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidReverseTanstionToExecutingFails()
        {
            Future future = new Future();
            future.SetExecuting();
            future.SetCompleted();
            future.SetExecuting();
        }
        #endregion

        #region valid result reading
        [Test]
        public void ReadingResultWhileCompletedCorrect()
        {
            object expected = new object();
            Future<object> future = new Future<object>();
            future.SetExecuting();
            future.SetCompleted(SafeInstructionResult<object>.Succeeded(expected));
            var actual = future.Result;
            Assert.That(actual.Value, Is.SameAs(expected));
        }

        [Test]
        public void ReadingResultWhileCompletedCancelledCorrect()
        {
            object expected = new object();
            Future<object> future = new Future<object>();
            future.SetExecuting();
            future.SetCompleted(SafeInstructionResult<object>.Succeeded(expected));
            future.Cancel();
            var actual = future.Result;
            Assert.That(actual.Value, Is.SameAs(expected));
        }
        #endregion

        #region invalid/premature result reading
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadingResultWhilePreExecutingFails()
        {
            Future<object> future = new Future<object>();
            var result = future.Result;
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void ReadingResultWhileExecutingFails()
        {
            Future<object> future = new Future<object>();
            future.SetExecuting();
            var result = future.Result;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadingResultWhilePreExecutingCancelledFails()
        {
            Future<object> future = new Future<object>();
            future.Cancel();
            var result = future.Result;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadingResultWhileExecutingCancelledFails()
        {
            Future<object> future = new Future<object>();
            future.SetExecuting();
            future.Cancel();
            var result = future.Result;
        }
        #endregion

        #region state transitions to Cancelled
        [Test]
        public void CancelPreExecuting()
        {
            Future future = new Future();
            future.Cancel();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Cancelled));
        }

        [Test]
        public void CancelExecuting()
        {
            Future future = new Future();
            future.SetExecuting();
            future.Cancel();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Cancelled));
        }

        [Test]
        public void CancelCompletedIsIgnored()
        {
            Future future = new Future();
            future.SetExecuting();
            future.SetCompleted();
            future.Cancel();
            Assert.That(future.State, Is.EqualTo(Future.FutureState.Completed));
        }
        #endregion

        #region invalid state transitions from Cancelled
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ContinueFromCancelledPreExecutingFails()
        {
            Future future = new Future();
            future.Cancel();
            future.SetExecuting();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ContinueFromCancelledExecutingFails()
        {
            Future future = new Future();
            future.SetExecuting();
            future.Cancel();
            future.SetCompleted();
        }
        #endregion

        #endregion
    }
}
