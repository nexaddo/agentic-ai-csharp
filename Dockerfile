FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./src ./src
COPY SupportAgent.sln .
RUN dotnet restore
RUN dotnet publish src/WebApi/WebApi.csproj -c Release -o /out
FROM base AS final
WORKDIR /app
COPY --from=build /out .
EXPOSE 5000
ENTRYPOINT ["dotnet","WebApi.dll"]
