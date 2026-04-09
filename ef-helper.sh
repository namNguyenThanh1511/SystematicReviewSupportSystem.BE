#!/bin/bash

# PATH TO PROJECTS
REPO_PATH="SRSS.IAM/SRSS.IAM.Repositories/SRSS.IAM.Repositories.csproj"
API_PATH="SRSS.IAM/SRSS.IAM.API/SRSS.IAM.API.csproj"

case $1 in
  add)
    if [ -z "$2" ]; then
      echo "Please input migration name (Ex: bash ef-helper.sh add InitialCreate)"
    else
      dotnet ef migrations add "$2" -p "$REPO_PATH" -s "$API_PATH"
    fi
    ;;
  update)
    dotnet ef database update -p "$REPO_PATH" -s "$API_PATH"
    ;;
  remove)
    dotnet ef migrations remove -p "$REPO_PATH" -s "$API_PATH"
    ;;
  drop)
    dotnet ef database drop -p "$REPO_PATH" -s "$API_PATH"
    ;;
  *)
    echo "Use case : bash ef-helper.sh {add|update|remove|drop} [migration_name]"
    ;;
esac