# 🍩 ApiGen

[![dotnet](https://img.shields.io/badge/dotnet-10.0.x-purple)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![Release](https://img.shields.io/badge/release-1.0.0-f4d03f.svg)](https://github.com/mkenney/software-guides/blob/master/STABILITY-BADGES.md#beta)
[![License: LGPL v3](https://img.shields.io/badge/license-LGPL_v3-blue.svg)](https://www.gnu.org/licenses/lgpl-3.0)
[![Security Rating](https://sonarqube.cloudappi.net/api/project_badges/measure?project=apigen-dotnet&metric=security_rating&token=sqb_c7abd9969334a4b41d2a566cc397d0aa4dea5ddc)](https://sonarqube.cloudappi.net/dashboard?id=apitools-apigen-dotnet)
[![Maintainability Rating](https://sonarqube.cloudappi.net/api/project_badges/measure?project=apigen-dotnet&metric=sqale_rating&token=sqb_c7abd9969334a4b41d2a566cc397d0aa4dea5ddc)](https://sonarqube.cloudappi.net/dashboard?id=apitools-apigen-dotnet)
[![Coverage](https://sonarqube.cloudappi.net/api/project_badges/measure?project=apigen-dotnet&metric=coverage&token=sqb_c7abd9969334a4b41d2a566cc397d0aa4dea5ddc)](https://sonarqube.cloudappi.net/dashboard?id=apitools-apigen-dotnet)
[![Lines of Code](https://sonarqube.cloudappi.net/api/project_badges/measure?project=apigen-dotnet&metric=ncloc&token=sqb_c7abd9969334a4b41d2a566cc397d0aa4dea5ddc)](https://sonarqube.cloudappi.net/dashboard?id=apitools-apigen-dotnet)

> ASP.NET microservice archetype generator for .NET 10. Point it at an OpenAPI spec and get a ready-to-run hexagonal architecture solution.


## 📦 What it generates

Given an OpenAPI document with extended annotations, ApiGen produces a fully structured .NET solution:

| Layer | Namespace | Contents |
|---|---|---|
| Api | `{Project}.Api` | Controllers, Helpers, Middleware, Program.cs |
| Domain | `{Project}.Domain` | Models (DTOs), Services, Utils |
| Infrastructure | `{Project}.Infrastructure` | Entities, Repositories, DbContext |
| Tests | `{Project}.Domain.Tests` | Controller tests, Service tests |

Inspired by [apigen.springboot](https://github.com/apiaddicts/apigen.springboot), adapted for the .NET ecosystem by [CloudAPPi Services](https://cloudappi.net).

[![try api](https://img.shields.io/badge/try_api-purple.svg?style=for-the-badge&logo=openapiinitiative&logoColor=white)](https://api-gateway.apiquality.io/api-apigen-dotnet/v1/swagger)
[![sample template](https://img.shields.io/badge/sample_template-orange.svg?style=for-the-badge&logo=dotnet&logoColor=white)](https://gitlab.com/cloudappi/templates/back-templates/dotnet-template)

---

## 🚀 Quick start

### `dotnet cli`
```bash
dotnet run --project ./src/Api/Api.csproj
```

### `docker`
```bash
docker build -t apigen .
docker run -d -p 8080:8080 --name apigen apigen
```

### `docker-compose`
```bash
docker-compose up --build -d
```

---

## ▶️ Usage

### REST API

Once running, the Swagger UI is available at `/swagger`. You can also trigger generation directly via `curl` — see the example specs under `src/Generator/Examples/`.

```bash
curl -X 'POST' \
  'http://localhost:8080/generator/file' \
  -H 'accept: */*' \
  -H 'Content-Type: multipart/form-data' \
  -F 'file=@<openapi-file>'
```

### CLI

Download the build or compile the `Command` project and run:

```bash
apigen <openapi-path>
```

---

## ⚙️ Configuration

### 🗄️ data-driver

Control the database provider via the `data-driver` field in the `x-apigen-project` OpenAPI extension:

| Value | NuGet package | `DbContext` registration |
|---|---|---|
| _(not set)_ | `Microsoft.EntityFrameworkCore.InMemory` | `UseInMemoryDatabase(...)` |
| `postgresql` | `Npgsql.EntityFrameworkCore.PostgreSQL` | `UseNpgsql(...)` |
| `mysql` | `Pomelo.EntityFrameworkCore.MySql` | `UseMySql(..., ServerVersion.AutoDetect(...))` |

```yaml
x-apigen-project:
  name: My Project
  description: ...
  version: 1.0.0
  data-driver: postgresql   # postgresql | mysql | (omit for in-memory)
```

The generated project includes **only** the required provider package. The connection string is read at runtime from the `DATABASE_URL` environment variable.

### 🏷️ OpenAPI extensions

ApiGen relies on custom extensions to enrich the generated code:

| Extension | Scope | Purpose |
|---|---|---|
| `x-apigen-project` | Document | Project metadata and database driver |
| `x-apigen-models` | Components | Entity definitions with relational persistence mapping |
| `x-apigen-mapping` | Schema | DTO → Entity AutoMapper profile |
| `x-apigen-binding` | Path | Binds an endpoint group to a service |

---

## 💿 ORM scaffolding from an existing database

If you prefer to scaffold entities from an existing database rather than defining them manually, use the EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
dotnet ef dbcontext scaffold <connection-string> <driver> -o Infrastructure/Entities
```

_Example with PostgreSQL:_
```bash
dotnet ef dbcontext scaffold \
  "Host=<url>:<port>;Database=<db>;Username=<user>;Password=<pass>" \
  Npgsql.EntityFrameworkCore.PostgreSQL \
  -o Infrastructure/Entities
```

---

<img src="imgs/logo-apiquality.png" height="75">
