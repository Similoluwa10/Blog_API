# Blog API

A RESTful API for managing blog posts and content built with ASP.NET Core.

## Prerequisites

- .NET 9.0 SDK
- SQL Server (Docker container for macOS)

## Getting Started

### Windows Setup
1. Install .NET 9.0 SDK
2. Install SQL Server or SQL Server Express
3. Update the connection string in `appsettings.json` if needed
4. Run `dotnet ef database update` to create the database
5. Run `dotnet run --project Blog_API/Blog_API.csproj`

### macOS Setup
1. Install .NET 9.0 SDK from Microsoft
2. Run SQL Server in Docker:
   ```
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=*Similoluwa10#" \
      -p 1433:1433 --name sql_server --hostname sql_server \
      -d mcr.microsoft.com/mssql/server:2022-latest
   ```
3. Trust the HTTPS development certificate:
   ```
   dotnet dev-certs https --trust
   ```
4. Run `dotnet ef database update` to create the database
5. Run `dotnet run --project Blog_API/Blog_API.csproj --launch-profile https`

## API Endpoints

- GET /api/posts - Get all blog posts
- GET /api/posts/{id} - Get a specific post
- POST /api/posts - Create a new post
- PUT /api/posts/{id} - Update a post
- DELETE /api/posts/{id} - Delete a post

## Technologies

- ASP.NET Core 9.0
- Entity Framework Core
- SQL Server