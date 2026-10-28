# Design decisions

A running log of technical decisions in this API and the reasoning behind them.

## Vertical-slice architecture
Each operation (e.g. `SearchMovies`) owns its endpoint and handler in one folder under `Features/`. Adding a feature means adding a folder rather than editing shared controllers and services.
**Trade-off:** some duplication between slices. Shared code lives at the lowest level that covers every user: `Features/Common` for app-wide types, `Features/Movies` for code shared by Movies slices.

## Minimal APIs instead of controllers
Controllers group endpoints by resource and inject dependencies for every action. Minimal API endpoints are functions, so each slice registers its own endpoint (`MapSearchMovies()`) and asks only for what it needs.

## No mediator library
MediatR-style pipelines were considered. Minimal API endpoint filters cover the same cross-cutting needs natively, and at this project's size a mediator adds indirection without much benefit. (MediatR also moved to a commercial license in 2025.)

## Typed HttpClient via IHttpClientFactory
Avoids socket exhaustion (new client per call) and stale DNS (one static client forever). Base address and Bearer token are configured once at registration, so `TmdbClient` methods only build relative paths.

## `ITmdbClient` abstraction
Handlers depend on `ITmdbClient`, not the concrete `TmdbClient`, so they can be unit tested with a stub instead of real HTTP. It is registered with `AddHttpClient<ITmdbClient, TmdbClient>()`, so the implementation still receives the configured `HttpClient`. A second plain `AddScoped<ITmdbClient, TmdbClient>()` would override that registration and give the client an unconfigured `HttpClient`.

## Configuration: options pattern, validated on start
TMDB settings bind to `TmdbOptions` with `[Required]` and `ValidateOnStart()`, so missing config stops the app at startup instead of failing on the first request. Non-secret values live in `appsettings.json`; the token comes from user secrets locally and environment variables (`Tmdb__ApiKey`) elsewhere.

## Own response DTOs, mapped at the boundary
TMDB's shape is never returned to clients. `MovieMapper` builds full image URLs, normalizes missing values to `null` (null, empty and whitespace-only image paths all count as missing, so clients never get a broken URL), and parses dates into `DateOnly` with `TryParseExact` and the invariant culture (machine data shouldn't depend on server culture). If TMDB changes, only the mapping changes.

## Shared `PagedResponse<T>`
Every paged endpoint returns the same paging fields, so one generic wrapper lives in `Features/Common`. A slice with genuinely different paging needs would define its own.

## Validation: built-in minimal API validation
.NET 10's `AddValidation()` with DataAnnotations on parameters replaces manual `if` checks and returns a standard 400 ProblemDetails.

## Errors: ProblemDetails everywhere, honest status codes
All errors use RFC 9457 ProblemDetails with a `traceId`. Upstream (TMDB) failures map to **502 Bad Gateway**, not 500, so it's clear whether this API or its dependency failed. Exception details are logged server-side, never returned to clients.
Exceptions are reserved for unexpected failures; expected outcomes (e.g. "not in watchlist") will be decided per slice (Result pattern vs exceptions).

## Testing approach
- Expected values in tests are literals, never recomputed with the same logic as production code. Exception: tests of code that only *delegates* (e.g. a handler that returns the mapper's output) build the expected value with the collaborator, so mapping rules are asserted once, in the mapper's own tests
- Shared valid test data via builders plus record `with` expressions to vary one property per case
- Private helpers are tested through public behaviour
- `[Theory]` + `[InlineData]` for the same rule over several inputs (e.g. every form of "missing" image path); `[MemberData]` with `TheoryData<T>` when the inputs aren't compile-time constants (e.g. exception instances)
- Hand-written test doubles in `Tests/TestDoubles/` instead of a mocking library: a stub returns canned data or throws, and records the arguments it received
- Forwarding tests use values a bug couldn't produce by accident: a real `CancellationTokenSource` token (not `CancellationToken.None`), a non-default page
- Handlers must let upstream exceptions propagate unchanged so `UpstreamExceptionHandler` can map them to 502; tests check this with `Assert.ThrowsAnyAsync` + `Assert.Same`
- Record equality is shallow: collection properties compare by reference, so paged responses assert their fields and compare result lists element-wise with `Assert.Equal`, or the whole object structurally with `Assert.Equivalent`
- Plain xUnit assertions only. FluentAssertions was considered for deep equality but v8+ needs a commercial license; free alternatives (Shouldly, AwesomeAssertions) exist if assertion noise becomes a real problem
