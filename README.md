
# 🍩 Apigen [![Release](https://img.shields.io/badge/dotnet-7.0.10-purple)](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) [![stability-alpha](https://img.shields.io/badge/version-alpha_0.0.6-f4d03f.svg)](https://github.com/mkenney/software-guides/blob/master/STABILITY-BADGES.md#alpha) ![Swagger](https://img.shields.io/badge/-openapi-%23Clojure?style=flat&logo=swagger&logoColor=white)  [![License: LGPL v3](https://img.shields.io/badge/license-LGPL_v3-blue.svg)](https://www.gnu.org/licenses/lgpl-3.0) 

`Asp.Net microservice` archetype generator in dotnet with hexagonal architecture based on an openapi document with extended annotations. This project is a wrapper of the [java apigen](https://github.com/apiaddicts/apigen.springboot) with springboot but using dotnet and adapting some concepts due to the paradigm difference. The project is currently being developed by the [CloudAPPi Services](https://cloudappi.net).

### This repository is intended for :octocat: **community** use, it can be modified and adapted without commercial use. If you need a version, support or help for your **enterprise** or project, please contact us 📧 devrel@apiaddicts.org

[![Twitter](https://img.shields.io/badge/Twitter-%23000000.svg?style=for-the-badge&logo=x&logoColor=white)](https://twitter.com/APIAddicts) 
[![Discord](https://img.shields.io/badge/Discord-%235865F2.svg?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/ZdbGqMBYy8)
[![LinkedIn](https://img.shields.io/badge/linkedin-%230077B5.svg?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/company/apiaddicts/)
[![Facebook](https://img.shields.io/badge/Facebook-%231877F2.svg?style=for-the-badge&logo=Facebook&logoColor=white)](https://www.facebook.com/apiaddicts)
[![YouTube](https://img.shields.io/badge/YouTube-%23FF0000.svg?style=for-the-badge&logo=YouTube&logoColor=white)](https://www.youtube.com/@APIAddictslmaoo)

# 🙌 Join the **Apigen** Adopters list 
📢 If Apigen is part of your organization's toolkit, we kindly encourage you to include your company's name in our Adopters list. 🙏 This not only significantly boosts the project's visibility and reputation but also represents a small yet impactful way to give back to the project.

| Organization  | Description of Use / Referenc |
|---|---|
|  [CloudAppi](https://cloudappi.net/)  | Apification and generation of microservices |
| [Apiquality](https://apiquality.io/)  | Generation of microservices  |

# 👩🏽‍💻  Contribute to ApiAddicts 

We're an inclusive and open community, welcoming you to join our effort to enhance ApiAddicts, and we're excited to prioritize tasks based on community input, inviting you to review and collaborate through our GitHub issue tracker.

Feel free to drop by and greet us on our GitHub discussion or Discord chat. You can also show your support by giving us some GitHub stars ⭐️, or by following us on Twitter, LinkedIn, and subscribing to our YouTube channel! 🚀

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/apiaddicts)


# 📑 Getting started 

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

# 💚 Status


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

# 🎲 Samples

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

## 💛 Sponsors
<p align="center">
	<a href="https://apiaddicts.org/">
    	<img src="https://apiaddicts.cloudappi.net/web/image/4248/LOGOCloudappi2020Versiones-01.png" alt="cloudappi" width="150"/>
        <img src="https://apiquality.io/wp-content/uploads/2022/09/cropped-logo-apiquality-principal-1-170x70.png" height = "75">
        <img src="https://apiaddicts-web.s3.eu-west-1.amazonaws.com/wp-content/uploads/2022/03/17155736/cropped-APIAddicts-logotipo_rojo.png" height = "75">
	</a>
</p>
