using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Parallel.Worker.Interface.Instruction;

namespace Parallel.Worker.Interface.Test.Instruction
{
    [TestFixture]
    public class SafeInstructionResultTest
    {
        #region tests

        [Test]
        public void SuccessfulFactory()
        {
            var result = SafeInstructionResult.Succeeded();
            Assert.That(result.State == SafeInstructionResult.ResultState.Succeeded);
        }

        [Test]
        public void SuccessfulGenericFactory()
        {
            object expectedValue = new object();
            var result = SafeInstructionResult<object>.Succeeded(expectedValue);
            Assert.That(result.State == SafeInstructionResult.ResultState.Succeeded);
            Assert.That(result.Value, Is.SameAs(expectedValue));
        }

        [Test]
        public void SuccessfulFailed()
        {
            var result = SafeInstructionResult.Failed();
            Assert.That(result.State == SafeInstructionResult.ResultState.Failed);
        }

        [Test]
        public void SuccessfulGenericFailed()
        {
            var expectedException = new Exception("Expected");
            var result = SafeInstructionResult<object>.Failed(expectedException);
            Assert.That(result.State == SafeInstructionResult.ResultState.Failed);
            Assert.That(result.Exception, Is.SameAs(expectedException));
        }

        #endregion
    }
}
