
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build

ENV SONAR_PROJECT_KEY=apigen-dotnet
ARG SONAR_HOST_URL
ARG SONAR_TOKEN
ENV SONAR_HOST_URL=$SONAR_HOST_URL
ENV SONAR_TOKEN=$SONAR_TOKEN

WORKDIR /app
COPY . .


RUN apt-get update && apt-get install -y openjdk-11-jdk
RUN dotnet tool install --global dotnet-sonarscanner
RUN dotnet tool install --global coverlet.console
ENV PATH="$PATH:/root/.dotnet/tools"

RUN echo "URL: $SONAR_HOST_URL"
RUN echo "KEY: $SONAR_PROJECT_KEY"

RUN dotnet sonarscanner begin \
  /k:"$SONAR_PROJECT_KEY" \
  /d:sonar.host.url="$SONAR_HOST_URL" \
  /d:sonar.login="$SONAR_TOKEN" \
  /d:sonar.cs.opencover.reportsPaths=/coverage.opencover.xml
  

RUN dotnet build src/Api/Api.csproj -c Release -o /app/build
RUN dotnet test src/Test \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput="/coverage"


RUN dotnet sonarscanner end /d:sonar.login="$SONAR_TOKEN"


FROM mcr.microsoft.com/dotnet/aspnet:7.0.2-alpine3.17-amd64

WORKDIR /app
COPY --from=build /app/build .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Api.dll"]

