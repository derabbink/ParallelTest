using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Parallel.Worker.Interface;
using Parallel.Worker.Interface.Events;

namespace Parallel.Worker.Test
{
    [TestFixture]
    public class TaskExecutorProgressTest
    {
        private IExecutor _executor;
        private Func<CancellationToken, Action, object, object> _progressions;
        private ManualResetEventSlim _blockExecution;

        #region setup
        [SetUp]
        public void Setup()
        {
            _executor = new TaskExecutor();
            _blockExecution = new ManualResetEventSlim(false);
            _progressions = (ct, p, a) =>
                {
                    _blockExecution.Wait(ct);
                    int count = (int)a;
                    for (int i = 0; i < count; i++)
                        p();
                    return a;
                };
        }
        #endregion

        #region tests
        [Test]
        public void CorrectNumberOfProgressions([Values(0, 1, 2, 5, 13)] int count)
        {
            int actual = 0;
            Future<object> result = _executor.Execute(_progressions, count);
            EventHandler<ProgressEventArgs> handler = (sender, args) => actual++;
            result.SubscribeProgress(handler);
            _blockExecution.Set();
            
            result.Wait();
            Assert.That(actual, Is.EqualTo(count));

            //cleanup
            result.UnsubscribeProgress(handler);
        }
        #endregion
    }
}
