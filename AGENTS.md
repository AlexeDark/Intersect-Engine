# AGENTS.md

## Mission
Modernize Intersect Engine without destabilizing gameplay, editor workflows, or deployment scripts.

## Working Rules
- Prefer small, reviewable commits over large rewrites.
- Keep behavior-compatible changes first; do architecture shifts behind feature-safe boundaries.
- Do not introduce sync-over-async (`.Result`, `.Wait()`, `GetAwaiter().GetResult()`) in runtime paths.
- `async void` is allowed only for UI event handlers.
- Every background operation must have:
  - owner/lifecycle,
  - cancellation path,
  - error logging.
- Avoid fire-and-forget tasks unless explicitly supervised.

## Concurrency Standards
- Prefer `Task`, `CancellationToken`, and bounded queues to raw threads.
- Use `Channel<T>` for producer/consumer pipelines.
- Use `Task.Run` only for CPU-bound work or isolation boundaries.
- Propagate cancellation from request/session/application scope.

## Data Access Standards
- Prefer async database APIs for I/O.
- Keep context scope explicit and short-lived.
- Ensure lock/state cleanup happens in `finally`.
- Avoid hidden side effects in static helpers.

## Observability Standards
- Log with structured properties (no string-only exception logs).
- Add metrics around queue depth, latency, error rate, and retries for critical flows.
- Prefer deterministic failure over silent fallback when data integrity is at risk.

## Delivery Sequence
1. Fix correctness and reliability issues (deadlock risks, unobserved tasks, cancellation leaks).
2. Introduce architecture primitives (bounded queues, managed workers).
3. Raise quality gates (analyzers, CI checks, integration coverage).
4. Expand nullability and API contract strictness.

## Done Criteria For Each Change
- Compiles locally for affected projects.
- No new analyzer warnings in touched files.
- Tests updated or added when behavior changes.
- Change is documented if it introduces a new pattern.
