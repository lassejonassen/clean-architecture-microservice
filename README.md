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