# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# Build
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY VibeLang.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Runtime
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# The app writes uploaded profile pictures under wwwroot and keeps Identity
# data-protection keys in the app user's home. Create both before dropping
# privileges so mounted volumes inherit the right ownership.
RUN mkdir -p /app/wwwroot/uploads/profile-pictures /home/app/.aspnet/DataProtection-Keys \
 && chown -R app:app /app /home/app

USER app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "VibeLang.dll"]
