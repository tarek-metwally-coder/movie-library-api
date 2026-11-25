# Movie Library API
[![CI](https://github.com/tarek-metwally-coder/movie-library-api/actions/workflows/ci.yml/badge.svg)](https://github.com/tarek-metwally-coder/movie-library-api/actions/workflows/ci.yml)

An ASP.NET Core Web API that powers the [movie-library-web](https://github.com/tarek-metwally-coder/movie-library-web) frontend. It proxies [TMDB](https://www.themoviedb.org/) server-side so the API token never reaches the browser, and returns its own clean, frontend-friendly response shapes.

Built with **vertical-slice architecture** and minimal APIs.

## Tech stack
- **.NET 10** / ASP.NET Core minimal APIs
- `IHttpClientFactory` with a typed TMDB client
- Built-in minimal API validation (DataAnnotations)
- RFC 9457 ProblemDetails for all error responses
- OpenAPI + Swagger UI
- xUnit v3: unit tests plus integration tests with `WebApplicationFactory` and a stubbed TMDB
- Docker (multi-stage image) and GitHub Actions CI

## Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/movies/search?query={text}&page={n}` | Search movies by title |
| `GET` | `/health` | Health check |

### Example: search

```http
GET /api/movies/search?query=batman&page=1
```

```json
{
  "page": 1,
  "totalResults": 177,
  "totalPages": 9,
  "results": [
    {
      "id": 414906,
      "title": "The Batman",
      "overview": "In his second year of fighting crime, Batman uncovers corruption in Gotham City...",
      "posterUrl": "https://image.tmdb.org/t/p/w500/74xTEgt7R36Fpooo50r9T25onhq.jpg",
      "backdropUrl": "https://image.tmdb.org/t/p/w1280/rvtdN5XkWAfGX6xDuPL6yYS2seK.jpg",
      "releaseDate": "2022-03-01",
      "voteAverage": 7.674,
      "originalLanguage": "en"
    }
  ]
}
```

### Errors

Every error uses the same [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) shape, so clients only need one way to read them.

| Status | When |
|---|---|
| `400` | Invalid input: missing, empty or whitespace-only `query`; `page` outside 1–500 or not a number |
| `502` | TMDB failed or rejected the request. Details are logged server-side and never returned to the client |
| `500` | Any other unhandled error |

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.3",
  "title": "Upstream Service Error",
  "status": 502,
  "detail": "An error occurred while communicating with an upstream service.",
  "instance": "/api/movies/search",
  "traceId": "00-c28b6ee4be08d5d0f2a6e547ea61265d-f83b83745bb894a0-00"
}
```

## Project structure

```
MovieLibrary.Api/
├── Features/                  # One folder per feature, one sub-folder per slice
│   ├── Common/                # Shared across all features (PagedResponse<T>)
│   └── Movies/
│       ├── MovieItem.cs       # Response DTO shared by Movies slices
│       ├── MovieMapper.cs     # TMDB → MovieItem mapping
│       └── SearchMovies/      # Endpoint + handler for this slice
├── Infrastructure/
│   ├── Tmdb/                  # Typed HTTP client, TMDB DTOs, options
│   └── ErrorHandling/         # Exception → ProblemDetails mapping (502 upstream, 400 bad requests)
└── Program.cs
MovieLibrary.Api.Tests/
├── Features/                  # Mirrors the API: mapper, handler and endpoint tests
├── Fixtures/                  # MovieLibraryApiFactory: the real app with TMDB stubbed out
└── TestDoubles/               # Hand-written stubs (no mocking library)
docs/decisions.md              # Design decisions and the reasoning behind them
Dockerfile                     # Multi-stage build: SDK to publish, ASP.NET runtime to run
.github/workflows/ci.yml       # Build, test and build the image on every push and PR
```

## Design decisions

- **Vertical slices over layers.** Each operation owns its endpoint and handler in one folder, so adding a feature means adding a folder, not editing shared controllers and services. The trade-off is some duplication. Shared code lives at the lowest level that covers every user (`Common/` app-wide, `Movies/` for Movies slices).
- **Own response DTOs, not TMDB's.** The API maps TMDB's shape at the boundary: it builds full image URLs, normalizes missing values to `null`, and parses dates as `DateOnly` using the invariant culture. If TMDB changes, only the mapping changes and the frontend contract stays stable.
- **Typed `HttpClient` via `IHttpClientFactory`.** Avoids socket exhaustion and stale DNS. Base address and auth header are configured once at registration.
- **Fail fast on config.** TMDB options are validated on startup, so a missing token stops the app instead of failing on the first request.
- **Honest status codes.** Upstream failures return `502 Bad Gateway`, not `500`, so it's clear whether this API or its dependency failed.
- **Tested at two levels.** Unit tests cover the mapper and handler with hand-written stubs. Integration tests run the real app in memory with only TMDB's HTTP handler replaced, so routing, validation, JSON handling and error mapping are tested together. They found and fixed a bug where invalid parameters returned `500` instead of `400`.
- **No secrets in CI or images.** Tests use a dummy token and a stubbed TMDB; the Docker image receives the token only when the container starts.

The full log, with alternatives considered, is in [docs/decisions.md](docs/decisions.md).

## Getting started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A TMDB account and its **API Read Access Token** (the long token, from TMDB → Settings → API)

### Run locally

```bash
git clone https://github.com/tarek-metwally-coder/movie-library-api.git
cd movie-library-api/MovieLibrary.Api

# Store your TMDB token outside the repo
dotnet user-secrets set "Tmdb:ApiKey" "<your-tmdb-read-access-token>"

dotnet run --launch-profile https
```

Swagger UI opens at `https://localhost:7002/swagger`.

> Outside your machine, provide the token as an environment variable instead: `Tmdb__ApiKey`.

### Run with Docker

```bash
docker build -t movielibrary-api .
docker run --rm -p 8080:8080 -e Tmdb__ApiKey="<your-tmdb-read-access-token>" movielibrary-api
```

The API is then at `http://localhost:8080` (for example `http://localhost:8080/api/movies/search?query=matrix`). The container runs in the Production environment, so Swagger UI is not exposed.

### Run the tests

```bash
dotnet test --solution MovieLibrary.slnx
```

No TMDB token is needed: the tests never call TMDB.

## Roadmap
- [x] Unit and integration tests for the search slice
- [x] Dockerfile and GitHub Actions CI
- [ ] Popular movies slice
- [ ] PostgreSQL + EF Core, with Docker Compose for local development
- [ ] Trending searches (replaces the frontend's Appwrite integration)
- [ ] JWT authentication and user watchlists
- [ ] Cloud deployment with Terraform, continuous deployment and observability
