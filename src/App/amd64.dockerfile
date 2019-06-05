FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app

COPY /src/App/out.amd64 .
EXPOSE 80
ENTRYPOINT ["dotnet", "App.dll"]