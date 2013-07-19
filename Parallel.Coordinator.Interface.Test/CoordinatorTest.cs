using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private ManualResetEventSlim _notify;
        private ManualResetEventSlim _hold;
        private Func<CancellationToken, Action, object, object> _identity;
        private Func<CancellationToken, Action, Exception, Exception> _identityEx;
        private Func<CancellationToken, Action, object, object> _blockingIdentity;
        private Func<CancellationToken, Action, Exception, object> _throw;
        private CoordinatedInstruction<object, object> _identityInstr;
        private CoordinatedInstruction<object, object> _blockingIdentityInstr;
        private CoordinatedInstruction<Exception, Exception> _identityInstrEx;
        private CoordinatedInstruction<Exception, object> _throwInstr;
        private int _timeout ;
        private object _identityArgument;
        private Exception _throwArgument;

        #region setup
        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _notify = new ManualResetEventSlim(false);
            _hold = new ManualResetEventSlim(false);
            _identity = (_, p, a) => a;
            _identityEx = (_, p, e) => e;
            _blockingIdentity = (ct, p, a) =>
                {
                    _notify.Set();
                    _hold.Wait(ct);
                    return a;
                };
            _throw = (_, p, e) => { throw e; };
            _timeout = -1;
            _identityInstr = new CoordinatedInstruction<object, object>(_executor, _identity, _timeout);
            _blockingIdentityInstr = new CoordinatedInstruction<object, object>(_executor, _blockingIdentity, _timeout);
            _identityInstrEx = new CoordinatedInstruction<Exception, Exception>(_executor, _identityEx, _timeout);
            _throwInstr = new CoordinatedInstruction<Exception, object>(_executor, _throw, _timeout);
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

        [Test]
        public void CancellationWorks()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task t = Task.Factory.StartNew(() =>
                {
                    Parallel.Coordinator.Interface.Coordinator.Do(cts.Token, _blockingIdentityInstr, _identityArgument);
                });
            _notify.Wait();
            cts.Cancel();
            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds extra layer of AggregateException
                Assert.That(e.InnerException.InnerException, Is.TypeOf<TaskCanceledException>());
            }
        }
        #endregion
    }
}
