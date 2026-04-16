# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/CleanArchitecture.WebAPI/CleanArchitecture.WebAPI.csproj", "src/CleanArchitecture.WebAPI/"]
COPY ["src/CleanArchitecture.Application/CleanArchitecture.Application.csproj", "src/CleanArchitecture.Application/"]
COPY ["src/CleanArchitecture.Contracts/CleanArchitecture.Contracts.csproj", "src/CleanArchitecture.Contracts/"]
COPY ["src/CleanArchitecture.SharedKernel/CleanArchitecture.SharedKernel.csproj", "src/CleanArchitecture.SharedKernel/"]
COPY ["src/CleanArchitecture.Domain/CleanArchitecture.Domain.csproj", "src/CleanArchitecture.Domain/"]
COPY ["src/CleanArchitecture.Infrastructure/CleanArchitecture.Infrastructure.csproj", "src/CleanArchitecture.Infrastructure/"]
COPY ["src/CleanArchitecture.WebAPI.Contracts/CleanArchitecture.WebAPI.Contracts.csproj", "src/CleanArchitecture.WebAPI.Contracts/"]
RUN dotnet restore "./src/CleanArchitecture.WebAPI/CleanArchitecture.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/CleanArchitecture.WebAPI"
RUN dotnet build "./CleanArchitecture.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CleanArchitecture.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanArchitecture.WebAPI.dll"]