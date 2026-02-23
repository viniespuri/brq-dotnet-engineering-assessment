# Case 01 - Database Incident & Periodic Processing Service

## THE SERVICE WAS SLIGHTLY CHANGED TO RUN ON DOTNET 8, BUT THE PROBLEM SIMULATION WAS SUCCESSFULLY REPRODUCED
## LINE REFERENCES ARE BASED ON THE UPDATED CODE IN THIS REPOSITORY

## 1. Problem Context
This incident happened because a service was running continuously, without pause, querying the database and generating files non-stop.  
In practice, this not only overloaded the database but also increased CPU and disk usage and generated repeated processing results.

## 2. Root Cause Analysis
The main issue was the infinite loop at line 17 of `Program.cs`, which calls `engine.Execute()` without intervals.  
As a result, the system kept trying to process at maximum speed all the time, leading to application timeout due to connection pool exhaustion.

In addition, the database access objects  
**SqlConnection at line 31**  
**SqlCommand at line 32**  
**ExecuteReader at line 34**  
were not being released correctly, which accumulates connections and degrades database health over time.

Another critical point: the query always fetches the "last 5 minutes" without storing how far processing has already advanced.  
In practice, this causes the same data to be processed repeatedly.

Another point, at line 40, is that each execution creates a new file with `Guid`, which quickly generates many files and puts pressure on disk usage.

Using `new` as done at line 16 keeps the code coupled and hard to evolve.  
For simple objects this is fine. For services, a good practice would be dependency injection.

## 3. Immediate Mitigation Strategy
To quickly stabilize production:

1. Control service frequency (add pause between cycles) to reduce database load.  
2. Ensure proper release of connection/command/reader in each execution.  
3. Save a checkpoint (last processed item) to avoid data replay.  
4. Limit how many records are processed per cycle to prevent spikes.  
5. Apply a temporary operational adjustment (reduce service pace and clean old files).  

These actions do not solve everything, but they reduce immediate risk and keep the system running.

## 4. Structural Improvements
**Observability:** tracking metrics such as execution time, errors, processed volume, and file growth helps detect problems early.

**Resilience:** applying timeout, retries with interval, and protection against sequential failures prevents cascade effects when something fails.

**Idempotency:** ensuring that if the same event arrives twice, the final outcome remains unique, avoiding duplicate output.

**Configuration and security:** move parameters to external configuration and protect credentials. These are quick adjustments with lower leakage risk.

## 5. Architectural Redesign Proposal
The proposal is to replace the "keep polling the database" model with an event-driven model.

Instead of querying the database all the time, events are sent to a queue.  
A consumer reads the queue, processes, and acknowledges when done.  
This makes horizontal scaling easier and allows progress control with real offset/checkpoint.

**Trade-off:** this approach is more robust, but it also introduces more components to operate (queue, monitoring, failure handling).

## 6. Trade-offs and Operational Considerations
The quick fix is cheaper and simpler, but has scaling limits in the medium term.

Event-driven architecture handles growth and failures better, but costs more to maintain and requires stronger operational discipline.

To avoid overengineering, the best path is phased evolution:
1. Stabilize now with cycle control + checkpoint.  
2. Strengthen observability and failure protection.  
3. Migrate to events when volume and criticality justify it.

## 7. Improvements Implemented in SafeExecutionEngine
- Execution was decoupled from the infinite loop in `Program.cs`, adopting a controlled cycle via `Throttle()` and configurable interval (`HOTFIX_POLL_INTERVAL_MS`).
- Database access now uses `using` for `SqlConnection`, `SqlCommand`, and `SqlDataReader`, removing connection leaks and reducing connection pool exhaustion risk.
- Query was parameterized with `@BatchSize`, `@Checkpoint`, and `@CycleStart`, removing dynamic SQL and improving execution predictability.
- Processing is now incremental via checkpoint persisted to file (`LoadCheckpoint`/`SaveCheckpoint`), avoiding continuous re-read of the same time window.
- Batch size limit (`HOTFIX_BATCH_SIZE`) reduces I/O and memory spikes per cycle, improving stability under load.
- Concurrency control with `lock (_sync)` was applied, preventing simultaneous execution of the same engine and checkpoint update races.
- File generation changed from "one file per execution" to daily append with rotation (`RotateOutputFiles`), reducing uncontrolled disk growth.
- Critical settings were externalized through environment variables with validation and clamping (`ReadInt`), increasing operational safety.
- As a current limitation, the hotfix improves immediate robustness but still does not implement `CancellationToken`, retry with backoff, and circuit breaker.
