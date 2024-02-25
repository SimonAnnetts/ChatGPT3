#!/bin/bash

# Build the project
echo "Building the project"
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

cd $SCRIPT_DIR
dotnet publish -c Release  -p:SelfContained=true -p:PublishSingleFile=true -r linux-x64
dotnet publish -c Release  -p:SelfContained=true -p:PublishSingleFile=true -r win-x64
dotnet publish -c Release  -p:SelfContained=true -p:PublishSingleFile=true -r osx-x64

