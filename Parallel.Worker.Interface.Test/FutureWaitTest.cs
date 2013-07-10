using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Parallel.Worker.Interface.Test
{
    [TestFixture]
    public class FutureWaitTest
    {
        private Future _future;
        private Action _wait;

        #region setup

        [SetUp]
        public void Setup()
        {
            _future = new Future();
            _wait = () => _future.Wait();
        }

        #endregion


        #region Tests

        [Test]
        public void WaitCompletes()
        {
            Task t = Task.Factory.StartNew(_wait);
            CompleteFuture();
            t.Wait();
            Assert.Pass();
        }

        [Test]
        public void MultipleWaitCompleted()
        {
            Task[] ts = new Task[]
                {
                    Task.Factory.StartNew(_wait),
                    Task.Factory.StartNew(_wait)
                };
            CompleteFuture();
            Task.WaitAll(ts);
            Assert.Pass();
        }

        #endregion

        private void CompleteFuture()
        {
            _future.SetExecuting();
            _future.SetCompleted();
        }
    }
}
