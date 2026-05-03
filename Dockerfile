# Stage 1: Base Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RentalVehicleService.csproj", "./"]
RUN dotnet restore "RentalVehicleService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "RentalVehicleService.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "RentalVehicleService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RentalVehicleService.dll"]