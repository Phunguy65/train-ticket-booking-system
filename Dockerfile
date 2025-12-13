FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["backend/backend.csproj", "backend/"]
RUN dotnet restore "backend/backend.csproj"

COPY backend/ backend/
WORKDIR "/src/backend"
RUN dotnet build "backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 5000 5001

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5001
ENV TcpServer__Port=5000
ENV SignalR__Host=0.0.0.0
ENV SignalR__Port=5001

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5001/health || exit 1

ENTRYPOINT ["dotnet", "backend.dll"]

