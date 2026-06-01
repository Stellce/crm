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
- Password reset request tokens

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

    ```teminal
    dotnet restore
    ```

4. Run migrations:

    ```terminal
    dotnet ef database update
    ```

5. Start project

    ```terminal
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

2. Configure password reset

    ```json
    "PasswordReset": {
      "FrontendBaseUrl": "http://localhost:5173",
      "TokenLifetime": "00:30:00"
    }
    ```

### Logging

The project uses Serilog for structured logging.

Implemented:

- request logging middleware for HTTP requests;
- centralized exception logging;
- business-level logs for customer and order creation;
- user id enrichment from JWT claims;
- console and rolling file sinks;
- environment-based configuration via appsettings.

## Local development

After starting the API, the service is available locally at:

- API: `https://localhost:7273`
- Swagger UI: `https://localhost:7273/swagger`
- OpenAPI JSON: `https://localhost:7273/openapi/v1.json`
- Hangfire Dashboard: `https://localhost:7273/hangfire`
- Dev mailbox: `http://localhost:8025`

> Exact ports may differ depending on `Api/Properties/launchSettings.json`.

### Background jobs

The application uses Hangfire for recurring background processing.

Currently configured jobs:

- `auth-token-cleanup` — removes expired refresh tokens and old password reset tokens.

Hangfire Dashboard is enabled only in the Development environment.
