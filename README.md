# CQRS API Boilerplate with Identity

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/DanielGregatto/cqrs-api-master/pulls)

A production-ready **CQRS (Command Query Responsibility Segregation)** API boilerplate built with **ASP.NET Core 8.0**, featuring a complete Identity layer, JWT authentication, social logins, and clean architecture. Perfect starting point for building scalable, maintainable APIs.

## Table of Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)
- [Using as a Boilerplate](#using-as-a-boilerplate)
- [Contributing](#contributing)
- [License](#license)

## Features

### Authentication & Authorization
- **JWT Token Authentication** with refresh token support
- **Social Login Integration** (Google, Facebook, Microsoft)
- **Email Confirmation** workflow
- **Password Reset** functionality
- **Role-Based Access Control** via ASP.NET Core Identity

### Architecture & Patterns
- **CQRS Pattern** - Separate commands and queries with MediatR
- **Clean Architecture** - Clear separation of concerns across layers
- **Domain Events** - Automatic entity lifecycle events (Insert, Update, Delete)
- **UoW Pattern** with Unit of Work that triggers domain events
- **Result Pattern** - Consistent error handling across the application

### Security & Resilience
- **Rate Limiting** - Prevent API abuse with configurable limits
- **Cloudflare Turnstile** CAPTCHA validation
- **Polly Resilience Policies** - Retry, circuit breaker, timeout
- **CORS Configuration** - Environment-specific origin whitelisting
- **Data Protection** with Azure Blob Storage for distributed scenarios
- **Global Exception Handling** with correlation ID tracking

### Development Features
- **FluentValidation** - Declarative validation for all commands
- **AutoMapper** - Object-to-object mapping
- **Swagger/OpenAPI** documentation with JWT support
- **Localization** - Multi-language support (en-US, pt-BR)
- **Structured Logging** with Application Insights integration
- **Docker Support** - Ready for containerized deployment
- **Unit Tests** - xUnit, FluentAssertions, Moq

## Technology Stack

| Layer | Technologies |
|-------|-------------|
| **Framework** | ASP.NET Core 8.0 |
| **CQRS/Mediator** | MediatR 12.0 |
| **ORM** | Entity Framework Core 8.0 |
| **Database** | SQL Server |
| **Validation** | FluentValidation 12.1 |
| **Mapping** | AutoMapper |
| **Authentication** | ASP.NET Core Identity, JWT Bearer |
| **Resilience** | Polly (via Microsoft.Extensions.Http.Polly) |
| **Testing** | xUnit, FluentAssertions, Moq |
| **Documentation** | Swashbuckle (Swagger) |
| **Containerization** | Docker |

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                     │
│                         (UI.API)                            │
│              Controllers, Middleware, Configs               │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                     Application Layer                       │
│                        (Services)                           │
│         Commands, Queries, Handlers, Validators             │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                       Domain Layer                           │
│                  (Domain + Domain.Core)                      │
│          Entities, DTOs, Interfaces, Events, Enums           │
└─────────────────────────┬───────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────┐
│                   Infrastructure Layer                       │
│              (Data, Identity, IoC, Util)                     │
│           DbContext, Authentication, DI, Utilities           │
└─────────────────────────────────────────────────────────────┘
```

### CQRS Flow

**Commands** (Write Operations):
```
Controller → MediatR → CommandHandler → Validation → Business Logic → UnitOfWork → Database
```

**Queries** (Read Operations):
```
Controller → MediatR → QueryHandler → EF Core (Read-Only) → DTOs → Response
```

All operations return a `Result<T>` object for consistent error handling.

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or Full)
- (Optional) [Docker Desktop](https://www.docker.com/products/docker-desktop) for containerized deployment

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/DanielGregatto/cqrs-api-master.git
   cd cqrs-api-boilerplate
   ```

2. **Set up configuration files**

   Copy the example configuration file and customize it:
   ```bash
   cd src/UI.API
   copy appsettings.Development.json.example appsettings.Development.json
   ```

   Then edit `appsettings.Development.json` with your settings:
   - Update database connection string
   - Add your email provider credentials (or use Mailtrap for testing)
   - Configure OAuth providers (Google, Facebook) if needed
   - Generate a secure JWT secret key (minimum 32 characters)

   **Note:** `appsettings.Development.json` and `appsettings.Production.json` are in `.gitignore` to keep your secrets safe.

3. **Run database migrations**
   ```bash
   cd src/UI.API
   dotnet ef database update --project ../Data
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access Swagger UI**

   Navigate to: `https://localhost:5001/swagger`

### Docker Deployment

```bash
cd platform/docker
docker-compose up --build
```

## Configuration

### JWT Settings

Configure JWT authentication in `appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "your-api-name",
    "Secret": "your-secret-key-min-32-characters",
    "MinutesValid": "60",
    "Audience": "https://your-frontend-url.com/",
    "RedirectUriExternalLogin": "https://your-frontend-url.com/auth/external-login",
    "RedirectUriEmailConfirm": "https://your-frontend-url.com/auth/email-confirmed",
    "RedirectUriResetPassword": "https://your-frontend-url.com/auth/reset-password"
  }
}
```

### Email Configuration

Configure SMTP settings for sending emails:

```json
{
  "EmailConfig": {
    "Host": "smtp.your-provider.com",
    "SmtpPort": 587,
    "Email": "noreply@yourdomain.com",
    "Password": "your-email-password",
    "Name": "Your API Name",
    "UseSSL": true,
    "MainUrl": "https://your-frontend-url.com/"
  }
}
```

### Social Logins

Configure OAuth providers in `appsettings.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "Facebook": {
      "AppId": "your-facebook-app-id",
      "AppSecret": "your-facebook-app-secret"
    }
  }
}
```

### Rate Limiting

Configure API rate limits:

```json
{
  "RateLimit": {
    "DefaultTokenLimit": 20,
    "DefaultTokensPerPeriod": 20,
    "DefaultReplenishmentPeriodSeconds": 20,
    "BypassToken": "your-bypass-token-for-testing",
    "BypassHeaderName": "x-test-bypass"
  }
}
```

### CORS

Configure allowed origins per environment:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://your-frontend-url.com"
    ]
  }
}
```

## Project Structure

```
src/
├── UI.API/                      # Presentation Layer
│   ├── Controllers/             # API Controllers
│   ├── Middleware/              # Custom middleware (Correlation ID, Exception Handler)
│   ├── Configurations/          # Startup configurations (Swagger, AutoMapper, Polly, etc.)
│   └── Program.cs               # Application entry point
│
├── Services/                    # Application Layer
│   ├── Features/                # Feature-based organization
│   │   ├── Auth/                # Authentication features
│   │   │   ├── Commands/        # Login, Register, ForgotPassword, etc.
│   │   │   └── Queries/         # (if any)
│   │   ├── Account/             # User account management
│   │   │   ├── Commands/        # UpdateProfile, ChangePassword, etc.
│   │   │   └── Queries/         # GetProfile, etc.
│   │   └── Status/              # Example domain feature
│   │       ├── Commands/        # Create, Update, Delete
│   │       └── Queries/         # GetAll, GetById, Search
│   ├── Core/                    # Base handlers
│   │   ├── BaseCommandHandler.cs
│   │   └── BaseQueryHandler.cs
│   └── Infrastructure/          # MediatorHandler implementation
│
├── Domain/                      # Domain Layer
│   ├── Entities/                # Domain entities (Status, LogError, etc.)
│   ├── DTO/                     # Data Transfer Objects
│   │   ├── Responses/           # Response DTOs
│   │   └── Infrastructure/      # CQRS, API infrastructure DTOs
│   ├── Enums/                   # Domain enumerations
│   ├── Interfaces/              # Domain interfaces
│   └── Resources/               # Localization resources
│
├── Domain.Core/                 # Core Domain (Framework-agnostic)
│   ├── EntityBase.cs            # Base entity class
│   └── Events/                  # Domain event interfaces and implementations
│
├── Data/                        # Infrastructure - Data Access
│   ├── Context/                 # DbContext
│   ├── Mappings/                # EF Core entity configurations
│   ├── Migrations/              # Database migrations
│   └── UnitOfWork/              # Unit of Work implementation
│
├── Identity/                    # Infrastructure - Identity
│   ├── Model/                   # ApplicationUser, JWT config
│   ├── Services/                # JWT token generation, email service
│   └── IdentityConfiguration.cs # Identity setup
│
├── IoC/                         # Dependency Injection
│   └── DIBootstrapper.cs        # DI container configuration
│
├── Util/                        # Cross-cutting Utilities
│   └── Handlers/                # Image, Text, Validation utilities
│
└── Tests/                       # Test Projects
    └── Unitary/
        └── Domain.Unit.Tests/   # Unit tests (xUnit)
```

## API Endpoints

### Authentication (`/api/auth`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/v1/auth/login` | Login with email/password |
| POST | `/v1/auth/register` | Register new user |
| POST | `/v1/auth/forgot-password` | Request password reset |
| POST | `/v1/auth/reset-password` | Reset password with token |
| POST | `/v1/auth/refresh` | Refresh access token |
| POST | `/v1/auth/start-refresh` | Start refresh token (authenticated) |
| GET | `/v1/auth/email-confirmed` | Confirm email address |
| GET | `/v1/auth/google-login` | Initiate Google OAuth |
| GET | `/v1/auth/facebook-login` | Initiate Facebook OAuth |
| GET | `/v1/auth/external-login-callback` | OAuth callback handler |

### Account (`/api/account`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/v1/account/profile` | Get user profile |
| POST | `/v1/account/update-personal-info` | Update personal information |
| POST | `/v1/account/update-address` | Update address |
| POST | `/v1/account/update-password` | Change password |

### Status (Example CRUD) (`/api/status`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/status` | Get all status records |
| GET | `/api/status/{id}` | Get status by ID |
| GET | `/api/status/ativo` | Get active status ID |
| GET | `/api/status/inativo` | Get inactive status ID |
| GET | `/api/status/search` | Paginated search |
| POST | `/api/status` | Create new status |
| PUT | `/api/status/{id}` | Update status |
| DELETE | `/api/status/{id}` | Delete status (soft delete) |

For complete API documentation, run the project and visit `/swagger`.

## Testing

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Project

```bash
dotnet test src/Tests/Unitary/Domain.Unit.Tests/Unit.Tests.csproj
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~CreateStatusCommandHandlerTests"
```

### Run with Detailed Output

```bash
dotnet test --verbosity normal
```

## Using as a Boilerplate

### 1. Clone and Rename

```bash
git clone https://github.com/DanielGregatto/cqrs-api-master.git my-new-api
cd my-new-api
rm -rf .git
git init
```

### 2. Update Namespaces

Search and replace across the solution:
- `repobasecsf` → `YourProjectName`
- Update assembly names in `.csproj` files

### 3. Configure Your Domain

Replace the example `Status` entity with your own domain entities:

1. Create entity in `Domain/Entities/`
2. Create entity mapping in `Data/Mappings/`
3. Add DbSet to `AppDbContext`
4. Create migration: `dotnet ef migrations add InitialCreate`

### 4. Add Your Features

Follow the existing pattern in `Services/Features/`:

```
YourFeature/
├── Commands/
│   └── CreateYourEntity/
│       ├── CreateYourEntityCommand.cs
│       ├── CreateYourEntityCommandHandler.cs
│       └── CreateYourEntityCommandValidator.cs
└── Queries/
    └── GetYourEntity/
        ├── GetYourEntityQuery.cs
        └── GetYourEntityQueryHandler.cs
```

### 5. Create Controller

Add controller in `UI.API/Controllers/`:

```csharp
[Route("api/[controller]")]
public class YourEntityController : CoreController
{
    private readonly IMediatorHandler _mediator;

    public YourEntityController(IMediatorHandler mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateYourEntityCommand command)
    {
        var result = await _mediator.SendCommand(command);
        return Response(result);
    }
}
```

### 6. Update Configuration

- Update `appsettings.json` with your configurations
- Set up your database connection string
- Configure email provider
- Set up OAuth providers (optional)

## Key Patterns & Conventions

### Result Pattern

All handlers return `Result<T>` for consistent error handling:

```csharp
// Success
return Result<StatusDto>.Success(statusDto);

// Validation failure
return Result<StatusDto>.ValidationFailure(errors);

// Not found
return Result<StatusDto>.NotFound("Entity not found");

// Unauthorized
return Result<StatusDto>.Unauthorized("Authentication required");
```

### Domain Events

Entities can raise domain events automatically:

```csharp
// Events are automatically raised by AppDbContext on SaveChangesAsync
// Listen to events by implementing INotificationHandler<EntityInsertedEvent<Status>>
```

### Validation

Use FluentValidation for all commands:

```csharp
public class CreateStatusCommandValidator : AbstractValidator<CreateStatusCommand>
{
    public CreateStatusCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
    }
}
```

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure:
- Code follows existing patterns and conventions
- All tests pass
- New features include unit tests
- Update documentation as needed

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- CQRS pattern implemented with [MediatR](https://github.com/jbogard/MediatR)
- Validation with [FluentValidation](https://fluentvalidation.net/)
- Resilience with [Polly](https://github.com/App-vNext/Polly)

---

Made with ❤️ for the .NET community. If this helped you, please give it a ⭐!
