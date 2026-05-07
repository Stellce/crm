# CRM API

## Stack
- ASP.NET Core Web API
- EF Core
- SQL Server
- JWT Authentication

## Features
- User registration/login
- Role-based authorization
- Orders management
- Global exception handling

## Setup

1. Create `appsettings.Development.json` or update `appsettings.json`

2. Set JWT key:

```json
"Jwt": {
  "Key": "replace with your own secure key",
  "Issuer": "crm-api",
  "Audience": "crm-client"
}
```

3. Install dependencies
```
dotnet restore
```

4. Run migrations:
```
dotnet ef database update
```

5. Start project

```
dotnet run
```
