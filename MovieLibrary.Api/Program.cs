using Microsoft.Extensions.Options;
using MovieLibrary.Api.Features.Movies.SearchMovies;
using MovieLibrary.Api.Infrastructure.ErrorHandling;
using MovieLibrary.Api.Infrastructure.Tmdb;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddExceptionHandler<UpstreamExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddValidation();
builder.Services.AddScoped<SearchMoviesHandler>();

builder.Services.AddOptions<TmdbOptions>()
    .BindConfiguration("Tmdb")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<TmdbClient>((serviceProvider, client) =>
{
    var tmdb = serviceProvider.GetRequiredService<IOptions<TmdbOptions>>().Value;

    client.BaseAddress = new Uri(tmdb.BaseUrl);

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", tmdb.ApiKey);
});

var app = builder.Build();

app.UseExceptionHandler();
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


