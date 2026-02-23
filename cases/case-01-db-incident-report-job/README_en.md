# Case 01 — Database Incident & Periodic Processing Service

## THE SERVICE WAS SLIGHTLY UPDATED TO RUN ON .NET 8, AND THE ISSUE WAS SUCCESSFULLY REPRODUCED
## LINE REFERENCES ARE BASED ON THE CURRENT VERSION OF THE CODE IN THIS REPOSITORY

## 1. Problem Context
This incident happened because a background service was running nonstop — no delay, no throttling — constantly querying the database and generating files.

In practice, this ended up overloading the database, increasing CPU and disk usage, and reprocessing the same data over and over again.

---

## 2. Root Cause Analysis
The core issue was an infinite loop at line 17 in `Program.cs`, calling `engine.Execute()` without any pause between executions.

That means the system was running at full speed all the time, eventually leading to application timeouts due to connection pool exhaustion.

On top of that, the database objects  
**SqlConnection at line 31**  
**SqlCommand at line 32**  
**ExecuteReader at line 34**  
were not properly disposed. Over time, connections piled up and the database health degraded.

Another important issue: the query always fetches data from the “last 5 minutes” but doesn’t track how far it has already processed.  
So the same data keeps getting processed repeatedly.

Also, at line 40, each execution creates a new file using a `Guid`. This quickly leads to a large number of files and unnecessary disk pressure.

Finally, using `new` directly at line 16 tightly couples the code.  
That might be fine for simple objects, but for services, dependency injection would make the system more flexible and easier to evolve.

---

## 3. Immediate Mitigation Strategy
To stabilize production quickly:

1. Add a delay between execution cycles to reduce database load.  
2. Properly dispose connection, command, and reader on every run.  
3. Implement a checkpoint (store the last processed record).  
4. Limit how many records are processed per cycle to avoid spikes.  
5. Temporarily slow down the service and clean up old files.

These steps don’t solve everything long term, but they reduce immediate risk and keep the system running safely.

---

## 4. Structural Improvements
**Observability:** track execution time, errors, processed volume, and file growth to catch issues early.

**Resilience:** apply timeouts, retry with backoff, and protection against repeated failures to prevent cascading problems.

**Idempotency:** make sure that if the same event is processed twice, the outcome is still consistent and doesn’t create duplicates.

**Configuration & security:** move settings to external configuration and secure credentials properly. Quick wins with low risk.

---

## 5. Architectural Redesign Proposal
Instead of continuously polling the database, move to an event-driven approach.

Rather than the service constantly checking for new data, events should be pushed to a queue.  
A consumer reads from the queue, processes the message, and acknowledges completion.

This makes horizontal scaling easier and gives you proper progress control through real offsets or checkpoints.

**Trade-off:** this is more robust, but it also introduces more moving parts (queue infrastructure, monitoring, failure handling).

---

## 6. Trade-offs and Operational Considerations
The quick fix is simpler and cheaper, but it won’t scale well in the medium term.

An event-driven architecture handles growth and failures much better, but it comes with higher operational complexity and cost.

To avoid overengineering, the best approach is to evolve in phases:

1. Stabilize now with throttling and checkpoint control.  
2. Improve observability and resilience.  
3. Move to an event-driven model when scale and business criticality justify it.

## 7. Improvements Implemented in SafeExecutionEngine
- Execution was decoupled from the infinite loop in `Program.cs`, introducing controlled cadence through `Throttle()` and configurable polling interval (`HOTFIX_POLL_INTERVAL_MS`).
- Database resources now use `using` blocks for `SqlConnection`, `SqlCommand`, and `SqlDataReader`, preventing connection leaks and pool exhaustion.
- SQL access was rewritten with parameters (`@BatchSize`, `@Checkpoint`, `@CycleStart`) instead of dynamic query composition, improving safety and deterministic behavior.
- Processing became incremental through persisted checkpoint state (`LoadCheckpoint`/`SaveCheckpoint`), replacing repeated “last 5 minutes” reprocessing.
- Batch limiting (`HOTFIX_BATCH_SIZE`) constrains per-cycle load and avoids throughput spikes that previously amplified DB pressure.
- Concurrency is explicitly serialized with `lock (_sync)`, preventing overlapping runs and checkpoint race conditions.
- Output handling moved from per-run random files to daily append plus retention rotation (`RotateOutputFiles`), controlling disk growth.
- Operational settings were externalized and bounded through environment parsing (`ReadString`/`ReadInt`), improving runtime control.
- Current resilience scope is a hotfix baseline: it adds timeout-related settings but does not yet include cancellation-aware async flow, retry/backoff, or circuit breaker patterns.
