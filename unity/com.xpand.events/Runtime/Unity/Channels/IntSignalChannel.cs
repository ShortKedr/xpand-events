using UnityEngine;

namespace Xpand.Events.Unity.Channels {
    /// <summary>An integer-payload ScriptableObject signal channel.</summary>
    [CreateAssetMenu(menuName = "Xpand Events/Int Signal Channel", fileName = "IntSignalChannel")]
    public sealed class IntSignalChannel : SignalChannel<int> {
    }
}
