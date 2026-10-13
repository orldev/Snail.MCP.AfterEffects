# syntax=docker/dockerfile:1
# The MCP server over HTTP, with its pages, for an After Effects that runs on the host beside the container.
# Built from the repository root: docker build -f docker/server/Dockerfile .
# Pinned to the band global.json asks for: on the floating tag a newer feature band would refuse the build, and the image and the NuGet package
# of one release would come from different toolchains.
# Framework-dependent (-p:PublishAot=false) onto aspnet:10.0: the Razor pages and the reflection they run on keep the server out of Native AOT,
# and the runtime image takes the patches of 10.0 as they ship.
#
# VERSION is the release's when the shared mcp-image.yml builds the image (0.0.0-<branch>.<commit> from a branch)
# and 0.0.0-local otherwise: the server reports it in initialize and the image carries it as its OCI version.
ARG VERSION=0.0.0-local

FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS build
WORKDIR /source

# .editorconfig sets the analyzers' severities; without it the build treats warnings the checkout never raises as errors.
COPY .editorconfig Directory.Build.props Directory.Packages.props README.md LICENSE global.json ./
COPY .mcp/server.json .mcp/server.json
COPY extension extension
COPY src src

# The checkout holds a placeholder version; a release passes the one of its tag, which ae_diagnose then reports.
ARG VERSION
RUN dotnet publish src/Snail.MCP.AfterEffects.csproj -c Release -o /app -p:PublishAot=false -p:UseAppHost=false -p:Version="$VERSION"

FROM mcr.microsoft.com/dotnet/aspnet:10.0
ARG VERSION
LABEL org.opencontainers.image.version=$VERSION
# tini runs as process 1 and starts the program under it: .NET ends an unhandled exception with SIGABRT, which process
# 1 of a container ignores, so run as process 1 itself a program that fails at start spins at a full core and never
# exits, and nothing restarts it. Under tini it exits with 134.
RUN apt-get update \
    && apt-get install --yes --no-install-recommends tini \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .

# The data directory is a bind mount of the host folder After Effects' bridge reads, at the same path on both sides, so every file path
# the server hands After Effects means the same file there. Every request at Information buries what matters in a container's log.
ENV SNAIL_MCP_AFTEREFFECTS_TRANSPORT=Http \
    SNAIL_MCP_AFTEREFFECTS_HTTP__URL=http://0.0.0.0:8080 \
    SNAIL_MCP_AFTEREFFECTS_BRIDGE__PORT=9876 \
    SNAIL_MCP_AFTEREFFECTS_BRIDGE__LISTEN=0.0.0.0 \
    SNAIL_MCP_AFTEREFFECTS_WORKERS__MAX=0 \
    Logging__LogLevel__Default=Information \
    Logging__LogLevel__Microsoft.AspNetCore=Warning \
    Logging__LogLevel__ModelContextProtocol=Warning

# The keys that sign sessions and form tokens live in a volume, so a recreated container does not sign everyone out.
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys && chown -R app:app /home/app/.aspnet
USER app
EXPOSE 8080 9876
ENTRYPOINT ["/usr/bin/tini", "--", "dotnet", "Snail.MCP.AfterEffects.dll"]
