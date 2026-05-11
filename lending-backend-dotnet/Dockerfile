FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LendingSystem.slnx ./
COPY proto ./proto
COPY src ./src
RUN dotnet restore LendingSystem.slnx
RUN dotnet publish src/LendingSystem.WebApi/LendingSystem.WebApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV APP_PORT=8000
EXPOSE 8000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LendingSystem.WebApi.dll"]
