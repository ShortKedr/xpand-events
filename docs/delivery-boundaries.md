# Delivery boundaries

Xpand Events is an in-process notification primitive. A publish invokes the
current stable snapshot of handlers in the same process. Once `Publish` or
`PublishAsync` returns successfully, the library keeps no durable record of the
payload.

## What the library guarantees

- deterministic subscription ordering and snapshot behavior;
- thread-safe subscription, unsubscription, and publishing in Core;
- explicit synchronous or sequential asynchronous exception behavior;
- process-local ownership through disposable subscription tokens;
- optional process-local buffering and backpressure through explicit Channel
  adapters.

It does **not** guarantee delivery after a crash, process restart, deployment,
network partition, machine loss, or subscriber outage. It provides no durable
acknowledgements, retries, dead-letter storage, deduplication, consumer offsets,
transactions, or cross-process transport.

## Choose the primitive by failure boundary

| Requirement | Appropriate primitive |
|---|---|
| Notify components synchronously inside one process | `Signal<T>` |
| Await handlers sequentially inside one process | `AsyncSignal<T>` |
| Buffer work or apply backpressure inside one process | `System.Threading.Channels` |
| Deliver across processes or machines | A message broker or transport protocol |
| Survive process or machine failure | Durable broker/queue or durable event store |
| Atomically publish after a database change | Transactional outbox plus a durable transport |
| Replay history or rebuild state | Event store or application-owned durable log |

Kafka, RabbitMQ, Azure Service Bus, Amazon SQS/SNS, NATS JetStream, database
outboxes, and similar systems solve different failure boundaries. Select one
from the application's durability, ordering, throughput, operational, and
compliance requirements; Xpand Events does not wrap those tradeoffs into Core.

## Safe integration pattern

A process may consume a durable message and then publish a local signal to notify
its in-process components. The durable transport owns acknowledgement and retry;
the local signal owns only the current in-process dispatch. A handler may also
write to an outbox, but acknowledging the source before that durable write is
committed creates a message-loss window.

Do not treat the `IObservable<T>` or Channel adapters as a network or persistence
layer. They preserve only the semantics their documented process-local primitive
can represent.
