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
    public class GenericExecutorProgressTest
    {
        private IExecutor<object, object> _executor;
        private Func<CancellationToken, Action, object, object> _progressions;

        #region setup
        [SetUp]
        public void Setup()
        {
            _executor = new Executor<object, object>();
            _progressions = (ct, p, a) =>
            {
                int count = (int)a;
                for (int i = 0; i < count; i++)
                    p();
                return a;
            };
        }
        #endregion

        #region tests
        /// <summary>
        /// Since the resulting future IsDone, we must expect 0 progress updates
        /// </summary>
        /// <param name="count"></param>
        [Test]
        public void CorrectNumberOfProgressions([Values(0, 1, 2, 5, 13)] int count)
        {
            int actual = 0;
            int expected = 0;
            Future<object> result = _executor.Execute(_progressions, count);
            EventHandler<ProgressEventArgs> handler = (sender, args) => actual++;
            result.SubscribeProgress(handler);

            result.Wait();
            Assert.That(actual, Is.EqualTo(expected));

            //cleanup
            result.UnsubscribeProgress(handler);
        }
        #endregion
    }
}
