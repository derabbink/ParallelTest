using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parallel.Coordinator;
using Parallel.Coordinator.Interface;
using NUnit.Framework;
using Parallel.Coordinator.Interface.Instruction;
using Parallel.Worker;
using Parallel.Worker.Interface;

namespace Parallel.Coordinator.Test
{
    [TestFixture]
    public class Coordinator2Test
    {
        private object _argument;
        private Func<CancellationToken, Action, object, object> _identity;
        private IExecutor _executor;
        
        #region setup
        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _argument = new object();
            _identity = (ct, p, a) => a;
        }
        #endregion

        #region tests
        [Test]
        public void ParallelSuccessful()
        {
            var expected = _argument;
            var instruction1 = new CoordinatedInstruction<object, object>(_executor, _identity);
            var instruction2 = new CoordinatedInstruction<object, object>(_executor, _identity);
            var coord = Coordinator2.Do(instruction1, instruction2, _argument);
            var tuple = coord.Result.Result;

            Assert.That(tuple.Item1, Is.SameAs(expected));
            Assert.That(tuple.Item2, Is.SameAs(expected));
        }

        [Test]
        public void ParallelFailOne1()
        {
            var expected = new Exception("Expected");
            var notify1 = new ManualResetEventSlim(false);
            var notify2 = new ManualResetEventSlim(false);
            var hold = new ManualResetEventSlim(false);
            int timeout = -1; //infinite timeout
            var instruction1 = new CoordinatedInstruction<Exception, object>(_executor, CreateControllableThrow<object>(notify1, hold), timeout);
            var instruction2 = new CoordinatedInstruction<Exception, Exception>(_executor, CreateControllableIdentity<Exception>(notify2, hold), timeout);

            Task t = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        //this should throw
                        Coordinator2.Do(instruction1, instruction2, expected);
                    }
                    finally
                    {
                        //so the test thread won't block if cancellation happens too early
                        notify1.Set();
                        notify2.Set();
                    }
                });
            notify1.Wait();
            notify2.Wait();
            hold.Set();

            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds one layer of AggregateException
                Assert.That(e.InnerException.InnerException, Is.SameAs(expected));
            }
        }

        [Test]
        public void ParallelFailOne2()
        {
            var expected = new Exception("Expected");
            var notify1 = new ManualResetEventSlim(false);
            var notify2 = new ManualResetEventSlim(false);
            var hold = new ManualResetEventSlim(false);
            int timeout = -1; //infinite timeout
            var instruction1 = new CoordinatedInstruction<Exception, Exception>(_executor, CreateControllableIdentity<Exception>(notify2, hold), timeout);
            var instruction2 = new CoordinatedInstruction<Exception, object>(_executor, CreateControllableThrow<object>(notify1, hold), timeout);

            Task t = Task.Factory.StartNew(() =>
            {
                try
                {
                    //this should throw
                    Coordinator2.Do(instruction1, instruction2, expected);
                }
                finally
                {
                    //so the test thread won't block if cancellation happens too early
                    notify1.Set();
                    notify2.Set();
                }
            });
            notify1.Wait();
            notify2.Wait();
            hold.Set();

            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds one layer of AggregateException
                Assert.That(e.InnerException.InnerException, Is.SameAs(expected));
            }
        }

        [Test]
        public void ParallelFailTwo()
        {
            var expected = new Exception("Expected");
            var notify1 = new ManualResetEventSlim(false);
            var notify2 = new ManualResetEventSlim(false);
            var hold = new ManualResetEventSlim(false);
            int timeout = -1; //infinite timeout
            var instruction1 = new CoordinatedInstruction<Exception, object>(_executor, CreateControllableThrow<object>(notify2, hold), timeout);
            var instruction2 = new CoordinatedInstruction<Exception, object>(_executor, CreateControllableThrow<object>(notify1, hold), timeout);

            Task t = Task.Factory.StartNew(() =>
            {
                try
                {
                    //this should throw
                    Coordinator2.Do(instruction1, instruction2, expected);
                }
                finally
                {
                    //so the test thread won't block if cancellation happens too early
                    notify1.Set();
                    notify2.Set();
                }
            });
            notify1.Wait();
            notify2.Wait();
            hold.Set();

            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds one layer of AggregateException
                Assert.That(e.InnerException.InnerException, Is.SameAs(expected));
            }
        }

        [Test]
        public void ParallelTimeoutOne1()
        {
            var notify1 = new ManualResetEventSlim(false);
            var notify2 = new ManualResetEventSlim(false);
            var hold1 = new ManualResetEventSlim(false);
            var hold2 = new ManualResetEventSlim(false);
            int timeout2 = -1; //infinite timeout
            //this one will time out
            var instruction1 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentity<object>(notify1, hold1));
            //this will complete
            var instruction2 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentity<object>(notify2, hold2), timeout2);

            Task t = Task.Factory.StartNew(() =>
            {
                try
                {
                    //this should throw
                    Coordinator2.Do(instruction1, instruction2, _argument);
                }
                finally
                {
                    //so the test thread won't block if cancellation happens too early
                    notify1.Set();
                    notify2.Set();
                }
            });
            notify1.Wait();
            notify2.Wait();
            //hold1.Set();
            hold2.Set();

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
        public void ParallelTimeoutOne2()
        {
            var notify1 = new ManualResetEventSlim(false);
            var notify2 = new ManualResetEventSlim(false);
            var hold1 = new ManualResetEventSlim(false);
            var hold2 = new ManualResetEventSlim(false);
            int timeout1 = -1; //infinite timeout
            //this one will complete
            var instruction1 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentity<object>(notify1, hold1), timeout1);
            //this will time out
            var instruction2 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentity<object>(notify2, hold2));

            Task t = Task.Factory.StartNew(() =>
            {
                try
                {
                    //this should throw
                    Coordinator2.Do(instruction1, instruction2, _argument);
                }
                finally
                {
                    //so the test thread won't block if cancellation happens too early
                    notify1.Set();
                    notify2.Set();
                }
            });
            notify1.Wait();
            notify2.Wait();
            hold1.Set();
            //hold2.Set();

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
        public void ParallelTimeoutTwo()
        {
            var notify1 = new ManualResetEventSlim(false);
            var notify2 = new ManualResetEventSlim(false);
            var hold1 = new ManualResetEventSlim(false);
            var hold2 = new ManualResetEventSlim(false);
            var instruction1 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentity<object>(notify1, hold1));
            var instruction2 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentity<object>(notify2, hold2));

            Task t = Task.Factory.StartNew(() =>
            {
                try
                {
                    //this should throw
                    Coordinator2.Do(instruction1, instruction2, _argument);
                }
                finally
                {
                    //so the test thread won't block if cancellation happens too early
                    notify1.Set();
                    notify2.Set();
                }
            });
            notify1.Wait();
            notify2.Wait();
            //hold1.Set();
            //hold2.Set();

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
        public void ParallelCancelOne1()
        {
            object target1 = null;
            object target2 = null;
            CancellationTokenSource cts = new CancellationTokenSource();
            Action sideEffect1 = () => { target1 = _argument; };
            Action sideEffect2 = () => { target2 = _argument; };
            var notify1 = new ManualResetEventSlim(false);
            var notify2 = new ManualResetEventSlim(false);
            var hold1 = new ManualResetEventSlim(false);
            var hold2 = new ManualResetEventSlim(false);
            int timeout = -1; //infinite timeout
            var instruction1 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentityWithSideEffect<object>(notify1, hold1, sideEffect1), timeout);
            var instruction2 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentityWithSideEffect<object>(notify2, hold2, sideEffect2), timeout);

            Task t = Task.Factory.StartNew(() =>
            {
                try
                {
                    //this should throw
                    Coordinator2.Do(cts.Token, instruction1, instruction2, _argument);
                }
                finally
                {
                    //so the test thread won't block if cancellation happens too early
                    notify1.Set();
                    notify2.Set();
                }
            });
            notify1.Wait();
            notify1.Reset();
            notify2.Wait();
            hold1.Set();
            notify1.Wait();
            cts.Cancel();
            //hold2.Set();

            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds one layer of AggregateException
                Assert.That(e.InnerException.InnerException, Is.TypeOf<TaskCanceledException>());
            }
            Assert.That(target1, Is.SameAs(_argument));
            Assert.That(target2, Is.Null);
        }

        [Test]
        public void ParallelCancelOne2()
        {
            object target1 = null;
            object target2 = null;
            CancellationTokenSource cts = new CancellationTokenSource();
            Action sideEffect1 = () => { target1 = _argument; };
            Action sideEffect2 = () => { target2 = _argument; };
            var notify1 = new ManualResetEventSlim(false);
            var notify2 = new ManualResetEventSlim(false);
            var hold1 = new ManualResetEventSlim(false);
            var hold2 = new ManualResetEventSlim(false);
            int timeout = -1; //infinite timeout
            var instruction1 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentityWithSideEffect<object>(notify1, hold1, sideEffect1), timeout);
            var instruction2 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentityWithSideEffect<object>(notify2, hold2, sideEffect2), timeout);

            Task t = Task.Factory.StartNew(() =>
            {
                try
                {
                    //this should throw
                    Coordinator2.Do(cts.Token, instruction1, instruction2, _argument);
                }
                finally
                {
                    //so the test thread won't block if cancellation happens too early
                    notify1.Set();
                    notify2.Set();
                }
            });
            notify1.Wait();
            notify2.Wait();
            notify2.Reset();
            hold2.Set();
            notify2.Wait();
            cts.Cancel();
            //hold1.Set();

            try
            {
                t.Wait();
            }
            catch (AggregateException e)
            {
                //Task t adds one layer of AggregateException
                Assert.That(e.InnerException.InnerException, Is.TypeOf<TaskCanceledException>());
            }
            Assert.That(target1, Is.Null);
            Assert.That(target2, Is.SameAs(_argument));
        }

        [Test]
        public void ParallelCancelTwo()
        {
            object target = null;
            CancellationTokenSource cts = new CancellationTokenSource();
            Action sideEffect = () => { target = _argument; };
            var notify1 = new ManualResetEventSlim(false);
            var notify2 = new ManualResetEventSlim(false);
            var hold1 = new ManualResetEventSlim(false);
            var hold2 = new ManualResetEventSlim(false);
            int timeout = -1; //infinite timeout
            var instruction1 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentityWithSideEffect<object>(notify1, hold1, sideEffect), timeout);
            var instruction2 = new CoordinatedInstruction<object, object>(_executor, CreateControllableIdentityWithSideEffect<object>(notify2, hold2, sideEffect), timeout);

            Task t = Task.Factory.StartNew(() =>
            {
                try
                {
                    //this should throw
                    Coordinator2.Do(cts.Token, instruction1, instruction2, _argument);
                }
                finally
                {
                    //so the test thread won't block if cancellation happens too early
                    notify1.Set();
                    notify2.Set();
                }
            });
            notify1.Wait();
            notify2.Wait();
            cts.Cancel();
            //hold1.Set();
            //hold2.Set();

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

        #region helpers
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
