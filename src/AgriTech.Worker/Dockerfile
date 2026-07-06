FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

COPY src/ src/
COPY AgriTech.sln .

RUN dotnet restore src/AgriTech.Worker/AgriTech.Worker.csproj

WORKDIR /app/src/AgriTech.Worker
RUN dotnet build AgriTech.Worker.csproj -c Release -o /app/build

RUN dotnet publish AgriTech.Worker.csproj -c Release -o /app/publish \
    --no-restore \
    --self-contained false \
    --framework net8.0

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
RUN apk add --no-cache curl
COPY --from=build /app/publish .

RUN adduser -D -u 1000 appuser && \
    chown -R appuser:appuser /app
USER appuser

EXPOSE 80

ENV DOTNET_RUNNING_IN_CONTAINER=true

HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
    CMD pgrep dotnet || exit 1

    
ENTRYPOINT ["dotnet", "AgriTech.Worker.dll"]