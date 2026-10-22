# Movie Library API

An ASP.NET Core Web API that powers the [movie-library-web](https://github.com/tarek-metwally-coder/movie-library-web) frontend. It proxies [TMDB](https://www.themoviedb.org/) server-side so the API token never reaches the browser, and returns its own clean, frontend-friendly response shapes.

Built with **vertical-slice architecture** and minimal APIs.

## Tech stack
- **.NET 10** / ASP.NET Core minimal APIs
- `IHttpClientFactory` with a typed TMDB client
- Built-in minimal API validation (DataAnnotations)
- RFC 9457 ProblemDetails for all error responses
- OpenAPI + Swagger UI
- xUnit (test project)

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
| `400` | Invalid input: missing or whitespace-only `query`, `page` outside 1–500 |
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
│   └── ErrorHandling/         # Exception → ProblemDetails mapping
└── Program.cs
MovieLibrary.Api.Tests/
```

## Design decisions

- **Vertical slices over layers.** Each operation owns its endpoint and handler in one folder, so adding a feature means adding a folder, not editing shared controllers and services. The trade-off is some duplication. Shared code lives at the lowest level that covers every user (`Common/` app-wide, `Movies/` for Movies slices).
- **Own response DTOs, not TMDB's.** The API maps TMDB's shape at the boundary: it builds full image URLs, normalizes missing values to `null`, and parses dates as `DateOnly` using the invariant culture. If TMDB changes, only the mapping changes and the frontend contract stays stable.
- **Typed `HttpClient` via `IHttpClientFactory`.** Avoids socket exhaustion and stale DNS. Base address and auth header are configured once at registration.
- **Fail fast on config.** TMDB options are validated on startup, so a missing token stops the app instead of failing on the first request.
- **Honest status codes.** Upstream failures return `502 Bad Gateway`, not `500`, so it's clear whether this API or its dependency failed.

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

> In Docker or production, provide the token as an environment variable instead: `Tmdb__ApiKey`.

## Roadmap
- [ ] Unit and integration tests for the search slice
- [ ] Popular movies slice
- [ ] PostgreSQL + EF Core
- [ ] Trending searches (replaces the frontend's Appwrite integration)
- [ ] JWT authentication and user watchlists
- [ ] Docker Compose and GitHub Actions CI
