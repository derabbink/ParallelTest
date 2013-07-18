using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface.Test.Instruction
{
    [TestFixture]
    public class WrapperTest
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Action _progress ;

            #region setup

        [SetUp]
        public void Setup()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _progress = () => { };
        }

        #endregion

        #region tests

        [Test]
        public void WrapNoArgs()
        {
            object arg = null;
            object expected = new object();
            Func<CancellationToken, Action, object> testable = (_, p) => expected;
            var wrapped = Wrapper.Wrap(testable);
            var actual = wrapped(_cancellationTokenSource.Token, _progress, arg);
            Assert.That(actual, Is.SameAs(expected));
        }

        [Test]
        public void WrapVoid()
        {
            object actual = null;
            object expected = new object();
            Action<CancellationToken, Action, object> testable = (_, p, o) => actual = o;
            var wrapped = Wrapper.Wrap(testable);
            wrapped(_cancellationTokenSource.Token, _progress, expected);
            Assert.That(actual, Is.SameAs(expected));
        }

        [Test]
        public void WrapNoArgsVoid()
        {
            object arg = null;
            object actual = null;
            object expected = new object();
            Action<CancellationToken, Action> testable = (_, p) => actual = expected;
            var wrapped = Wrapper.Wrap(testable);
            wrapped(_cancellationTokenSource.Token, _progress, arg);
            Assert.That(actual, Is.SameAs(expected));
        }

        #endregion
    }
}
