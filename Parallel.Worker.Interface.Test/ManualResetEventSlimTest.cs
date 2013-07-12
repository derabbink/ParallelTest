using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Parallel.Worker.Interface.Test
{
    [TestFixture]
    public class ManualResetEventSlimTest
    {
        private ManualResetEventSlim _mresNotify;
        private ManualResetEventSlim _mresHold;

        [SetUp]
        public void Setup()
        {
            _mresNotify = new ManualResetEventSlim(false);
            _mresHold = new ManualResetEventSlim(false);
        }

        [Test]
        [ExpectedException(typeof(OperationCanceledException))]
        public void CancellationAfterWaitBegins()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task t = Task.Factory.StartNew(() =>
                {
                    _mresNotify.Set();
                    Thread.Sleep(1000);
                    cts.Cancel();
                });
            _mresNotify.Wait();
            Assert.That(cts.Token.IsCancellationRequested, Is.False);
            _mresHold.Wait(cts.Token);
        }
    }
}
