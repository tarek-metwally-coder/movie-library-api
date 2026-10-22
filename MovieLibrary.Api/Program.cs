using Microsoft.Extensions.Options;
using MovieLibrary.Api.Features.Movies.SearchMovies;
using MovieLibrary.Api.Infrastructure.Tmdb;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<SearchMoviesHandler>();

builder.Services.AddOptions<TmdbOptions>()
    .BindConfiguration("Tmdb")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<TmdbClient>((serviceProvider, client) =>
{
    // Ask the DI container for the validated options you registered above
    var tmdb = serviceProvider.GetRequiredService<IOptions<TmdbOptions>>().Value;

    // Every request from this client starts from this address
    client.BaseAddress = new Uri(tmdb.BaseUrl);

    // Every request from this client carries the token. Set once, never repeated.
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", tmdb.ApiKey);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(settings =>
    {
        settings.SwaggerEndpoint("/openapi/v1.json", "MovieLibrary.Api v1");
    });
}

app.UseHttpsRedirection();



app.MapHealthChecks("/health");

app.MapSearchMovies();

app.Run();


