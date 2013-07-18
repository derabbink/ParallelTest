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
        private ManualResetEventSlim _notifyOperation;
        private ManualResetEventSlim _holdOperation;

        [SetUp]
        public void Setup()
        {
            _notifyOperation = new ManualResetEventSlim(false);
            _holdOperation = new ManualResetEventSlim(false);
        }

        [Test]
        [ExpectedException(typeof(OperationCanceledException))]
        public void CancellationAfterWaitBegins()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task t = Task.Factory.StartNew(() =>
                {
                    _notifyOperation.Set();
                    Thread.Sleep(1000);
                    cts.Cancel();
                });
            _notifyOperation.Wait();
            Assert.That(cts.Token.IsCancellationRequested, Is.False);
            //this will throw the expected exception
            _holdOperation.Wait(cts.Token);
        }
    }
}
