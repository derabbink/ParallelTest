using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface.Test.Instruction
{
    [TestFixture]
    public class WrapperTest
    {
        #region tests

        [Test]
        public void WrapNoArgs()
        {
            object arg = null;
            object expected = new object();
            Func<object> testable = () => expected;
            var wrapped = Wrapper.Wrap(testable);
            var actual = wrapped(arg);
            Assert.That(actual, Is.SameAs(expected));
        }

        [Test]
        public void WrapVoid()
        {
            object actual = null;
            object expected = new object();
            Action<object> testable = o => actual = o;
            var wrapped = Wrapper.Wrap(testable);
            wrapped(expected);
            Assert.That(actual, Is.SameAs(expected));
        }

        [Test]
        public void WrapNoArgsVoid()
        {
            object arg = null;
            object actual = null;
            object expected = new object();
            Action testable = () => actual = expected;
            var wrapped = Wrapper.Wrap(testable);
            wrapped(arg);
            Assert.That(actual, Is.SameAs(expected));
        }

        #endregion
    }
}
