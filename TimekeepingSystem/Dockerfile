FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY . .

RUN dotnet restore "TimekeepingSystem.csproj"
RUN dotnet build "TimekeepingSystem.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TimekeepingSystem.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Render truyền biến môi trường PORT lúc runtime.
# Dùng shell entrypoint để thay thế ${PORT} bằng PORT thật của Render (mặc định 10000).
ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["sh", "-c", "exec dotnet TimekeepingSystem.dll --urls http://+:${PORT:-10000}"]

