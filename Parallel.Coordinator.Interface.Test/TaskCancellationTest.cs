using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Prallel.Coordinator.Interface.Test
{
    [TestFixture]
    public class TaskCancellationTest
    {
        [Test]
        public void TestIsCanceledPreStart()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var token = cts.Token;
            cts.Cancel();
            Task task = Task.Factory.StartNew(token.ThrowIfCancellationRequested, token);
            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.TypeOf<TaskCanceledException>());
            }

            Assert.That(task.IsCanceled, Is.True);
        }

        [Test]
        public void TestIsCanceledDifferentTokens()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var token1 = cts.Token;
            var token2 = cts.Token;
            Assert.That(token1, Is.Not.SameAs(token2));
            cts.Cancel();
            Task task = Task.Factory.StartNew(token1.ThrowIfCancellationRequested, token2);
            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.TypeOf<TaskCanceledException>());
            }

            Assert.That(task.IsCanceled, Is.True);
        }

        [Test]
        public void TestIsCancelledPostStart()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            ManualResetEventSlim notifyOperation = new ManualResetEventSlim(false);
            ManualResetEventSlim holdOperation = new ManualResetEventSlim(false);
            var token = cts.Token;
            Task task = Task.Factory.StartNew(() =>
                {
                    notifyOperation.Set();
                    holdOperation.Wait();
                    token.ThrowIfCancellationRequested();
                }, token);
            notifyOperation.Wait();
            cts.Cancel();
            holdOperation.Set();
            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.TypeOf<TaskCanceledException>());
            }

            Assert.That(task.IsCanceled, Is.True);
        }

        [Test]
        public void TestIsCancelledWait()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            ManualResetEventSlim notifyOperation = new ManualResetEventSlim(false);
            ManualResetEventSlim holdOperation = new ManualResetEventSlim(false);
            var token = cts.Token;
            Task task = Task.Factory.StartNew(() =>
            {
                notifyOperation.Set();
                holdOperation.Wait(token);
            }, token);
            notifyOperation.Wait();
            cts.Cancel();
            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.TypeOf<TaskCanceledException>());
            }

            Assert.That(task.IsCanceled, Is.True);
        }
    }
}
