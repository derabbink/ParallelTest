using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Parallel.Worker.Interface.Test
{
    [TestFixture]
    public class EventHandlerSelfUnsubscribeTest
    {
        private event EventHandler TestEvent;

        private void OnTestEvent()
        {
            if (TestEvent != null)
                TestEvent(this, new EventArgs());
        }

        private void SubscribeTestEvent(EventHandler handler)
        {
            TestEvent += handler;
        }

        private void UnsubscribeTestEvent(EventHandler handler)
        {
            TestEvent -= handler;
        }

        [Test]
        public void NoEventHandlersLeftAfterwards()
        {
            EventHandler handler = null;
            object o = null;
            handler = (sender, args) =>
                {
                    o = new object();
                    UnsubscribeTestEvent(handler);
                };
            SubscribeTestEvent(handler);
            
            OnTestEvent();
            Assert.That(o, Is.Not.Null);
            object oldO = o;
            o = null;
            OnTestEvent();
            Assert.That(o, Is.Not.SameAs(oldO));
            Assert.That(o, Is.Null);
        }
    }
}
