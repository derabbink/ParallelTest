using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface.Test.Instruction
{
    [TestFixture]
    public class SafeInstructionTest
    {
        #region tests

        [Test]
        public void ValidConstruction()
        {
            new SafeInstruction<object, object>(Wrapper.Wrap(() => {}), null);
            Assert.Pass();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvalidConstruction()
        {
            new SafeInstruction<object, object>(null, null);
        }

        [Test]
        public void ExecuteSuccess()
        {
            var expectedValue = new object();
            Func<object, object> id = a => a;
            SafeInstructionResult<object> result = new SafeInstruction<object, object>(id, expectedValue).Invoke();
            Assert.That(result.State, Is.EqualTo(SafeInstructionResult.ResultState.Succeeded));
            Assert.That(result.Value, Is.SameAs(expectedValue));
        }

        [Test]
        public void ExecuteFailure()
        {
            var expectedException = new Exception("Expected");
            Func<object, object> fail = Wrapper.Wrap(() => { throw expectedException; });
            SafeInstructionResult<object> result = new SafeInstruction<object, object>(fail, null).Invoke();
            Assert.That(result.State, Is.EqualTo(SafeInstructionResult.ResultState.Failed));
            Assert.That(result.Exception, Is.SameAs(expectedException));
        }

        #endregion
    }
}
