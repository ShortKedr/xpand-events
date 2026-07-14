using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Xpand.Events.Unity.Channels;

namespace Xpand.Events.Unity.Tests {
    public sealed class SignalChannelTests {
        [Test]
        public void ChannelUsesCorePriorityAndPayloadContract() {
            var channel = ScriptableObject.CreateInstance<IntSignalChannel>();
            var observed = new List<int>();
            using (channel.Subscribe(value => observed.Add(value), priority: int.MinValue))
            using (channel.Subscribe(value => observed.Add(value * 10), priority: int.MaxValue)) {
                channel.Raise(2);
            }

            Assert.That(observed, Is.EqualTo(new[] { 20, 2 }));
            Object.DestroyImmediate(channel);
        }
    }
}
