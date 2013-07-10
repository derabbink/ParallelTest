using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Test.Instruction
{
    [TestFixture]
    public class CallbackInstructionTest
    {
        private Action<object, Action<object>> _identity;

        #region tests

        [SetUp]
        public void Setup()
        {
            _identity = (arg, callback) => callback(arg);
        }

        [Test]
        public void CallbackToReturn()
        {
            object expected = new object();
            Func<object, object> wrapped = CallbackInstruction<object, object>.CallbackToReturn(_identity);
            object actual = wrapped(expected);
            Assert.That(actual, Is.SameAs(expected));
        }

        #endregion
    }
}
