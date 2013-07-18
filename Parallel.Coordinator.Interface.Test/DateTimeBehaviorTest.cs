using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Prallel.Coordinator.Interface.Test
{
    [TestFixture]
    public class DateTimeBehaviorTest
    {
        [Test]
        public void AddMethodIsPure()
        {
            DateTime expected = new DateTime(1970, 1, 1);
            DateTime callee = new DateTime(1970, 1, 1);
            callee.Add(new TimeSpan(1, 0, 0, 0));
            Assert.That(callee, Is.EqualTo(expected));
        }
    }
}
