using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Xpand.Events.Unity.Tests {
    public sealed class UnityMainThreadDispatcherPlayModeTests {
        [UnityTest]
        public IEnumerator BackgroundPostRunsFromUpdateOnMainThread() {
            var gameObject = new GameObject("signal-dispatcher");
            var dispatcher = gameObject.AddComponent<UnityMainThreadDispatcher>();
            var ranOnMainThread = false;

            Task post = Task.Run(() => dispatcher.Post(() => ranOnMainThread = dispatcher.IsMainThread));
            while (!post.IsCompleted) yield return null;
            if (post.IsFaulted) throw post.Exception;

            yield return null;
            Assert.That(ranOnMainThread, Is.True);

            Object.Destroy(gameObject);
        }
    }
}
