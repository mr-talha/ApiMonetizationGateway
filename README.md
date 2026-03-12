**API Monetization Gateway**
**System Architecture**

The system is designed as an API Gateway with monetization and rate-limiting features.

**Components**
**1. API Consumers**

External clients are calling your API endpoints using API keys.

**2. API Gateway (Core Service)**

Handles requests, applies business rules, and enforces limits.

**Middlewares**

RateLimitingMiddleware → Enforces per-second rate limits and monthly quotas via RateLimitingService.

ApiUsageLoggingMiddleware → Logs all API calls into the database for auditing & reporting.

**Services**

**CustomerService**

Retrieves customer details by API key.

Links customer → tier subscription.

**TierService**

Manages tier definitions (Free, Pro, etc.).

Handles quota & rate limit thresholds.

**UsageTrackingService**

Records raw API usage (ApiUsageLog).

Aggregates daily/weekly/monthly statistics.

**BillingService**

Calculates charges based on tier and usage.

Generates monthly invoices.

**RateLimitingService**

Implements the sliding window algorithm.

Uses Redis to track requests per second.

Updates monthly quota counters.

UsageTrackingService → Tracks usage and calculates monthly summaries.

CustomerService → Fetches customer info and tier details.

**3. Database (SQL via ApplicationDbContext)**

Customer

Tier

ApiUsageLog

MonthlyUsageSummary

**4. Redis**

Used for fast, in-memory rate limiting:

Sliding window counters.

Monthly usage counts (temporary, before persisting).

**5. Background Service**

MonthlySummaryService → Aggregates monthly usage logs, updates summaries, and calculates costs.

**High-Level Flow**

API Consumer sends a request with an API key.

RateLimitingMiddleware checks Redis for usage limits.

If allowed, request proceeds → ApiUsageLoggingMiddleware logs usage.

Response returned to client.

MonthlySummaryService processes logs & updates costs at month-end.
