using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Parallel.Coordinator.Interface.Instruction;
using Parallel.Worker;
using Parallel.Worker.Interface;

namespace Prallel.Coordinator.Interface.Test
{
    [TestFixture]
    public class CoordinatorTest
    {
        private IExecutor _executor;
        private Func<CancellationToken, Action, object, object> _identity;
        private Func<CancellationToken, Action, Exception, Exception> _identityEx;
        private Func<CancellationToken, Action, Exception, object> _throw;
        private CoordinatedInstruction<object, object> _identityInstr;
        private CoordinatedInstruction<Exception, Exception> _identityInstrEx;
        private CoordinatedInstruction<Exception, object> _throwInstr;
        private object _identityArgument;
        private Exception _throwArgument;

        #region setup
        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _identity = (_, p, a) => a;
            _identityEx = (_, p, e) => e;
            _throw = (_, p, e) => { throw e; };
            _identityInstr = new CoordinatedInstruction<object, object>(_executor, _identity);
            _identityInstrEx = new CoordinatedInstruction<Exception, Exception>(_executor, _identityEx);
            _throwInstr = new CoordinatedInstruction<Exception, object>(_executor, _throw);
            _identityArgument = new object();
            _throwArgument = new Exception("Expected");
        }
        #endregion

        #region tests

        [Test]
        public void DoSingleOperation()
        {
            var expected = _identityArgument;
            var coordinator = Parallel.Coordinator.Interface.Coordinator.Do(_identityInstr, _identityArgument);
            var actual = coordinator.Result;
            Assert.That(actual.IsDone, Is.True);
            Assert.That(actual.Result, Is.SameAs(expected));
        }

        [Test]
        public void DoSingleOperationError()
        {
            var expected = _throwArgument;
            try
            {
                Parallel.Coordinator.Interface.Coordinator.Do(_throwInstr, _throwArgument);
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.SameAs(expected));
            }
        }

        [Test]
        public void Do2ChainedOperations()
        {
            var expected = _identityArgument;
            var coordinator = Parallel.Coordinator.Interface.Coordinator.Do(_identityInstr, _identityArgument)
                .ThenDo(_identityInstr);
            var actual = coordinator.Result;
            Assert.That(actual.IsDone, Is.True);
            Assert.That(actual.Result, Is.SameAs(expected));
        }

        [Test]
        public void Do2ChainedOperationsError()
        {
            var expected = _throwArgument;
            var coordinator = Parallel.Coordinator.Interface.Coordinator.Do(_identityInstrEx, _throwArgument);
            try
            {
                coordinator.ThenDo(_throwInstr);
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.SameAs(expected));
            }
        }

        [Test]
        public void Do3ChainedOperations()
        {
            var expected = _identityArgument;
            var coordinator = Parallel.Coordinator.Interface.Coordinator.Do(_identityInstr, _identityArgument)
                .ThenDo(_identityInstr)
                .ThenDo(_identityInstr);
            var actual = coordinator.Result;
            Assert.That(actual.IsDone, Is.True);
            Assert.That(actual.Result, Is.SameAs(expected));
        }

        [Test]
        public void Do3ChainedOperationsError()
        {
            var expected = _throwArgument;
            var coordinator = Parallel.Coordinator.Interface.Coordinator.Do(_identityInstrEx, _throwArgument)
                                      .ThenDo(_identityInstrEx);
            try
            {
                coordinator.ThenDo(_throwInstr);
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.SameAs(expected));
            }
        }
        #endregion
    }
}
