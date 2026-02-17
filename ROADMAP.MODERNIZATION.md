# Intersect Engine Modernization Roadmap

## Goal
Bring runtime quality to a "production-grade modern .NET" baseline:
- predictable async behavior (no sync-over-async deadlocks),
- controlled concurrency,
- measurable performance and reliability,
- safer code evolution with analyzers and tests.

## Phase 1: Async Safety and Reliability (Completed 2026-02-17)
- Remove `async void` in non-UI flows.
- Remove `.Result`, `.Wait()`, and `GetAwaiter().GetResult()` from server/web/data hot paths.
- Replace fire-and-forget tasks with tracked background execution.
- Ensure cancellation token propagation for I/O operations.
- Add timeouts for outbound network calls.

Progress (updated 2026-02-17):
- [x] Removed sync-over-async from key server hot paths (`FullServerContext`, `ApiService`, `DbInterface`, embedded resource unpacking).
- [x] Reduced fire-and-forget risk in hot paths (`LogAndDisconnect` callers, logic token cleanup, net debug command path).
- [x] Added outbound timeout for network diagnostics upload (`HasteBinClient` request timeout).
- [x] Removed shutdown lifecycle `Task.Wait()` usage in application context (`RequestShutdown` now thread-join based).
- [x] Removed unsupervised exception-dispatch task scheduling (`ServerContext.DispatchUnhandledException` now direct dispatch).
- [x] Replaced bootstrap `ContinueWith` chain with explicit `async/await` lifecycle flow (`BootstrapperService.StartAsync`).
- [x] Audit result: no `GetAwaiter().GetResult()`, `Task.Wait()`, or `Task.Result` remaining in server/core/framework runtime paths.
- [x] Audit result: no non-event-handler `async void` in server/core/framework runtime projects.
- [x] Validation: `dotnet test Intersect.Tests.Server/Intersect.Tests.Server.csproj -c Debug` passed (44/44).

Exit Criteria:
- No sync-over-async in runtime paths.
- No non-event-handler `async void`.
- Critical flows pass tests and smoke run.

## Phase 2: Concurrency Architecture
- Replace ad-hoc threading with bounded queues (`Channel<T>`) where applicable.
- Introduce explicit worker ownership (`BackgroundService` or equivalent lifecycle-managed services).
- Separate CPU-bound and I/O-bound workloads.
- Standardize retry/backoff and circuit-breaker behavior for external calls.

Progress (updated 2026-02-17):
- [x] Introduced bounded channel-based worker for net diagnostics generation (`NetDebug`) to replace direct ad-hoc fire-and-forget command execution.
- [x] Added explicit worker ownership and shutdown cancellation path for `NetDebug` queue processor.
- [ ] Continue migrating high-traffic ad-hoc queues (logic/network/update pipelines) to bounded, lifecycle-managed workers.

Exit Criteria:
- Worker model documented per subsystem (network, DB, logic, updater).
- Queue depth and worker throughput observable.

## Phase 3: Data and API Layer Hardening
- Consolidate data layer contracts around async methods for I/O.
- Reduce context lifetime ambiguity and enforce explicit transactional boundaries.
- Add idempotency and consistency checks around token/session/account operations.
- Normalize exception handling and remove silent failure paths.

Exit Criteria:
- Data APIs are consistent and cancellation-aware.
- Error handling produces actionable logs and metrics.

## Phase 4: Quality Gates and Tooling
- Enable/expand analyzers and warnings as errors for agreed rule sets.
- Add CI checks for anti-patterns (sync-over-async, unobserved tasks, unsafe thread usage).
- Add benchmark baselines for network serialization and DB-heavy operations.
- Add integration tests for authentication/session lifecycle and startup/shutdown flows.

Exit Criteria:
- CI prevents reintroducing fixed anti-patterns.
- Performance regression budget is defined and enforced.

## Phase 5: Incremental Nullability and API Cleanup
- Move selected core/server modules from `Nullable=disable` to `warnings`, then `enable`.
- Tighten public contracts (nullable annotations, argument validation, typed results).
- Remove obsolete compatibility shims that block refactoring.

Exit Criteria:
- Priority modules have actionable nullable warnings near zero.
- Public API contracts are explicit and test-covered.

## Phase 6: Refactoring and Performance Optimization
- Refactor high-complexity classes into smaller services with clear responsibilities.
- Remove duplicated logic in networking, token/session flows, and data migration paths.
- Reduce allocations in hot paths (packet processing, map updates, serialization).
- Add profiling-guided optimizations only after baseline metrics are captured.
- Introduce explicit performance budgets for startup time, tick latency, and DB operations.

Exit Criteria:
- Top hot paths are benchmarked before/after with measurable gains.
- Largest "god classes" have reduced complexity and clearer ownership boundaries.
- No optimization change lands without reproducible metric evidence.
