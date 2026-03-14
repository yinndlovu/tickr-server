#!/bin/bash
dotnet publish TickrServer/TickrServer.csproj -c Release -o out
dotnet out/TickrServer.dll