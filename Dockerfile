FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY MovieLibrary.Api/MovieLibrary.Api.csproj MovieLibrary.Api/
RUN dotnet restore MovieLibrary.Api/MovieLibrary.Api.csproj
COPY . .
RUN dotnet publish MovieLibrary.Api/MovieLibrary.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MovieLibrary.Api.dll"]