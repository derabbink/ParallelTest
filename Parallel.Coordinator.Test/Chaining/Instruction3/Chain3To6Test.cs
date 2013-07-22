using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Parallel.Coordinator.Interface.Instruction;

namespace Parallel.Coordinator.Test.Chaining.Instruction3
{
    [TestFixture]
    public class Chain3To6Test : ChainingTest
    {
        #region tests

        [Test]
        public void Do2ChainedOperations()
        {
            var expected = _argument1;
            var coordinator = Coordinator3.Do(_1_1Instr, _1_1Instr, _1_1Instr, _argument1)
                .ThenDo(_3_3Instr, _3_3Instr, _3_3Instr, _3_3Instr, _3_3Instr, _3_3Instr);
            var actual = coordinator.Result;
            Assert.That(actual.IsDone, Is.True);
            var tuple = actual.Result.Item1;
            Assert.That(tuple.Item1, Is.SameAs(expected));
            Assert.That(tuple.Item2, Is.SameAs(expected));
            Assert.That(tuple.Item3, Is.SameAs(expected));
            Assert.That(actual.Result.Item2, Is.SameAs(tuple));
            Assert.That(actual.Result.Item3, Is.SameAs(tuple));
            Assert.That(actual.Result.Item4, Is.SameAs(tuple));
            Assert.That(actual.Result.Item5, Is.SameAs(tuple));
            Assert.That(actual.Result.Item6, Is.SameAs(tuple));
        }

        [Test]
        public void Do2ChainedOperationsError()
        {
            var expected = _argument1;
            var coordinator = Coordinator3.Do(_1_1Instr, _1_1Instr, _1_1Instr, _argument1);
            try
            {
                coordinator.ThenDo(_3_3Instr, _3_3Instr, _3_3Instr, _3_3Instr, _3_3Instr, _throw3_3Instr);
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
                Coordinator3.Do(cts.Token, _1_1Instr, _1_1Instr, _1_1Instr, _argument1).ThenDo(_3_3Instr, _3_3Instr, _3_3Instr, _3_3Instr, _3_3Instr, _blocking3_3Instr);
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
                Coordinator3.Do(cts.Token, _1_1Instr, _1_1Instr, _1_1Instr, _argument1).ThenDo(_3_3Instr, _3_3Instr, _3_3Instr, _3_3Instr, _3_3Instr, _timeoutable3_3Instr);
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
