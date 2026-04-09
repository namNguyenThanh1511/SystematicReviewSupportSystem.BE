#!/bin/bash

# 1. Change directory to solution (.sln)
cd SRSS.IAM

echo "--- Restoring Nuget packages ---"
dotnet restore

echo "--- Building all projects  ---"
dotnet build --no-restore

# If build succeed then run api
if [ $? -eq 0 ]; then
    echo "--- Starting Web API ---"
    # Run project api form solution 
    dotnet run --project SRSS.IAM.API/SRSS.IAM.API.csproj
else
    echo "--- Build failed ---"
fi