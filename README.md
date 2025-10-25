# 💳 Banking Transaction System (.NET 8)

A robust backend system built with .NET 8 to manage banking operations including account creation, transactions, and external payment integration (using Paystack). Designed with scalability, security, and performance in mind.

---

## 🚀 Features

- 🏦 Account creation and management  
- 💰 Deposits, withdrawals, and transfers  
- 📄 Monthly transaction statements  
- 🔐 Role-based access control (RBAC)  
- 🔄 CQRS pattern for command/query separation  
- 🔗 External payment integration (Paystack) 
- 📊 Structured logging and monitoring  
- 🧪 Unit testing with xUnit  
- 🔁 Idempotency support for critical operations  
- 🚦 Rate limiting to prevent abuse and ensure stability   

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

### 🧠 How It Works

- The `Idempotence-Key` should be a **unique GUID** for each request.
- The server stores the result of the first request with that key.
- If the same key is sent again (within 60 seconds), the server returns the original response without reprocessing.

### ✅ Example Header

```http
Idempotence-Key: 3f2504e0-4f89-11d3-9a0c-0305e82c3301
