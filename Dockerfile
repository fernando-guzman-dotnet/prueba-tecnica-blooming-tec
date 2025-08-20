# Multi-stage build for BloomingTec Todo API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/BloomingTec.Todo.Api/BloomingTec.Todo.Api.csproj", "src/BloomingTec.Todo.Api/"]
COPY ["src/BloomingTec.Todo.Application/BloomingTec.Todo.Application.csproj", "src/BloomingTec.Todo.Application/"]
COPY ["src/BloomingTec.Todo.Domain/BloomingTec.Todo.Domain.csproj", "src/BloomingTec.Todo.Domain/"]
COPY ["src/BloomingTec.Todo.Infrastructure/BloomingTec.Todo.Infrastructure.csproj", "src/BloomingTec.Todo.Infrastructure/"]
RUN dotnet restore "src/BloomingTec.Todo.Api/BloomingTec.Todo.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/BloomingTec.Todo.Api"
RUN dotnet build "BloomingTec.Todo.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "BloomingTec.Todo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Labels for better container management
LABEL maintainer="BloomingTec"
LABEL version="1.0.0"
LABEL description="BloomingTec Todo API - .NET 8 REST API for task management"

ENTRYPOINT ["dotnet", "BloomingTec.Todo.Api.dll"]
