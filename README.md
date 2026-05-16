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

6. Development seed

When the API runs in `Development`, it creates a bootstrap SuperAdmin user if it does not already exist:

```json
{
  "Email": "superadmin@crm.local",
  "Password": "SuperAdmin123!"
}
```

Use this account to log in and create the first admin/manager users. The seed values can be changed in `appsettings.Development.json` under `Seed:SuperAdmin`.

## Options

1. Configure Token Lifetime

```json
"Auth": {
  "AccessTokenLifetime": "00:10:00",
  "RefreshTokenLifetime": "00:30:00",
  "TokenClockSkew": "00:00:30"
}
```