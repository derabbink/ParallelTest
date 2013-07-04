using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using NUnit.Framework;
using Parallel.Worker.Interface.Rx;

namespace Parallel.Worker.Interface.Test
{
    [TestFixture]
    public class WorkerTest
    {
        private Worker _worker;

        #region Setup

        [SetUp]
        public void Setup()
        {
            CreateWorker();
        }

        private void CreateWorker()
        {
            _worker = new Worker();
        }

        #endregion

        [Test]
        public void ExecuteProducesCorrectMessages()
        {
            Action op = CreateOperation();
            var actualObs = _worker.Execute(op);
            var actual = actualObs.ToEnumerable().GetEnumerator();
            var expected = CreateOperationUpdates();
            
            foreach (var opExpected in expected)
            {
                actual.MoveNext();
                var opActual = actual.Current;
                Assert.That(opActual.GetType(), Is.EqualTo(opExpected.GetType()));
            }
            Assert.That(actual.MoveNext(), Is.False);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "ExpectedError")]
        public void ExecuteFailingProducesCorrectError()
        {
            var expectedError = "ExpectedError";
            Action op = CreateFailingOperation(expectedError);
            var actualObs = _worker.Execute(op);
            var actual = actualObs.ToEnumerable().GetEnumerator();
            var expected = CreateFailingOperationUpdates();

            foreach (var opExpected in expected)
            {
                actual.MoveNext();
                var opActual = actual.Current;
                Assert.That(opActual.GetType(), Is.EqualTo(opExpected.GetType()));
            }
            //exception comes after all messages have been checked
            //exception will inhibit assertion execution
            Assert.That(actual.MoveNext(), Is.False);
        }

        private Action CreateOperation()
        {
            return () => { };
        }

        private IEnumerable<OperationProgress> CreateOperationUpdates()
        {
            object operationResult = null;
            Guid operationId = new Guid();
            return new OperationProgress[]
                {
                    new OperationStarted(operationId),
                    new OperationCompletedSuccess(operationResult, operationId)
                }.AsEnumerable();
        }

        private Action CreateFailingOperation(string message)
        {
            return () =>
                {
                    throw new Exception(message);
                };
        }

        private IEnumerable<OperationProgress> CreateFailingOperationUpdates()
        {
            object operationResult = null;
            Guid operationId = new Guid();
            return new OperationProgress[]
                {
                    new OperationStarted(operationId)
                }.AsEnumerable();
        }
    }
}
