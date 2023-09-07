# 🍩 ApiGen [![Release](https://img.shields.io/badge/dotnet-7.0.2-purple)](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) [![stability-alpha](https://img.shields.io/badge/version-alpha_0.0.1-f4d03f.svg)](https://github.com/mkenney/software-guides/blob/master/STABILITY-BADGES.md#alpha) [![License: LGPL v3](https://img.shields.io/badge/license-LGPL_v3-blue.svg)](https://www.gnu.org/licenses/lgpl-3.0) 

<img src="imgs/logo-dotnet.png" height = "75">
<img src="imgs/logo-apiquality.png" height = "75">
<img src="https://www.openapis.org/wp-content/uploads/sites/3/2018/02/OpenAPI_Logo_Pantone-1.png" height = "75">


`Asp.Net microservice` archetype generator in dotnet with hexagonal architecture based on an openapi document with extended annotations. This project is a wrapper of the [java apigen](https://github.com/apiaddicts/apigen.springboot) with springboot but using dotnet and adapting some concepts due to the paradigm difference. The project is currently being developed by the [CloudAPPi Services](https://cloudappi.net).

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
if you start your application you can directly access `/swagger` to see the documentation. You can also call the generation endpoint directly with a curl, you have some examples in the `Doc/Examples/` folder

### _example api-hospital.yaml_
```
curl -X 'POST' \
  '{{url}}/generator/file' \
  -H 'accept: */*' \
  -H 'Content-Type: multipart/form-data' \
  -F 'file=@api-hospital.yaml'
```

# 💟 Status


<details>
  <summary>

## 🧱 Feature

</summary>

The following table shows a state of the functionalities, remember that the project is currently in `alpha` state and the generation of archetypes may vary and generate some errors.

| Features    | Status | 📜          |
| ----------- | ----------- | ----------- |
| CRUD                  | Ok                                                    | ✔️    |
| Expand                | Ok. No depth limit                                    | ✔️    |
| Select                | Ok. Needs QA                                          | ✔️    |
| Search                | Some operations                                       | ⚠️    |
| Pagination            | Ok. Not dynamic                                       | ✔️    |
| Dynamic Response      | Needs definition                                      | 🚨    |
| OrderBy               | Ok                                                    | ✔️    |
| Control Exception     | Ok. Not custom messages                               | ✔️    |
| dBContext             | Needs more testing and quality control. Only works with the 'api-hospital' example                | 🚨    |
| OpenApi               | Needs quality control                                 | 🚨    |

</details> 

<details>
  <summary>

## 🔍 Standard Search
 
</summary>

| Operation    | Status | 📜          |
| ----------- | ----------- | ----------- |
| AND                  | Ok. Needs QA                                                     | ✔️    |
| OR                  | Ok. Needs QA                                                      | ✔️    |
| GT                  | Ok. Needs QA                                                      | ✔️    |
| LT                  | Ok. Needs QA                                                      | ✔️    |
| GTEQ                | Ok. Needs QA                                                     | ✔️    |
| LTEQ                | Ok. Needs QA                                                      | ✔️    |
| EQ                  | Ok. Needs QA                                                      | ✔️    |
| SUBSTRING           | Ok. Needs QA                                                     | ✔️    |
| LIKE                | Ok. Needs QA                                                                            | ✔️    |
| NLIKE               | Ok. Needs QA                                                                               | ✔️    |
| ILIKE               | Ok. Needs QA                                                                             | ✔️    |
| IN                  | Ok. Needs QA                                                                  | ✔️     |
| BETWEEN             | Ok. Needs QA                                                                | ✔️    |
| REGEXP              | Not Implement                                                                 | 🚨    |

</details> 

<details>
  <summary>

# 🎲 Examples

</summary>

## 🔎 Search operations

### json body for search queries for the openapi `api-hospital.yaml` provided in the examples. ⚠️ don't work with lists.

### `EQ`
```
{
    "filter": {
        "operation": "EQ",
        "values": [
            {
                "property": "cause",
                "value": "Crocodile bite"
            }
        ]
    }
}
```
### variant `LIKE` with `OR`
```
{
    "filter": {
        "operation": "OR",
        "values": [
            {
                "filter": {
                    "operation": "LIKE",
                    "values": [
                        {
                            "property": "cause",
                            "value": "Cro%"
                        }
                    ]
                }
            },
            {
                "filter": {
                    "operation": "ILIKE",
                    "values": [
                        {
                            "property": "room.code",
                            "value": "%2"
                        }
                    ]
                }
            }
        ]
    }
}
```
### `BETWEEN`
```
{
    "filter": {
        "operation": "BETWEEN",
        "values": [
            {
                "property": "entrydate",
                "value": ["2021-01-01", "2021-08-30"]
            }
        ]
    }
}
```

### variant `IN` with `AND`
```
{
    "filter": {
        "operation": "AND",
        "values": [
            {
                "filter": {
                    "operation": "IN",
                    "values": [
                        {
                            "property": "cause",
                            "value": ["Dog bite","Burn"]
                        }
                    ]
                }
            },
            {
                "filter": {
                    "operation": "IN",
                    "values": [
                        {
                            "property": "room.active",
                            "value": [true]
                        }
                    ]
                }
            }
        ]
    }
}

```
</details> 

# 📚 TargetFramework .net 7
the libraries that extend from others are not indicated in the description
## 🏡 apigen core libraries

| Name        | Descripción |   
| ----------- | ----------- | 
| [![CodegenCS](https://shields.io/badge/CodegenCS-3.0.1-004880?logo=Nuget&style=flat)](https://github.com/Drizin/CodegenCS)         | Class Library and Toolkit for Code Generation using plain C#                        |
| [![DotNetZip](https://shields.io/badge/DotNetZip-1.16.0-004880?logo=Nuget&style=flat)](https://github.com/haf/DotNetZip.Semverd)             | Create, extract, or update zip files        |1.16.0|
| [![OpenAPI.NET](https://shields.io/badge/OpenAPI.NET-1.3.2-004880?logo=Nuget&style=flat)](https://github.com/Microsoft/OpenAPI.NET)               | Extract raw OpenAPI JSON and YAML documents from the model                                               |
| [![Serilog](https://shields.io/badge/Serilog-2.12.0-004880?logo=Nuget&style=flat)](https://serilog.net/)               | Simple .NET logging with fully-structured events support                                              |
| [![Swashbuckle](https://shields.io/badge/Swashbuckle-6.4.0-004880?logo=Nuget&style=flat)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)              | Swagger tools for documenting APIs built on ASP.NET Coreframework                                              |
## ✏️ libraries that the generated project has

| Name        | Descripción | 
| ----------- | ----------- | 
| [![AutoMapper](https://shields.io/badge/AutoMapper-12.0.0-004880?logo=Nuget&style=flat)](https://automapper.org/)              | Mapping one object to another                                               |
| [![EntityFrameworkCore](https://shields.io/badge/EntityFrameworkCore-7.0.0-004880?logo=Nuget&style=flat)](https://learn.microsoft.com/es-es/ef/core/)              | Object-database mapper                                              |
| [![EntityFrameworkCore](https://shields.io/badge/DynamicLinq-7.2.23-004880?logo=Nuget&style=flat)](https://learn.microsoft.com/es-es/ef/core/)              | Dynamic Linq extensions for Microsoft.EntityFrameworkCore which adds Async support                                              |
| [![Serilog](https://shields.io/badge/Serilog-2.12.0-004880?logo=Nuget&style=flat)](https://serilog.net/)             | Simple .NET logging with fully-structured events support                                              |
| [![xUnit](https://shields.io/badge/xUnit-2.4.2-004880?logo=Nuget&style=flat)](https://github.com/xunit/xunit)             | developer testing framework                                              |
|[![Swashbuckle](https://shields.io/badge/Swashbuckle-6.4.0-004880?logo=Nuget&style=flat)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)              | Swagger tools for documenting APIs built on ASP.NET Coreframework                                              |
| [![Moq](https://shields.io/badge/Moq-4.18.2-004880?logo=Nuget&style=flat)](https://github.com/moq/moq4)               | Moq is the most popular and friendly mocking framework for .NET                                              |

# 💿 Auto ORM

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