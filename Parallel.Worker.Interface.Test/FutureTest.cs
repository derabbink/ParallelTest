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
    public class FutureTest
    {
        private ManualResetEventSlim _notifyOperation;
        private ManualResetEventSlim _holdOperation;
        private Action<CancellationToken, Action> _cancelableOperation;
        private Action<CancellationToken, Action> _cancelableWaitingOperation;
        
        #region setup
        [SetUp]
        public void Setup()
        {
            _notifyOperation = new ManualResetEventSlim(false);
            _holdOperation = new ManualResetEventSlim(false);

            _cancelableOperation = (ct, p) =>
                {
                    _notifyOperation.Set();
                    _holdOperation.Wait();
                    ct.ThrowIfCancellationRequested();
                };
            _cancelableWaitingOperation = (ct, p) =>
                {
                    _notifyOperation.Set();
                    _holdOperation.Wait(ct);
                };
        }
        #endregion

        #region tests
        [Test]
        public void CancelingWorks()
        {
            Future future = Future.Create(_cancelableOperation);
            future.Start();
            _notifyOperation.Wait();
            future.Cancel();
            _holdOperation.Set();
            try {
                future.Wait();
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.TypeOf<TaskCanceledException>());
            }

            Assert.That(future.IsCanceled, Is.True);
        }

        [Test]
        public void CancelingWaitWorks()
        {
            Future future = Future.Create(_cancelableWaitingOperation);
            future.Start();
            _notifyOperation.Wait();
            future.Cancel();
            _holdOperation.Set();
            try
            {
                future.Wait();
            }
            catch (AggregateException e)
            {
                Assert.That(e.InnerException, Is.TypeOf<TaskCanceledException>());
            }

            Assert.That(future.IsCanceled, Is.True);
        }
        #endregion
    }
}
