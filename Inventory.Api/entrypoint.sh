#!/bin/bash

# Wait for the database to be ready
echo "Waiting for database to be ready..."
sleep 5

# Run migrations
echo "Running database migrations..."
dotnet ef database update --project ../Inventory.Infrastructure --startup-project ../Inventory.Api

# Start the application
echo "Starting application..."
dotnet Inventory.Api.dll 