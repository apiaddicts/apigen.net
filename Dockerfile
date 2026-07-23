FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine-amd64 as build

WORKDIR /app
COPY . .

RUN apk add --no-cache openjdk17

RUN dotnet build src/Api/Api.csproj -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine-amd64

USER app

WORKDIR /app
COPY --from=build /app/build .

ENTRYPOINT ["dotnet", "Api.dll"]
