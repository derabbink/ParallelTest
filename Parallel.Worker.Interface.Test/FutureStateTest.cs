using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Parallel.Worker.Interface.Test
{
    [TestFixture]
    public class FutureStateTest
    {
        #region tests

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

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidTanstionToCompletedFails()
        {
            Future future = new Future();
            future.SetCompleted();
        }

        #endregion
    }
}
