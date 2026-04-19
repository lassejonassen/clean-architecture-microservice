# Clean Architecture Microservice Template

[![.NET CI and Docker Validation](https://github.com/lassejonassen/clean-architecture-microservice/actions/workflows/build.yml/badge.svg)](https://github.com/lassejonassen/clean-architecture-microservice/actions/workflows/build.yml)

This repository contains a .NET 8.0 solution and a Docker Compose configuration. The CI/CD pipeline is managed via GitHub Actions to ensure code quality and container orchestration stability.

## 🚀 CI/CD Pipeline

We use a dual-stage validation process:

### 1. Automatic Build & Test (Continuous Integration)
**Trigger:** Every `push` and `pull_request` to the `main` branch.
- **Native .NET Build:** Restores dependencies and builds the solution using the `.sln` file.
- **Unit Testing:** Runs all tests within the solution to ensure no regressions.

### 2. Manual Docker Validation
**Trigger:** Manual execution via `workflow_dispatch`.
- **Dependency:** Only runs if the `.NET CI` job passes.
- **Docker Compose:** Builds the images from the local Dockerfiles and spins up the services.
- **Health Check:** Verifies that all containers start successfully.

---

## 🛠 Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Local Build
To build the solution locally:
```bash
dotnet restore YourSolutionName.sln
dotnet build YourSolutionName.sln --configuration Release
```

---

## TODO:

### Health Checks

### OpenTelemetry

### RabbitMQ Dead Letter Exchange (DLS)

### Docker Conpose Watch Mode

### Centralized Library

### Automatic Database Migrations (The "First Run" Experience)

In a template, you want a new developer to just run docker-compose up and have everything work. Usually, the database starts empty. You should add a small helper in your Presentation Layer to apply migrations on startup.
Location: CleanArchitecture.WebAPI/Extensions/MigrationExtensions.cs

#### Environment Strategy
Development: YES. It’s perfect. It keeps the team in sync and ensures the database matches the code immediately.

Test/Staging: SOMETIMES. It’s okay if you are doing automated integration testing, but it’s better to use the same "Production" process here to catch deployment issues early.

Production: NO. Running migrations on startup in production is dangerous for three reasons:

- Race Conditions: If you scale your Web API to 3 instances, all 3 might try to run the same migration at the exact same second.
- Permissions: Your Web API’s database user should (ideally) not have "Schema Modification" (DDL) permissions. It should only have Read/Write (DML) permissions.
- Rollbacks: If a migration fails halfway through, your app is crashed, and you have no easy way to revert the database state.

##### Recommended Template Approach

Modify your MigrationExtensions to check the environment. This gives you the "Developer Experience" of automation without the "Production Risk."
```c-sharp
public static void ApplyMigrations(this IApplicationBuilder app, IWebHostEnvironment env)
{
    // Only run automatic migrations in Development
    if (!env.IsDevelopment())
    {
        return;
    }

    using IServiceScope scope = app.ApplicationServices.CreateScope();
    using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}
```

##### How to handle Production/Test
For higher environments, the industry standard is to generate SQL Scripts during your CI/CD pipeline and run them using a deployment tool (like Azure DevOps, GitHub Actions, or Octopus Deploy).

The workflow:

- Build Phase: Run dotnet ef migrations script --output bundle.sql.
- Deploy Phase: A specialized tool or a "Migration Runner" container executes that SQL against the production DB before the new Web API version starts.

### Architecture Tests

Since this is a template for other solutions, you want to prevent future developers from breaking Clean Architecture (e.g., preventing the Domain layer from accidentally referencing the Infrastructure layer).

### RabbitMQ Consumer Template

An integration event is useless without a consumer. Your template should include a Background Service project that:

- Uses the same RabbitMqConnection and Polly logic.
- Shows how to handle a message and create a Scoped database context within a background task.
- Integrates with the same OpenTelemetry trace so you can see the "hop" from the API to the Worker.

### API Versioning

If this is for long-term solutions, adding Asp.Versioning.Http now saves a massive headache later. It allows you to have /v1/products and /v2/products side-by-side.

### Authentication / Authorization
- OpenID Connect (OIDC) for authetnication and OAuth 2.0 for authorization with JWT.
- Keycloak
- AzureAd
- Other...

#### The Architectural Strategy: "Claims-Based"
In Clean Architecture, your Infrastructure layer handles the technical validation of the token, but your Application and Domain layers should only care about Claims (e.g., User.Id, User.Email, User.Role).

#### Keycloak vs. Azure AD: The Subtle Differences

While the protocol is the same, how they handle "Roles" differs:
- Keycloak: Roles are usually in a nested JSON object (realm_access.roles). You will need a Claims Transformation step in your template to "flatten" these into standard .NET ClaimTypes.Role
- Azure AD: Roles are usually in a top-level roles claim, but you must configure "App Roles" in the Azure Portal first.

#### Industry Standards for Authorization

Don't just use [Authorize(Roles = "Admin")]. That is considered "brittle." The industry standard is Policy-Based Authorization.

- Requirement: "Must be an owner of the resource."
- Policy: options.AddPolicy("CanEditProject", policy => policy.Requirements.Add(new ProjectOwnerRequirement()));
- Benefit: If the business logic changes (e.g., "Managers can now also edit projects"), you change it in one Policy file, not in 50 Controllers.

#### Securing RabbitMQ and Internal Communication

When your Web API sends a message to RabbitMQ, the "User Context" is usually lost.

- The Pro Move: In your IntegrationEvent base class, include a UserId property.
-Traceability: When the Consumer picks up the message, OpenTelemetry shows the trace, and your logs show which user triggered the background work via the UserId in the event.

#### Summary: What to add to the Template

- Identity Abstraction: Create an ICurrentUser interface in the Application layer.

```c-sharp
public interface ICurrentUser {
    string Id { get; }
    bool IsAuthenticated { get; }
}
```

- Infrastructure Implementation: Implement ICurrentUser using IHttpContextAccessor to pull the ID from the JWT claims.

- Swagger/OpenAPI UI: Configure Swagger to show the "Authorize" button (Padlock icon). This allows developers to paste a JWT from Keycloak/Azure AD directly into the browser to test the API.


# Docker

## Build
```bash
docker build -t <image-name> -f .\src\CleanArchitecture.WebAPI\Dockerfile .
```

## Tag
```bash
docker tag <image-name> yourusername/<image-name>:v1.0
```

## Push
```bash
docker push yourusername/<image-name>:v1.0
```
