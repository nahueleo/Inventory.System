FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Inventory.Worker/Inventory.Worker.csproj", "Inventory.Worker/"]
COPY ["Inventory.Domain/Inventory.Domain.csproj", "Inventory.Domain/"]
COPY ["Inventory.Infrastructure/Inventory.Infrastructure.csproj", "Inventory.Infrastructure/"]
RUN dotnet restore "Inventory.Worker/Inventory.Worker.csproj"
COPY . .
WORKDIR "/src/Inventory.Worker"
RUN dotnet build "Inventory.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Inventory.Worker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/data && chmod 777 /app/data
ENTRYPOINT ["dotnet", "Inventory.Worker.dll"]