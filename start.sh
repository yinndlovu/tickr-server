#!/bin/bash
# Build and publish the TickrServer project
dotnet publish TickrServer/TickrServer.csproj -c Release -o ./publish

# Run the published DLL
dotnet ./publish/TickrServer.dll