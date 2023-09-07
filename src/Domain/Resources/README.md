[![Release](https://img.shields.io/badge/dotnet-7.0-purple)](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
# [[projectName]] [[projectId]] 

## Deploy

```
docker build -t apigen .
docker run -d -p 1000:80 --name apigen.net apigen
```