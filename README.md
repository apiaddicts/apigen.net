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

Download a prebuilt binary (see [Standalone builds](#-standalone-builds)) or compile the `Command` project, then run:

```bash
apigen <openapi-path> -o <output-folder>
```

- Positional argument: the input OpenAPI (`.yml` / `.yaml`).
- `-o` / `--outpath`: **output folder** (default `./`). The CLI writes `<name>.zip` inside it.

> The output folder must exist — the CLI does not create it.

```bash
mkdir -p out
apigen in/api-hospital.yml -o out/   # -> out/api-hospital.zip
```

---

## ⬇️ Install (one-liner)

**macOS, Linux, WSL:**

```bash
curl -fsSL https://raw.githubusercontent.com/apiaddicts/apigen.net/main/install.sh | sh
```

**Windows PowerShell:**

```powershell
irm https://raw.githubusercontent.com/apiaddicts/apigen.net/main/install.ps1 | iex
```

The scripts detect your OS/arch, download the matching self-contained binary from
the latest [GitHub Release](https://github.com/apiaddicts/apigen.net/releases), install it into
`~/.apigen/bin` (Unix) / `%LOCALAPPDATA%\apigen\bin` (Windows) and add it to your PATH.

Optional overrides: `APIGEN_VERSION` (e.g. `1.0.2`) and `APIGEN_INSTALL_DIR`.

> The one-liners only **download** a binary — they require the release assets to
> already exist (see [Publishing a release](#-publishing-a-release-maintainers)).

### Docker

The Linux binary is `linux-x64` (**glibc**) and is published with
`InvariantGlobalization`, so it needs **no ICU** — only glibc + `ca-certificates`
(for the HTTPS calls that fetch NuGet versions). It runs on glibc images
(`debian`, `ubuntu`) but **not** on `alpine` (musl) or `scratch` (no libc). For
containers, prefer copying the binary directly:

```dockerfile
FROM debian:stable-slim
RUN apt-get update && apt-get install -y --no-install-recommends \
        ca-certificates && rm -rf /var/lib/apt/lists/*
COPY dist/apigen-dotnet-cli-linux-x64 /usr/local/bin/apigen
RUN chmod +x /usr/local/bin/apigen
ENTRYPOINT ["apigen"]
```

To use the `install.sh` one-liner inside a Dockerfile instead, the base image also
needs `curl`/`wget`:

```dockerfile
FROM debian:stable-slim
RUN apt-get update && apt-get install -y --no-install-recommends \
        curl ca-certificates && rm -rf /var/lib/apt/lists/*
RUN curl -fsSL https://raw.githubusercontent.com/apiaddicts/apigen.net/main/install.sh | sh
ENV PATH="/root/.apigen/bin:${PATH}"
ENTRYPOINT ["apigen"]
```

**Alpine** images need the musl binary (`LinuxMusl` profile → `linux-musl-x64`)
plus `libstdc++` (the .NET runtime links against it and Alpine doesn't ship it by
default) and `ca-certificates`:

```dockerfile
FROM alpine:3
RUN apk add --no-cache libstdc++ ca-certificates
COPY dist/apigen-dotnet-cli-linux-musl-x64 /usr/local/bin/apigen
RUN chmod +x /usr/local/bin/apigen
ENTRYPOINT ["apigen"]
```

> Without `libstdc++` the binary fails at startup with
> `Error relocating ...: _ZTVSt9exception: symbol not found`.

## 📦 Standalone builds

The CLI can be published as a **self-contained, single-file, trimmed** binary that
**embeds the .NET runtime** — the resulting executable runs on a machine **without
.NET installed**.

### Publish one platform

From the repo root:

```bash
dotnet publish src/Command/Command.csproj -p:PublishProfile=<Profile>
```

Profiles live in `src/Command/Properties/PublishProfiles/`:

| Profile | RID | Executable |
|---|---|---|
| `FolderProfile` | win-x64 | `apigen-dotnet-cli-win-x64.exe` |
| `Linux` | linux-x64 | `apigen-dotnet-cli-linux-x64` |
| `LinuxMusl` | linux-musl-x64 | `apigen-dotnet-cli-linux-musl-x64` |
| `MacArm64` | osx-arm64 | `apigen-dotnet-cli-osx-arm64` |
| `MacIntel` | osx-x64 | `apigen-dotnet-cli-osx-x64` |

> **glibc vs musl:** `Linux` (`linux-x64`) targets **glibc** distros (Debian,
> Ubuntu, RHEL…). `LinuxMusl` (`linux-musl-x64`) targets **musl** distros —
> **Alpine** and Alpine-based container images. They are not interchangeable: a
> glibc binary won't run on Alpine and vice versa.

### Build all platforms at once

`scripts/build-all.sh` compiles win-x64, linux-x64, osx-arm64 and osx-x64 and
copies the binaries into a local `dist/` folder:

```bash
./scripts/build-all.sh              # all four -> dist/
./scripts/build-all.sh linux-x64    # a single one
                                    # (win-x64 | linux-x64 | linux-musl-x64 | osx-arm64 | osx-x64)
```

Result in `dist/`:

```
apigen-dotnet-cli-win-x64.exe   (~29 MB)
apigen-dotnet-cli-linux-x64     (~18 MB)
apigen-dotnet-cli-osx-arm64     (~16 MB)
apigen-dotnet-cli-osx-x64       (~17 MB)
```

> `linux-musl-x64` (Alpine) is not part of `all`; build it on demand with
> `-p:PublishProfile=LinuxMusl`.

> Cross-compiling downloads each RID's runtime pack on first use (needs internet).

### Trimming notes

Trimming shrinks the binary (~45 MB → ~27 MB on Windows), but several dependencies
resolve types via reflection and would break if trimmed away. The profiles handle
this with:

- `JsonSerializerIsReflectionEnabledByDefault=true` — re-enables reflection-based
  JSON serialization (required by `FileUtils.ReadOpenApi`).
- `TrimmerRootAssembly` entries preserving the reflection-heavy assemblies:
  `Microsoft.OpenApi(.Readers)`, `SharpYaml`, `CodegenCS.Core`, `CommandLine`,
  `Newtonsoft.Json` and `NuGet.Protocol` (+ `NuGet.Common`, `NuGet.Configuration`,
  `NuGet.Versioning`, `NuGet.Packaging`, `NuGet.Frameworks`).

Without the `NuGet.*` roots the CLI fails when fetching the latest package versions
(`NuGet.Protocol.Model.RegistrationIndex: Unable to find a constructor`). The
`IL2026` / `IL2104` build warnings are static analysis over the preserved
assemblies; runtime behaviour is verified.

---

## 🏷️ Publishing a release (maintainers)

The install one-liners download binaries from **GitHub Releases**, so each version
must be published there first. The two halves are independent:

- **You (once per version):** build the binaries and upload them as release assets.
- **Everyone else (any time):** run the install one-liner, which downloads them.

Steps (manual, via the GitHub web UI — no extra tooling required):

1. Build all platforms into `dist/`:

   ```bash
   ./scripts/build-all.sh
   # optionally also the Alpine/musl binary:
   dotnet publish src/Command/Command.csproj -p:PublishProfile=LinuxMusl
   cp src/Command/bin/Release/net10.0/publish/linux-musl-x64/apigen-dotnet-cli-linux-musl-x64 dist/
   ```

2. Go to **`https://github.com/apiaddicts/apigen.net/releases`** → **Draft a new release**.
3. Create a tag matching the version, e.g. `1.0.2` (should match `<Version>` in
   `Directory.Build.props`).
4. **Drag the files from `dist/`** into the release assets area:
   - `apigen-dotnet-cli-win-x64.exe`
   - `apigen-dotnet-cli-linux-x64`
   - `apigen-dotnet-cli-osx-arm64`
   - `apigen-dotnet-cli-linux-musl-x64` (optional, Alpine)
5. **Publish release.**

> Asset filenames must match exactly — `install.sh` / `install.ps1` build the
> download URL from them (`apigen-dotnet-cli-<rid>`). The binaries are **not**
> committed to git (too large); they live only as release assets, which is why
> `dist/` is git-ignored.

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
