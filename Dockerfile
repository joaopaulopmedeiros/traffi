#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FileMigration/FileMigration.csproj", "FileMigration/"]
RUN dotnet restore "FileMigration/FileMigration.csproj"
COPY . .
WORKDIR "/src/FileMigration"
RUN dotnet build "FileMigration.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileMigration.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileMigration.dll"]