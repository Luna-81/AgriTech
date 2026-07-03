FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

COPY src/ src/
COPY AgriTech.sln .

RUN dotnet restore src/WebAPI/WebAPI.csproj

WORKDIR /app/src/WebAPI
RUN dotnet build WebAPI.csproj -c Release -o /app/build

RUN dotnet publish WebAPI.csproj -c Release -o /app/publish \
    --no-restore \
    --self-contained false \
    --framework net8.0

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
RUN apk add --no-cache curl
COPY --from=build /app/publish .

RUN adduser -D -u 1000 appuser
USER appuser

EXPOSE ${WEBAPI_INTERNAL_PORT:-80}

ENV DOTNET_RUNNING_IN_CONTAINER=true

HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:${WEBAPI_INTERNAL_PORT:-80}/health || exit 1

    
ENTRYPOINT ["dotnet", "WebAPI.dll"]