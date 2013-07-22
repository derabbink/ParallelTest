using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Parallel.Coordinator.Interface.Instruction;

namespace Parallel.Coordinator.Test.Chaining.Instruction4
{
    [TestFixture]
    public class Chain4To1Test : ChainingTest
    {
        #region tests

        [Test]
        public void Do2ChainedOperations()
        {
            var expected = _argument1;
            var coordinator = Coordinator4.Do(_1_1Instr, _1_1Instr, _1_1Instr, _1_1Instr, _argument1)
                .ThenDo(_4_4Instr);
            var actual = coordinator.Result;
            Assert.That(actual.IsDone, Is.True);
            Assert.That(actual.Result.Item1, Is.SameAs(expected));
            Assert.That(actual.Result.Item2, Is.SameAs(expected));
            Assert.That(actual.Result.Item3, Is.SameAs(expected));
            Assert.That(actual.Result.Item4, Is.SameAs(expected));
        }

        [Test]
        public void Do2ChainedOperationsError()
        {
            var expected = _argument1;
            var coordinator = Coordinator4.Do(_1_1Instr, _1_1Instr, _1_1Instr, _1_1Instr, _argument1);
            try
            {
                coordinator.ThenDo(_throw4_4Instr);
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.SameAs(expected));
            }
        }

        [Test]
        public void Cancellation2ChainedWorks()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task t = Task.Factory.StartNew(() =>
            {
                Coordinator4.Do(cts.Token, _1_1Instr, _1_1Instr, _1_1Instr, _1_1Instr, _argument1).ThenDo(_blocking4_4Instr);
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

        [Test]
        public void Timeout2ChainedWorks()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task t = Task.Factory.StartNew(() =>
            {
                Coordinator4.Do(cts.Token, _1_1Instr, _1_1Instr, _1_1Instr, _1_1Instr, _argument1).ThenDo(_timeoutable4_4Instr);
            });
            _notify.Wait();
            //cts.Cancel();
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
