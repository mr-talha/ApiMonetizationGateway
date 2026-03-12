# API Monetization Gateway

API Gateway with monetization and rate-limiting features (SQL Server + Redis).

## Docker (build + run)

### Prerequisites

- Docker Desktop (Windows/macOS) or Docker Engine (Linux)
- Docker Compose v2 (`docker compose ...`)

### Option A: Run the full stack with Docker Compose (recommended)

From the repo root:

```bash
cd ApiMonetizationGateway

# Create a .env file (required for SQL Server)
# Windows PowerShell:
Set-Content -Path .env -Value "MSSQL_SA_PASSWORD=Your_strong_password123!"

docker compose up --build
```

Then open:

- Swagger UI: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`
- Redis Commander (optional): `http://localhost:8081`

Notes:

- On the first run, SQL Server may take a bit to start. If the API container exits due to migrations failing, run `docker compose up` again after ~10–20 seconds.
- The API listens on container port `8080` (mapped to `localhost:8080`).

### Option B: Build and run only the API container

Build:

```bash
docker build -f ApiMonetizationGateway/Dockerfile -t api-monetization-gateway:latest ApiMonetizationGateway
```

Run (you must provide reachable Redis + SQL Server connection strings):

```bash
docker run --rm -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e "ConnectionStrings__Redis=host.docker.internal:6379,abortConnect=false" \
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal,1433;Database=ApiGatewayDb;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=true;" \
  api-monetization-gateway:latest
```

## System architecture

The system is designed as an API Gateway with monetization and rate-limiting features.

### Components

#### 1) API Consumers

External clients call API endpoints using API keys.

#### 2) API Gateway (Core Service)

Handles requests, applies business rules, and enforces limits.

**Middlewares**

- `RateLimitingMiddleware` → Enforces per-second rate limits and monthly quotas via `RateLimitingService`.
- `ApiUsageLoggingMiddleware` → Logs all API calls into the database for auditing & reporting.

**Services**

- `CustomerService` → Retrieves customer details by API key; links customer → tier subscription.
- `TierService` → Manages tier definitions (Free, Pro, etc.) and thresholds.
- `UsageTrackingService` → Records raw API usage (`ApiUsageLog`) and aggregates stats.
- `BillingService` → Calculates charges based on tier and usage; generates monthly invoices.
- `RateLimitingService` → Implements a sliding window algorithm; uses Redis for counters and monthly quotas.

#### 3) Database (SQL via `ApplicationDbContext`)

- `Customer`
- `Tier`
- `ApiUsageLog`
- `MonthlyUsageSummary`

#### 4) Redis

Used for fast, in-memory rate limiting:

- Sliding window counters
- Monthly usage counts (temporary, before persisting)

#### 5) Background Service

`MonthlySummaryService` → Aggregates monthly usage logs, updates summaries, and calculates costs.

### High-level flow

1. API Consumer sends a request with an API key.
2. `RateLimitingMiddleware` checks Redis for usage limits.
3. If allowed, the request proceeds → `ApiUsageLoggingMiddleware` logs usage.
4. Response is returned to the client.
5. `MonthlySummaryService` processes logs and updates costs at month-end.
