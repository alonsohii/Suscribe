# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["Suscripcion.Api/Suscripcion.Api.csproj", "Suscripcion.Api/"]
COPY ["Suscripcion.Application/Suscripcion.Application.csproj", "Suscripcion.Application/"]
COPY ["Suscripcion.Infrastructure/Suscripcion.Infrastructure.csproj", "Suscripcion.Infrastructure/"]
COPY ["Sucropcion.Domain/Sucripcion.Domain.csproj", "Sucropcion.Domain/"]

# Restore dependencies
RUN dotnet restore "Suscripcion.Api/Suscripcion.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/Suscripcion.Api"
RUN dotnet build "Suscripcion.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "Suscripcion.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Suscripcion.Api.dll"]
