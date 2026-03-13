#!/bin/bash
# install .NET first
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
bash dotnet-install.sh --channel 10.0
export PATH=$HOME/.dotnet:$PATH

# build & run
dotnet publish TickrServer/TickrServer.csproj -c Release -o ./publish
dotnet ./publish/TickrServer.dll