using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Parallel.Coordinator.Interface.Instruction;

namespace Parallel.Coordinator.Test
{
    [TestFixture]
    public class Coordinator7Test : CoordinatorMultiTest
    {
        private const int ParallelCount = 7;

        #region tests
        [Test]
        public void ParallelSuccessful()
        {
            var expected = _argument;
            int timeout = -1; //infinite timeout
            var instructions = GenerateIdentityInstructions(ParallelCount, timeout);
            var coord = Coordinator7.Do(instructions[0], instructions[1], instructions[2], instructions[3], instructions[4], instructions[5], instructions[6], _argument);
            var tuple = coord.Result.Result;

            Assert.That(tuple.Item1, Is.SameAs(expected));
            Assert.That(tuple.Item2, Is.SameAs(expected));
            Assert.That(tuple.Item3, Is.SameAs(expected));
            Assert.That(tuple.Item4, Is.SameAs(expected));
            Assert.That(tuple.Item5, Is.SameAs(expected));
            Assert.That(tuple.Item6, Is.SameAs(expected));
            Assert.That(tuple.Item7, Is.SameAs(expected));
        }

        [Test]
        public void ParallelFailOne([Range(0, ParallelCount - 1)] int failIndex)
        {
            var notifyEvents = GenerateManualResetEvents(ParallelCount, false);
            var hold = new ManualResetEventSlim(false);
            var holdEvents = Enumerable.Repeat(hold, ParallelCount).ToArray();
            int timeout = -1; //infinite timeout
            var instructions = GenerateControllableIdentityInstructions(notifyEvents, holdEvents, timeout);
            var throwingInstruction = GenerateControllableThrowingInstruction(notifyEvents[failIndex], hold, timeout);
            instructions[failIndex] = throwingInstruction;

            Task t = RunParallelInstructions(instructions, notifyEvents, CancellationToken.None, _expectedException);
            WaitAll(notifyEvents);
            hold.Set();

            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds one layer of AggregateException
                Assert.That(e.InnerException.InnerException, Is.SameAs(_expectedException));
            }
        }

        [Test]
        public void ParallelFailAll()
        {
            var notifyEvents = GenerateManualResetEvents(ParallelCount, false);
            var hold = new ManualResetEventSlim(false);
            var holdEvents = Enumerable.Repeat(hold, ParallelCount).ToArray();
            int timeout = -1; //infinite timeout
            var instructions = GenerateControllableThrowingInstructions(notifyEvents, holdEvents, timeout);

            Task t = RunParallelInstructions(instructions, notifyEvents, CancellationToken.None, _expectedException);
            WaitAll(notifyEvents);
            hold.Set();

            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds one layer of AggregateException
                Assert.That(e.InnerException.InnerException, Is.SameAs(_expectedException));
            }
        }

        [Test]
        public void ParallelTimeoutOne([Range(0, ParallelCount - 1)] int timeoutIndex)
        {
            var notifyEvents = GenerateManualResetEvents(ParallelCount, false);
            var holdEvents = GenerateManualResetEvents(ParallelCount, false);
            int timeout = -1; //infinite timeout
            var instructions = GenerateControllableIdentityInstructions(notifyEvents, holdEvents, timeout);
            var timeoutInstruction = new CoordinatedInstruction<Exception, Exception>(_executor, CreateControllableIdentity<Exception>(notifyEvents[timeoutIndex], holdEvents[timeoutIndex]));
            instructions[timeoutIndex] = timeoutInstruction;

            Task t = RunParallelInstructions(instructions, notifyEvents, CancellationToken.None, _expectedException);
            WaitAll(notifyEvents);
            SetAllButOne(notifyEvents, timeoutIndex);

            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds one layer of AggregateException
                Assert.That(e.InnerException.InnerException, Is.TypeOf<TaskCanceledException>());
            }
        }

        [Test]
        public void ParallelTimeoutAll()
        {
            var notifyEvents = GenerateManualResetEvents(ParallelCount, false);
            var holdEvents = GenerateManualResetEvents(ParallelCount, false);
            int timeout = 1000;
            var instructions = GenerateControllableIdentityInstructions(notifyEvents, holdEvents, timeout);

            Task t = RunParallelInstructions(instructions, notifyEvents, CancellationToken.None, _expectedException);
            WaitAll(notifyEvents);
            //SetAll(notifyEvents);

            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds one layer of AggregateException
                Debug.WriteLine(e.InnerException.InnerException);
                Assert.That(e.InnerException.InnerException, Is.TypeOf<TaskCanceledException>());
                //This test can fail with an AggregateException(OperationCanceledException) due to a race condition:
                // the first timed-out subtask cancels all other subtasks, on of which had not yet been started
                // Solution: increase timeout
            }
        }

        [Test]
        public void ParallelCancelOne([Range(0, ParallelCount - 1)] int cancelIndex)
        {
            Exception[] targets = new Exception[ParallelCount];
            Action[] sideEffects = new Action[]
                {
                    () => { targets[0] = _argument; },
                    () => { targets[1] = _argument; },
                    () => { targets[2] = _argument; },
                    () => { targets[3] = _argument; },
                    () => { targets[4] = _argument; },
                    () => { targets[5] = _argument; },
                    () => { targets[6] = _argument; }
                };
            CancellationTokenSource cts = new CancellationTokenSource();
            var notifyEvents = GenerateManualResetEvents(ParallelCount, false);
            var holdEvents = GenerateManualResetEvents(ParallelCount, false);
            int timeout = -1; //infinite timeout
            var instructions = GenerateControllableIdentityInstructionsWithSideEffect(notifyEvents, holdEvents, sideEffects, timeout);

            Task t = RunParallelInstructions(instructions, notifyEvents, cts.Token, _argument);
            WaitAllButOne(notifyEvents, cancelIndex);
            ResetAllButOne(notifyEvents, cancelIndex);

            notifyEvents[cancelIndex].Wait();

            SetAllButOne(holdEvents, cancelIndex);
            WaitAllButOne(notifyEvents, cancelIndex);

            cts.Cancel();
            //holdEvents[cancelIndex].Set();

            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds one layer of AggregateException
                Assert.That(e.InnerException.InnerException, Is.TypeOf<TaskCanceledException>());
            }

            for (int i = 0; i < ParallelCount; i++)
                if (i == cancelIndex)
                    Assert.That(targets[i], Is.Null);
                else
                    Assert.That(targets[i], Is.SameAs(_argument));
        }

        [Test]
        public void ParallelCancelAll()
        {
            Exception target = null;
            Action[] sideEffects = new Action[]
                {
                    () => { target = _argument; },
                    () => { target = _argument; },
                    () => { target = _argument; },
                    () => { target = _argument; },
                    () => { target = _argument; },
                    () => { target = _argument; },
                    () => { target = _argument; }
                };
            CancellationTokenSource cts = new CancellationTokenSource();
            var notifyEvents = GenerateManualResetEvents(ParallelCount, false);
            var holdEvents = GenerateManualResetEvents(ParallelCount, false);
            int timeout = -1; //infinite timeout
            var instructions = GenerateControllableIdentityInstructionsWithSideEffect(notifyEvents, holdEvents, sideEffects, timeout);

            Task t = RunParallelInstructions(instructions, notifyEvents, cts.Token, _argument);
            WaitAll(notifyEvents);
            cts.Cancel();
            //SetAll(holdEvents);

            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds one layer of AggregateException
                Assert.That(e.InnerException.InnerException, Is.TypeOf<TaskCanceledException>());
            }
            Assert.That(target, Is.Null);
        }
        #endregion

        protected override Task RunParallelInstructions(CoordinatedInstruction<Exception, Exception>[] instructions,
                                                        ManualResetEventSlim[] notifyEvents, CancellationToken cancellationToken, Exception argument)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                try
                {
                    //this should throw
                    Coordinator7.Do(cancellationToken, instructions[0], instructions[1], instructions[2], instructions[3], instructions[4], instructions[5], instructions[6], argument);
                }
                finally
                {
                    //so the test thread won't block if cancellation happens too early
                    SetAll(notifyEvents);
                }
            });
            return t;
        }
    }
}
