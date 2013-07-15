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
        private Func<CancellationToken, object, object> _identity;
        private CoordinatedInstruction<object, object> _identityInstr;
        private Func<CancellationToken, Future<object>, object> _unwrap;
        private CoordinatedInstruction<Future<object>, object> _unwrapInstr;
        private object _identityArgument;

        #region setup
        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _identity = (_, a) => a;
            _identityInstr = new CoordinatedInstruction<object, object>(_executor, _identity);
            _unwrap = (_, f) => f.Unwrap();
            _unwrapInstr = new CoordinatedInstruction<Future<object>, object>(_executor, _unwrap);
            _identityArgument = new object();
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
        public void Do2ChainedOperations()
        {
            var expected = _identityArgument;
            var coordinator = Parallel.Coordinator.Interface.Coordinator.Do(_identityInstr, _identityArgument).ThenDo(_unwrapInstr);
            var actual = coordinator.Result;
            Assert.That(actual.IsDone, Is.True);
            Assert.That(actual.Result, Is.SameAs(expected));
        }
        #endregion
    }
}
