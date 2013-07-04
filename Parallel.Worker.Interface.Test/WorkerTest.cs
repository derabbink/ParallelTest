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
            var obsActual = _worker.Execute(op);
            var actual = obsActual.ToEnumerable().GetEnumerator();
            var expected = CreateOperationUpdates();
            
            foreach (var opExpected in expected)
            {
                actual.MoveNext();
                var opActual = actual.Current;
                Assert.That(opActual.GetType(), Is.EqualTo(opExpected.GetType()));
            }
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
    }
}
