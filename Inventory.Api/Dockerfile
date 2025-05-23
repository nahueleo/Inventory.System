FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Inventory.Api/Inventory.Api.csproj", "Inventory.Api/"]
COPY ["Inventory.Domain/Inventory.Domain.csproj", "Inventory.Domain/"]
COPY ["Inventory.Infrastructure/Inventory.Infrastructure.csproj", "Inventory.Infrastructure/"]
RUN dotnet restore "Inventory.Api/Inventory.Api.csproj"

# Copy the rest of the code
COPY . .

# Build and publish
RUN dotnet build "Inventory.Api/Inventory.Api.csproj" -c Release -o /app/build
RUN dotnet publish "Inventory.Api/Inventory.Api.csproj" -c Release -o /app/publish

# Create data directory and set permissions
RUN mkdir -p /app/data && chmod 777 /app/data

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /app/data /app/data
COPY --from=build /src/Inventory.Api/entrypoint.sh .

# Create data directory and set permissions in the final image
RUN mkdir -p /app/data && chmod 777 /app/data && \
    chmod +x /app/entrypoint.sh

# Run migrations and start the app
ENTRYPOINT ["/app/entrypoint.sh"]

