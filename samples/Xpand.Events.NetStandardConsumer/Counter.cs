using Xpand.Events;

namespace Xpand.Events.NetStandardConsumer {
    public sealed class Counter {
        private readonly Signal<int> _changed = new Signal<int>();

        public Signal<int> Changed => _changed;

        public void Set(int value) {
            _changed.Publish(value);
        }
    }
}
