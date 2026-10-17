# Repository structure — fixes (2026-10-14)

Cleanup pass over the solution layout and project identity. There are **no behaviour changes**
to the HTTP contract (PascalCase JSON / numeric enums / `409 {success,exception}` /
`400 {Success,Errors}` are untouched).

## What changed

### Git hygiene
- Stopped tracking `.idea/` (JetBrains user state) and added it to `.gitignore`.
- Deleted the untracked 547 MB legacy `packages/` folder (no `nuget.config` referenced it).

### Naming: no more `Qeust`
- Fixed the misspelled project identity everywhere — folder, `.csproj`, assembly name, root
  namespace, solution entry, docs and all `using`/`namespace` declarations:
  `IntellaQeust.BusinessLogic` → **`NexCart.Business`**.
- Aligned the data project so folder = file = assembly = namespace:
  `NexCart.Repository/` (with a mixed `NexCart.Repository.*` namespace) →
  **`NexCart.Data.NHibernate`**, matching the `.csproj`/solution name it already carried.
- Removed the now-unused residual project name `NexCart.Business`.

Every project now follows: *folder = .csproj name = assembly name = root namespace*.

### Build
- Added `Directory.Build.props` centralizing `TargetFramework`, `ImplicitUsings`, `Nullable`
  and `GenerateAssemblyInfo`; removed the duplicated lines from each `.csproj`.

### Dead files
- Removed the unreferenced legacy MVC static assets `NexCart.Web/Content`,
  `NexCart.Web/Scripts` and `NexCart.Web/fonts` (71 files). `_Layout.cshtml` only
  uses the CDN styles/scripts and the hashed Angular bundle (`@Spa.*`), so nothing referenced them.
  `Views/` is kept — `Home/Home.cshtml` **is** the SPA host.

### Tests
- Added `NexCart.Tests` (xUnit) with `PasswordHasher` unit tests, registered in the solution.

### Code
- Removed a duplicated `using NexCart.Data.NHibernate.Repositories;` in `Program.cs`.

### Docs
- Updated `OVERVIEW.md`, `README.md` and the per-project docs (`NexCart.Web.md`,
  `NexCart.Business.md`, `NexCart.Data.NHibernate.md`) to reflect the new names
  and the current auth / security / database behaviour.

## Deliberately not changed

- **The Angular app stays nested at `NexCart.Web/my-app/`.** Moving it would break the
  `SpaAssets` / static-file paths and the CLI output location for no functional gain.
- **Build output under `my-app/bundles/browser/` stays committed** — the ASP.NET host serves it
  directly, so the API runs without a Node/npm toolchain.
- **Migration folders keep the `<timestamp>-<gitUser>` convention** with
  `sqlmigration.cs` / `up.sql` / `down.sql` (project convention).
- **`Nullable` remains disabled** — enabling it repo-wide would surface many warnings and is a
  separate task.
- **Central package management was not introduced** — only two projects share a package
  (`Microsoft.Data.SqlClient`), so the extra indirection isn't justified.
