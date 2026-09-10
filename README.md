# WareStockApi

The project was generated using the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) version 10.8.0.

## Build

Run `dotnet build` to build the solution.

## Run

To run the application:

```bash
dotnet run --project .\src\AppHost
```

The Aspire dashboard will open automatically, showing the application URLs and logs.

## Database

By default, `AppHost` automatically provisions a SQL Server **container** on startup (requires Docker running) — no setup needed.

If you already have your own SQL Server (local install, existing container, or Azure SQL) and want to use that instead, point `AppHost` at it via .NET user-secrets (per-developer, **not** committed to git):

```bash
cd src/AppHost
dotnet user-secrets set "ConnectionStrings:WareStockApiDb" "Server=localhost;Database=WareStockApiDb;User Id=sa;Password=<your-password>;TrustServerCertificate=True"
```

Then change `src/AppHost/Program.cs` to reference the connection string instead of provisioning a container:

```csharp
// Instead of: builder.AddAzureSqlServer(...).RunAsContainer(...).AddDatabase(...)
var databaseServer = builder.AddConnectionString(Services.Database);
```

Each developer who wants to use their own SQL Server must run the `dotnet user-secrets set` command above with their own connection details — it is stored locally per machine, not shared via git.

## Code Styles & Formatting

The template includes [EditorConfig](https://editorconfig.org/) support to help maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. The **.editorconfig** file defines the coding styles applicable to this solution.

## Code Scaffolding

The template includes support to scaffold new commands and queries.

Start in the `.\src\Application\` folder.

Create a new command:

```
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

Create a new query:

```
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

If you encounter the error *"No templates or subcommands found matching: 'ca-usecase'."*, install the template and try again:

```bash
dotnet new install Clean.Architecture.Solution.Template::10.8.0
```

## Test

The solution contains unit, integration, and functional tests.

To run the tests:
```bash
dotnet test
```

## Help
To learn more about the template go to the [project website](https://cleanarchitecture.jasontaylor.dev). Here you can find additional guidance, request new features, report a bug, and discuss the template with other users.# ware-stock-api
