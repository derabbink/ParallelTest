using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Parallel.Coordinator.Interface.Instruction;
using Parallel.Worker;
using Parallel.Worker.Interface;

namespace Parallel.Coordinator.Test
{
    public class CoordinatorMultiTest
    {
        protected Exception _argument;
        protected Exception _expectedException;
        protected Func<CancellationToken, Action, Exception, Exception> _identityEx;
        protected Func<CancellationToken, Action, Exception, Exception> _throw;
        protected IExecutor _executor;

        #region setup
        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _argument = new Exception("Pass through");
            _expectedException = new Exception("Expected");
            _identityEx = (ct, p, a) => a;
            _throw = (ct, p, e) => { throw e; };
        }
        #endregion

        #region helper functions
        protected ManualResetEventSlim[] GenerateManualResetEvents(int count, bool initValue)
        {
            ManualResetEventSlim[] result = new ManualResetEventSlim[count];
            for (int i=0; i<count; i++)
                result[i] = new ManualResetEventSlim(initValue);
            return result;
        }

        protected CoordinatedInstruction<Exception, Exception>[] GenerateIdentityInstructions(int count, int timeout)
        {
            CoordinatedInstruction<Exception, Exception>[] result = new CoordinatedInstruction<Exception, Exception>[count];
            for (int i = 0; i < count; i++)
                result[i] = new CoordinatedInstruction<Exception, Exception>(_executor, _identityEx, timeout);
            return result;
        }

        protected CoordinatedInstruction<Exception, Exception>[] GenerateControllableIdentityInstructions(ManualResetEventSlim[] notifyEvents, ManualResetEventSlim[] holdEvents, int timeout)
        {
            return GenerateControllableIdentityInstructionsWithSideEffect(notifyEvents, holdEvents, Enumerable.Repeat<Action>(()=>{}, notifyEvents.Length).ToArray(), timeout);
        }

        protected CoordinatedInstruction<Exception, Exception>[] GenerateControllableIdentityInstructionsWithSideEffect(ManualResetEventSlim[] notifyEvents, ManualResetEventSlim[] holdEvents, Action[] sideEffects, int timeout)
        {
            var count = notifyEvents.Length;
            CoordinatedInstruction<Exception, Exception>[] result = new CoordinatedInstruction<Exception, Exception>[count];
            for (int i = 0; i < count; i++)
                result[i] = new CoordinatedInstruction<Exception, Exception>(_executor, CreateControllableIdentityWithSideEffect<Exception>(notifyEvents[i], holdEvents[i], sideEffects[i]), timeout);
            return result;
        }

        protected CoordinatedInstruction<Exception, Exception> GenerateControllableThrowingInstruction(ManualResetEventSlim notify, ManualResetEventSlim hold, int timeout)
        {
            return new CoordinatedInstruction<Exception, Exception>(_executor, CreateControllableThrow<Exception>(notify, hold), timeout);
        }

        protected CoordinatedInstruction<Exception, Exception>[] GenerateControllableThrowingInstructions(ManualResetEventSlim[] notifyEvents, ManualResetEventSlim[] holdEvents, int timeout)
        {
            var count = notifyEvents.Length;
            CoordinatedInstruction<Exception, Exception>[] result = new CoordinatedInstruction<Exception, Exception>[count];
            for (int i = 0; i < count; i++)
                result[i] = GenerateControllableThrowingInstruction(notifyEvents[i], holdEvents[i], timeout);
            return result;
        }

        protected virtual Task RunParallelInstructions(CoordinatedInstruction<Exception, Exception>[] instructions, ManualResetEventSlim[] notifyEvents, CancellationToken cancellationToken, Exception argument)
        {
            //can't make this class abstract, since NUnit cannot instantiate it then.
            throw new NotImplementedException();
        }
        
        public static Func<CancellationToken, Action, T, T> CreateControllableIdentity<T>
            (ManualResetEventSlim notify, ManualResetEventSlim hold)
        {
            return (ct, p, a) =>
            {
                notify.Set();
                p();
                hold.Wait(ct);
                return a;
            };
        }

        public static Func<CancellationToken, Action, T, T> CreateControllableIdentityWithSideEffect<T>
            (ManualResetEventSlim notify, ManualResetEventSlim hold, Action sideEffect)
        {
            return (ct, p, a) =>
            {
                notify.Set();
                p();
                hold.Wait(ct);
                notify.Set();
                sideEffect();
                return a;
            };
        }

        public static Func<CancellationToken, Action, Exception, T> CreateControllableThrow<T>
            (ManualResetEventSlim notify, ManualResetEventSlim hold)
        {
            return (ct, p, e) =>
            {
                notify.Set();
                p();
                hold.Wait(ct);
                throw e;
            };
        }

        public void WaitAll(ManualResetEventSlim[] waitEvents)
        {
            foreach (ManualResetEventSlim mres in waitEvents)
                mres.Wait();
        }

        public void WaitAllButOne(ManualResetEventSlim[] waitEvents, int indexToSkip)
        {
            int count = waitEvents.Length;
            for (int i = 0; i < count; i++)
                if (i != indexToSkip)
                    waitEvents[i].Wait();
        }

        public void SetAll(ManualResetEventSlim[] waitEvents)
        {
            foreach (ManualResetEventSlim mres in waitEvents)
                mres.Set();
        }

        public void SetAllButOne(ManualResetEventSlim[] waitEvents, int indexToSkip)
        {
            int count = waitEvents.Length;
            for (int i = 0; i < count; i++)
                if (i != indexToSkip)
                    waitEvents[i].Set();
        }

        public void ResetAll(ManualResetEventSlim[] waitEvents)
        {
            foreach (ManualResetEventSlim mres in waitEvents)
                mres.Reset();
        }

        public void ResetAllButOne(ManualResetEventSlim[] waitEvents, int indexToSkip)
        {
            int count = waitEvents.Length;
            for (int i = 0; i < count; i++)
                if (i != indexToSkip)
                    waitEvents[i].Reset();
        }

        #region testing helpers
        [Test]
        public void CreateControllableIdentityWorks()
        {
            var expected = _argument;
            ManualResetEventSlim notify = new ManualResetEventSlim(false);
            ManualResetEventSlim hold = new ManualResetEventSlim(true);
            Func<CancellationToken, Action, object, object> funcUnderTest = CreateControllableIdentity<object>(notify, hold);
            CancellationTokenSource cts = new CancellationTokenSource();
            Action progress = () => { };

            var actual = funcUnderTest(cts.Token, progress, _argument);
            Assert.That(actual, Is.SameAs(expected));
        }

        [Test]
        public void CreateControllableIdentityWithSideEffectWorks()
        {
            var expected = _argument;
            object target = null;
            Action sideEffect = () => { target = _argument; };
            ManualResetEventSlim notify = new ManualResetEventSlim(false);
            ManualResetEventSlim hold = new ManualResetEventSlim(true);
            Func<CancellationToken, Action, object, object> funcUnderTest = CreateControllableIdentityWithSideEffect<object>(notify, hold, sideEffect);
            CancellationTokenSource cts = new CancellationTokenSource();
            Action progress = () => { };

            var actual = funcUnderTest(cts.Token, progress, _argument);
            Assert.That(target, Is.SameAs(expected));
            Assert.That(actual, Is.SameAs(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void CreateControllableThrowWorks()
        {
            var argument = new Exception("Expected");
            ManualResetEventSlim notify = new ManualResetEventSlim(false);
            ManualResetEventSlim hold = new ManualResetEventSlim(true);
            Func<CancellationToken, Action, Exception, object> funcUnderTest = CreateControllableThrow<object>(notify, hold);
            CancellationTokenSource cts = new CancellationTokenSource();
            Action progress = () => { };

            var actual = funcUnderTest(cts.Token, progress, argument);
        }
        #endregion
        #endregion
    }
}
