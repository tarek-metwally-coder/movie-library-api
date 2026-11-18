using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MovieLibrary.Api.Infrastructure.Tmdb;
using MovieLibrary.Api.Tests.TestDoubles;

namespace MovieLibrary.Api.Tests.Fixtures;

public class MovieLibraryApiFactory : WebApplicationFactory<Program>
{
    public StubHttpMessageHandler Stub { get; } = new();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Tmdb:ApiKey", "test-key");
        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClient<ITmdbClient, TmdbClient>()
            .ConfigurePrimaryHttpMessageHandler(() => Stub);
        });
    }
}
