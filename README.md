# 🍩 ApiGen [![Release](https://img.shields.io/badge/dotnet-8.0.x-purple)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) [![stability-alpha](https://img.shields.io/badge/beta-0.1.x-f4d03f.svg)](https://github.com/mkenney/software-guides/blob/master/STABILITY-BADGES.md#alpha) [![License: LGPL v3](https://img.shields.io/badge/license-LGPL_v3-blue.svg)](https://www.gnu.org/licenses/lgpl-3.0)


<img src="https://sonarqube.cloudappi.net/api/project_badges/quality_gate?project=apigen-dotnet&token=sqb_c7abd9969334a4b41d2a566cc397d0aa4dea5ddc" height = "75">
<img src="imgs/logo-dotnet.png" height = "75">
<img src="imgs/logo-apiquality.png" height = "75">
<img src="https://www.openapis.org/wp-content/uploads/sites/3/2018/02/OpenAPI_Logo_Pantone-1.png" height = "75">


 [![Security Rating](https://sonarqube.cloudappi.net/api/project_badges/measure?project=apigen-dotnet&metric=security_rating&token=sqb_c7abd9969334a4b41d2a566cc397d0aa4dea5ddc)](https://sonarqube.cloudappi.net/dashboard?id=apitools-apigen-dotnet) [![Maintainability Rating](https://sonarqube.cloudappi.net/api/project_badges/measure?project=apigen-dotnet&metric=sqale_rating&token=sqb_c7abd9969334a4b41d2a566cc397d0aa4dea5ddc)](https://sonarqube.cloudappi.net/dashboard?id=apitools-apigen-dotnet) [![Coverage](https://sonarqube.cloudappi.net/api/project_badges/measure?project=apigen-dotnet&metric=coverage&token=sqb_c7abd9969334a4b41d2a566cc397d0aa4dea5ddc)](https://sonarqube.cloudappi.net/dashboard?id=apitools-apigen-dotnet) [![Lines of Code](https://sonarqube.cloudappi.net/api/project_badges/measure?project=apigen-dotnet&metric=ncloc&token=sqb_c7abd9969334a4b41d2a566cc397d0aa4dea5ddc)](https://sonarqube.cloudappi.net/dashboard?id=apitools-apigen-dotnet)
 


`Asp.Net microservice` archetype generator in dotnet with hexagonal architecture based on an openapi document with extended annotations. This project is a wrapper of the [java apigen](https://github.com/apiaddicts/apigen.springboot) with springboot but using dotnet and adapting some concepts due to the paradigm difference. The project is currently being developed by the [CloudAPPi Services](https://cloudappi.net).


[![web](https://img.shields.io/badge/try_api-purple.svg?style=for-the-badge&logo=openapiinitiative&logoColor=white)](https://api-gateway.apiquality.io/api-apigen-dotnet/v1/swagger)
[![web](https://img.shields.io/badge/sample_template-orange.svg?style=for-the-badge&logo=dotnet&logoColor=white)](https://gitlab.com/cloudappi/templates/back-templates/dotnet-template)

# ▶️ How to start

## `cli` dotnet
```
dotnet run --project ./src/Api/Api.csproj
```

## `docker`
```
docker build -t apigen .
docker run -d -p 1000:80 --name apigen.net apigen
```

## `docker-compose`
```
 docker-compose up --build -d
```

# ▶️ Usage
## Api
if you start your application you can directly access `/swagger` to see the documentation. You can also call the generation endpoint directly with a curl, you have some examples in the `src/Generator/Examples` folder

### _example api-hospital.yaml_
```
curl -X 'POST' \
  '{{url}}/generator/file' \
  -H 'accept: */*' \
  -H 'Content-Type: multipart/form-data' \
  -F 'file=@<openapi-file>'
```

## Command
Compiling the `Command` project or downloading the build

```
> apigen <openapi-path>
```

## ⏩ Next Steps
### 💿 ORM

The generation of the orm through special tags in openapi still generates problems but you can integrate the automatically generated code using the orm provided by entity framework. This generates the database context and the necessary entities.

```
dotnet tool uninstall dotnet-ef -g
dotnet tool install --global dotnet-ef
dotnet ef dbcontext scaffold <db_conexion> <driver> -o <output>
```
_example generate with `PostgreSQL`_
```
dotnet ef dbcontext scaffold "Host=<url>:<port>;Database=<db>;Username=<user>;Password=<pass>" Npgsql.EntityFrameworkCore.PostgreSQL -o Infrastructure
```