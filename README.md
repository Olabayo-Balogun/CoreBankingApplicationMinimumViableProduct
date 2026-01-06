# 💳 Banking Transaction System (.NET 8)

A robust backend system built with .NET 8 to manage banking operations including account creation, transactions, and external payment integration (using Paystack). Designed with scalability, security, and performance in mind.

---

## 🚀 Features

- 🏦 Account creation and management  
- 💰 Deposits, withdrawals, and transfers  
- 📄 Monthly transaction statements
- 🏭 Industry and industry field management    
- 🔐 Role-based access control (RBAC)  
- 🔄 CQRS pattern for command/query separation  
- 🔗 External payment integration (Paystack) 
- 📊 Structured logging and monitoring  
- 🧪 Unit testing with xUnit  
- 🔁 Idempotency support for critical operations  
- 🚦 Rate limiting to prevent abuse and ensure stability
- 💾 Response caching for improved performance    

---

## 🛠️ Tech Stack

| Technology        | Purpose                              |
|------------------|---------------------------------------|
| .NET 8           | Core framework                        |
| ASP.NET Core     | Web API backend                       |
| Entity Framework | ORM for MySQL database                |
| MySQL            | Relational data persistence           |
| Serilog          | Structured logging                    |
| xUnit            | Unit testing                          |
| Paystack         | External payment integration          |
| Scalar           | API documentation                     |

---

## 📦 Installation

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  
- MySQL Server  
- Visual Studio / VS Code  

### Setup

```bash
git clone https://github.com/Olabayo-Balogun/CoreBankingApplicationMinimumViableProduct.git
cd CoreBankingApplicationMinimumViableProduct
dotnet restore
dotnet build
dotnet run

### Database Connection String
"ConnectionStrings": {
    "DefaultConnection": "Server=db24923.public.databaseasp.net; Database=db24923; User Id=db24923; Password=Pp8!6#oKNy7%; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Min Pool Size=5; Max Pool Size=100; Connection Timeout=30"
  },

### API Documentation
## 📚 API Documentation

Interactive API docs are available via Scalar:

👉 [View Scalar Documentation](https://cbamvp.runasp.net/scalar/)

This includes endpoints for:

### 🔐 Authentication Operations

- [`POST /api/v1/Authentication/login`](https://cbamvp.runasp.net/scalar/#tag/authentication/post/api/v1/Authentication/login)  
- [`POST /api/v1/Authentication/register`](https://cbamvp.runasp.net/scalar/#tag/authentication/post/api/v1/Authentication/register)  
- [`POST /api/v1/Authentication/forgot-password`](https://cbamvp.runasp.net/scalar/#tag/authentication/post/api/v1/Authentication/forgot-password)  
- [`PUT /api/v1/Authentication/change-password`](https://cbamvp.runasp.net/scalar/#tag/authentication/put/api/v1/Authentication/change-password)  

### 🏦 Account Operations

- [`POST /api/v1/Accounts/account`](https://cbamvp.runasp.net/scalar/#tag/accounts/post/api/v1/Accounts/account)  
- [`GET /api/v1/Accounts/account`](https://cbamvp.runasp.net/scalar/#tag/accounts/get/api/v1/Accounts/account)  
- [`PUT /api/v1/Accounts/account`](https://cbamvp.runasp.net/scalar/#tag/accounts/put/api/v1/Accounts/account)  
- [`DELETE /api/v1/Accounts/account`](https://cbamvp.runasp.net/scalar/#tag/accounts/delete/api/v1/Accounts/account)  
- [`GET /api/v1/Accounts/accounts/{id}`](https://cbamvp.runasp.net/scalar/#tag/accounts/get/api/v1/Accounts/accounts/{id})  

### 🏭 Industry Operations

- [`POST /api/v1/Industries/industry`](https://cbamvp.runasp.net/scalar/#tag/industries/post/api/v1/Industries/industry) — Create an industry (Admin only)  
- [`GET /api/v1/Industries/industry`](https://cbamvp.runasp.net/scalar/#tag/industries/get/api/v1/Industries/industry) — Get industry by ID, user ID, or name  
- [`GET /api/v1/Industries/industries`](https://cbamvp.runasp.net/scalar/#tag/industries/get/api/v1/Industries/industries) — Get all industries with pagination  
- [`PUT /api/v1/Industries/industry`](https://cbamvp.runasp.net/scalar/#tag/industries/put/api/v1/Industries/industry) — Update an industry (Admin only)  
- [`DELETE /api/v1/Industries/industry`](https://cbamvp.runasp.net/scalar/#tag/industries/delete/api/v1/Industries/industry) — Delete an industry (Admin only)  

### 🏷️ Industry Field Operations

- [`POST /api/v1/IndustryFields/industry-field`](https://cbamvp.runasp.net/scalar/#tag/industryfields/post/api/v1/IndustryFields/industry-field) — Create an industry field (Admin only)  
- [`GET /api/v1/IndustryFields/industry-field`](https://cbamvp.runasp.net/scalar/#tag/industryfields/get/api/v1/IndustryFields/industry-field) — Get industry field by ID, user ID, or name  
- [`GET /api/v1/IndustryFields/industry-fields`](https://cbamvp.runasp.net/scalar/#tag/industryfields/get/api/v1/IndustryFields/industry-fields) — Get industry fields by industry ID or user ID with pagination  
- [`PUT /api/v1/IndustryFields/industry-field`](https://cbamvp.runasp.net/scalar/#tag/industryfields/put/api/v1/IndustryFields/industry-field) — Update an industry field (Admin only)  
- [`DELETE /api/v1/IndustryFields/industry-field`](https://cbamvp.runasp.net/scalar/#tag/industryfields/delete/api/v1/IndustryFields/industry-field) — Delete an industry field (Admin only)  

### 💳 Transaction Operations

- [`POST /api/v1/Transactions/deposit`](https://cbamvp.runasp.net/scalar/#tag/transactions/post/api/v1/Transactions/deposit) — Initiate a deposit (Idempotent; include `Idempotence-Key` header). When using Paystack the response includes `CheckoutUrl`.  
- [`POST /api/v1/Transactions/withdraw`](https://cbamvp.runasp.net/scalar/#tag/transactions/post/api/v1/Transactions/withdraw) — Request a withdrawal (Idempotent; include `Idempotence-Key` header).  
- [`PUT /api/v1/Transactions/flag`](https://cbamvp.runasp.net/scalar/#tag/transactions/put/api/v1/Transactions/flag) — Flag suspicious transactions (Admin & Staff).  
- [`GET /api/v1/Transactions/verify/{id}`](https://cbamvp.runasp.net/scalar/#tag/transactions/get/api/v1/Transactions/verify/{paymentReferenceId}) — Verify a payment by `paymentReferenceId`.  
- [`GET /api/v1/Transactions/transaction`](https://cbamvp.runasp.net/scalar/#tag/transactions/get/api/v1/Transactions/transaction) — Admin/Staff: query a single transaction or analytics by date/week/month/year/fromDate/toDate/user/account.  
- [`GET /api/v1/Transactions/transactions`](https://cbamvp.runasp.net/scalar/#tag/transactions/get/api/v1/Transactions/transactions) — List transactions (users see their own; staff/admin may query by user/account/date ranges).

> Notes:
> - Deposit and withdraw endpoints require `Currency = "NGN"` currently.
> - Deposit returns `PaymentReferenceId` and (when Paystack is configured) a `CheckoutUrl` for client redirection.

### 👥 User Operations

- [`POST /api/v1/Authentication/register`](https://cbamvp.runasp.net/scalar/#tag/authentication/post/api/v1/Authentication/register) — Register a new user (AllowAnonymous). Idempotent: include `Idempotence-Key`.  
- [`POST /api/v1/Authentication/login`](https://cbamvp.runasp.net/scalar/#tag/authentication/post/api/v1/Authentication/login) — User login (AllowAnonymous). Returns JWT and expiry.  
- [`POST /api/v1/Authentication/logout`](https://cbamvp.runasp.net/scalar/#tag/authentication/post/api/v1/Authentication/logout) — Logout (Authorize).  
- [`POST /api/v1/Authentication/verify-email`](https://cbamvp.runasp.net/scalar/#tag/authentication/post/api/v1/Authentication/verify-email) — Verify email with token (AllowAnonymous).  
- [`POST /api/v1/Authentication/forgot-password`](https://cbamvp.runasp.net/scalar/#tag/authentication/post/api/v1/Authentication/forgot-password) — Request password reset (AllowAnonymous; strict rate limit).  
- [`POST /api/v1/Authentication/resend-email-verification-token`](https://cbamvp.runasp.net/scalar/#tag/authentication/post/api/v1/Authentication/resend-email-verification-token) — Resend email verification token (AllowAnonymous).  
- [`PUT /api/v1/Authentication/change-password`](https://cbamvp.runasp.net/scalar/#tag/authentication/put/api/v1/Authentication/change-password) — Change password using reset token (AllowAnonymous).  
- [`PUT /api/v1/Authentication/update-password`](https://cbamvp.runasp.net/scalar/#tag/authentication/put/api/v1/Authentication/update-password) — Authenticated password update (Authorize).  
- [`GET /api/v1/Users/user/{id}`](https://cbamvp.runasp.net/scalar/#tag/users/get/api/v1/Users/user/{id}) — Get user by public id (Admin & Staff). Cached.  
- [`GET /api/v1/Users/users`](https://cbamvp.runasp.net/scalar/#tag/users/get/api/v1/Users/users) — List users (Admin & Staff). Supports filters and pagination.  
- [`GET /api/v1/Users/count`](https://cbamvp.runasp.net/scalar/#tag/users/get/api/v1/Users/count) — User counts and analytics (Admin only).  
- [`PUT /api/v1/Users/user`](https://cbamvp.runasp.net/scalar/#tag/users/put/api/v1/Users/user) — Update user profile (Admin only).  
- [`PUT /api/v1/Users/role`](https://cbamvp.runasp.net/scalar/#tag/users/put/api/v1/Users/role) — Change user role (Admin only).  
- [`PUT /api/v1/Users/profile-image`](https://cbamvp.runasp.net/scalar/#tag/users/put/api/v1/Users/profile-image) — Update profile image (Authorize).  
- [`DELETE /api/v1/Users/user`](https://cbamvp.runasp.net/scalar/#tag/users/delete/api/v1/Users/user) — Delete a single user (Admin only).  
- [`DELETE /api/v1/Users/users`](https://cbamvp.runasp.net/scalar/#tag/users/delete/api/v1/Users/users) — Delete multiple users (Admin only). Payload size is validated.

The documentation follows **OpenAPI 3.0.1** and includes model schemas, request/response formats, and error codes.

---

## 🔐 Authentication & Test Accounts

To help you explore the platform, here are pre-configured test accounts for each role:

| Role   | Email                          | Password       |
|--------|--------------------------------|----------------|
| User   | `user@gmail.com`               | `Password123!` |
| Staff  | `staff@cbamvp.runasp.net`      | `Password123!` |
| Admin  | `admin@cbamvp.runasp.net`      | `Password123!` |

After logging in, you'll receive a JWT token. **Important:**  
When making authenticated requests, pass the token directly in the `Authorization` header **without** the `"Bearer "` prefix.

✅ Example:

```http
Authorization: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

---

## 🔁 Idempotency

To ensure safe and repeatable operations, the system enforces **idempotency** on critical endpoints. This prevents duplicate processing of requests—especially useful in cases of network retries or accidental resubmissions.

### 📌 Required Header

For the following endpoints, you **must** include an `Idempotence-Key` header:

- `POST /api/v1/Authentication/register`
- `POST /api/v1/Accounts/account`
- `POST /api/v1/Transactions/deposit`
- `POST /api/v1/Transactions/withdraw`
- `POST /api/v1/Industries/industry`
- `POST /api/v1/IndustryFields/industry-field`

### 🧠 How It Works

- The `Idempotence-Key` should be a **unique GUID** for each request.
- The server stores the result of the first request with that key.
- If the same key is sent again (within 60 seconds), the server returns the original response without reprocessing.

### ✅ Example Header

```http
Idempotence-Key: 3f2504e0-4f89-11d3-9a0c-0305e82c3301

---

## 💾 Response Caching

GET endpoints implement **response caching** to improve performance and reduce database load. Cached responses are stored for **10 minutes (600 seconds)** and vary by query parameters.

### 📌 Cached Endpoints

| Endpoint | Cache Duration | Varies By |
|----------|---------------|-----------|
| `GET /api/v1/IndustryFields/industry-field` | 600s | `id`, `userPublicId`, `name` |
| `GET /api/v1/IndustryFields/industry-fields` | 600s | `industryId`, `id`, `pageNumber`, `pageSize` |

### 🧠 How It Works

- First request fetches data from the database and caches the response.
- Subsequent requests with the same query parameters return the cached response.
- Cache automatically expires after 10 minutes.

---

### MetaData format for transaction commands

For `DepositCommand` and `WithdrawCommand` the `MetaData` payload must be provided as a `Dictionary<string, string>` and follow these rules:

- Type: `Dictionary<string, string>` where each key is the industry field name and each value is the field value encoded as a string.
- Key matching: Keys must match the `IndustryField.Name` exactly after trimming (case-sensitive). Keys cannot be null or empty after trimming.
- Required fields: If an `IndustryField` has `IsRequired == true` the corresponding key must be present in `MetaData`.
- Value conversion: Each string value must be convertible to the `IndustryField.DataType`. Common mappings expected by the API include:
  - `string` / `text` — any string
  - `int` / `integer` — e.g. `"123"`
  - `long` — e.g. `"9223372036854775807"`
  - `decimal` / `double` / `float` — numeric formats, e.g. `"123.45"`
  - `bool` / `boolean` — `"true"` or `"false"`
  - `date` — ISO date or any parseable date string, e.g. `"2025-12-31"`
  - `datetime` — ISO 8601 or any parseable date/time string, e.g. `"2025-12-31T14:30:00Z"`
  - `guid` — e.g. `"3f2504e0-4f89-11d3-9a0c-0305e82c3301"`
  - `email` — a syntactically valid email address

- Limits enforced by the API's request validation:
  - Maximum entries: 100
  - Maximum key length: 200 characters (after trimming)
  - Maximum value length: 1000 characters (after trimming)
  - Duplicate keys after trimming are not allowed

Example (C#):
var meta = new Dictionary<string, string>
{
    ["CustomerId"] = "12345",
    ["IsVerified"] = "true",
    ["SignupDate"] = "2025-12-31"
};

var deposit = new DepositCommand
{
    RecipientAccountNumber = "0123456789",
    Amount = 1000.00m,
    Currency = "NGN",
    MetaData = meta,
    CreatedBy = "user-guid-here"
};

var withdraw = new WithdrawCommand
{
    AccountNumber = "0123456789",
    Amount = 500.00m,
    Currency = "NGN",
    MetaData = meta,
    CreatedBy = "user-guid-here"
};

Notes for integrators:
- Ensure keys match the exact `IndustryField.Name` used by the bank's industry configuration.
- Provide values as strings that can be parsed to the expected type on the server side.
- The API performs trimming and basic validation; sending clean, correctly typed strings minimizes validation failures.