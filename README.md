# User Management API

A comprehensive **ASP.NET Core 10.0** REST API for managing user records with enterprise-grade features including input validation, error handling, request logging, and token-based authentication.

## 📋 Overview

This API was developed for **TechHive Solutions** to enable their HR and IT departments to manage user records efficiently. It supports full CRUD operations with proper validation, security, and audit trails.

### Features

✅ **Complete CRUD Operations** - Create, read, update, and delete user records  
✅ **Input Validation** - Comprehensive validation on all user fields  
✅ **Error Handling** - Standardized error responses with detailed messages  
✅ **Bearer Token Authentication** - Secure endpoints with token validation  
✅ **Request Logging** - Audit trail of all API requests and responses  
✅ **Exception Handling Middleware** - Catches and handles all unhandled exceptions  
✅ **OpenAPI/Swagger Documentation** - Auto-generated API documentation  
✅ **Soft Delete** - Maintains data integrity with soft delete pattern  

## 🚀 Getting Started

### Prerequisites

- .NET 10.0 SDK installed
- Visual Studio Code or Visual Studio
- REST Client extension (for testing via `.http` file)

### Installation

```bash
# Clone the repository
git clone https://github.com/arunjojo-dev/User_Management_API.git
cd "User Management API"

# Restore dependencies
dotnet restore

# Build the project
dotnet build
```

### Running the API

```bash
# Run in development mode
dotnet run

# The API will be available at:
# HTTP:  http://localhost:5090
# HTTPS: https://localhost:7243
```

## 📖 API Documentation

### Base URL
```
http://localhost:5090/api
```

### Authentication
All endpoints (except `/health` and public OpenAPI endpoints) require Bearer token authentication:

```
Authorization: Bearer <your-token-here>
```

Example token for testing:
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkFkbWluIFVzZXIiLCJpYXQiOjE1MTYyMzkwMjJ9.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

### Endpoints

#### 1. Health Check (No Authentication Required)
```
GET /health
```

**Response:**
```json
{
  "status": "healthy",
  "service": "User Management API",
  "timestamp": "2026-06-13T10:30:00Z"
}
```

#### 2. Get All Users
```
GET /api/users
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "John Doe",
    "email": "john.doe@techhive.com",
    "phoneNumber": "+1-555-0101",
    "jobTitle": "Senior Developer",
    "department": "Engineering",
    "createdAt": "2026-05-14T10:30:00Z",
    "updatedAt": "2026-06-13T10:30:00Z",
    "isActive": true
  }
]
```

#### 3. Get User by ID
```
GET /api/users/{id}
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john.doe@techhive.com",
  "phoneNumber": "+1-555-0101",
  "jobTitle": "Senior Developer",
  "department": "Engineering",
  "createdAt": "2026-05-14T10:30:00Z",
  "updatedAt": "2026-06-13T10:30:00Z",
  "isActive": true
}
```

**Error Response (404 Not Found):**
```json
{
  "error": "User not found",
  "message": "No active user found with ID 9999",
  "timestamp": "2026-06-13T10:30:00Z"
}
```

#### 4. Create User
```
POST /api/users
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Alice Johnson",
  "email": "alice.johnson@techhive.com",
  "phoneNumber": "+1-555-0103",
  "jobTitle": "QA Engineer",
  "department": "Quality Assurance"
}
```

**Response (201 Created):**
```json
{
  "id": 3,
  "name": "Alice Johnson",
  "email": "alice.johnson@techhive.com",
  "phoneNumber": "+1-555-0103",
  "jobTitle": "QA Engineer",
  "department": "Quality Assurance",
  "createdAt": "2026-06-13T10:30:00Z",
  "updatedAt": "2026-06-13T10:30:00Z",
  "isActive": true
}
```

**Validation Error (400 Bad Request):**
```json
{
  "error": "Validation failed",
  "details": [
    "Invalid email format.",
    "Name must be between 2 and 100 characters."
  ],
  "timestamp": "2026-06-13T10:30:00Z"
}
```

#### 5. Update User
```
PUT /api/users/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "John Updated",
  "email": "john.updated@techhive.com",
  "phoneNumber": "+1-555-0105",
  "jobTitle": "Principal Engineer",
  "department": "Engineering"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "John Updated",
  "email": "john.updated@techhive.com",
  "phoneNumber": "+1-555-0105",
  "jobTitle": "Principal Engineer",
  "department": "Engineering",
  "createdAt": "2026-05-14T10:30:00Z",
  "updatedAt": "2026-06-13T10:35:00Z",
  "isActive": true
}
```

#### 6. Delete User
```
DELETE /api/users/{id}
Authorization: Bearer <token>
```

**Response (204 No Content):**
No response body (status code indicates success)

## 🏗️ Architecture

### Project Structure

```
User Management API/
├── Models/
│   └── User.cs                    # User entity with validation
├── Controllers/
│   └── UsersController.cs         # API endpoints
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs      # Exception handling
│   ├── AuthenticationMiddleware.cs     # Token validation
│   └── RequestLoggingMiddleware.cs     # Request/response logging
├── Properties/
│   └── launchSettings.json        # Port configuration
├── Program.cs                     # Startup & middleware pipeline
├── appsettings.json              # Configuration
├── appsettings.Development.json  # Dev overrides
├── User Management API.http      # API testing file
└── AGENTS.md                      # AI agent guidelines
```

### Middleware Pipeline

The middleware executes in this order (critical for correctness):

```
1. ErrorHandlingMiddleware      → Catches all exceptions
   ↓
2. AuthenticationMiddleware      → Validates Bearer tokens
   ↓
3. RequestLoggingMiddleware      → Logs requests/responses
   ↓
4. Business Logic                → Controllers
```

### Data Model

**User Entity:**
```csharp
public class User
{
    public int Id { get; set; }                    // Auto-generated
    public string Name { get; set; }               // Required, 2-100 chars
    public string Email { get; set; }              // Required, valid email
    public string? PhoneNumber { get; set; }       // Optional, valid phone
    public string JobTitle { get; set; }           // Required, max 50 chars
    public string Department { get; set; }         // Required, max 50 chars
    public DateTime CreatedAt { get; set; }        // Auto-set UTC
    public DateTime UpdatedAt { get; set; }        // Auto-updated UTC
    public bool IsActive { get; set; }             // Soft delete flag
}
```

### Validation Rules

| Field | Rules |
|-------|-------|
| Name | Required, 2-100 characters |
| Email | Required, valid email format, unique across active users |
| PhoneNumber | Optional, valid phone format if provided |
| JobTitle | Required, max 50 characters |
| Department | Required, max 50 characters |

### HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request succeeded |
| 201 | Created - New resource created |
| 204 | No Content - Deletion successful |
| 400 | Bad Request - Validation failed |
| 401 | Unauthorized - Invalid/missing token |
| 404 | Not Found - Resource doesn't exist |
| 500 | Internal Server Error - Unhandled exception |

## 🧪 Testing

### Using the .http File

The project includes `User Management API.http` with ready-to-use test requests:

1. Open `User Management API.http` in VS Code
2. Install the "REST Client" extension if not already installed
3. Update the `@AuthToken` variable with a valid token
4. Click "Send Request" above any request

**Example Test Sequence:**
```
1. GET /health                  (no auth required)
2. GET /api/users              (list all users)
3. POST /api/users             (create new user)
4. GET /api/users/{id}         (get specific user)
5. PUT /api/users/{id}         (update user)
6. DELETE /api/users/{id}      (delete user)
```

### Using cURL

```bash
# Health check
curl http://localhost:5090/health

# Get all users (requires token)
curl -H "Authorization: Bearer <token>" \
     http://localhost:5090/api/users

# Create user
curl -X POST http://localhost:5090/api/users \
     -H "Authorization: Bearer <token>" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Test User",
       "email": "test@example.com",
       "phoneNumber": "+1-555-0100",
       "jobTitle": "Test Role",
       "department": "Testing"
     }'
```

## 📊 Logging

The API logs all activities for auditing and debugging:

**Log Levels (configurable in appsettings.json):**
- **Debug** - Detailed information for development
- **Information** - General informational messages (requests, CRUD ops)
- **Warning** - Warning messages (401 unauthorized, validation failures)
- **Error** - Error messages (exceptions, 500 errors)

**Logged Information:**
- HTTP method, path, status code
- Request/response processing time
- Authentication attempts (success/failure)
- CRUD operations (create, update, delete)
- Validation errors and exceptions

## 🔒 Security Considerations

### Current Implementation
- Bearer token validation on protected endpoints
- Basic token format validation
- Public endpoints for health checks and documentation

### For Production
1. **Replace token validation** with JWT or OAuth 2.0
2. **Enable HTTPS only** (HTTP redirect enforced)
3. **Add rate limiting** to prevent abuse
4. **Implement CORS policies** for specific origins
5. **Use database** instead of in-memory storage
6. **Add input sanitization** for all fields
7. **Implement role-based access control (RBAC)**
8. **Use secrets management** for sensitive configuration

## 📝 Configuration

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ApiSettings": {
    "Name": "User Management API",
    "Version": "1.0.0",
    "Environment": "Development"
  }
}
```

### Environment Variables
Set via `appsettings.Development.json` or system environment:
- `ASPNETCORE_ENVIRONMENT` - Development/Production
- `ASPNETCORE_URLS` - API URLs (ports)

## 🐛 Troubleshooting

### Port Already in Use
```bash
# Find process using port 5090
netstat -ano | findstr :5090

# Kill the process
taskkill /PID <PID> /F
```

### 401 Unauthorized
- Ensure Authorization header is included
- Token must start with "Bearer "
- Token must be at least 10 characters

### Validation Errors
- Check all required fields are provided
- Email must be in valid format
- String fields must meet length requirements

## 🚀 Future Enhancements

- [ ] Entity Framework Core integration
- [ ] SQL Server database
- [ ] JWT token generation endpoint
- [ ] User roles and permissions
- [ ] Pagination for user lists
- [ ] Advanced filtering and search
- [ ] Unit and integration tests
- [ ] CI/CD pipeline
- [ ] Docker containerization
- [ ] API versioning (v2, v3)

## 📝 License

This project is developed for TechHive Solutions.

## 👤 Author

**Arun Jojo**  
GitHub: [@arunjojo-dev](https://github.com/arunjojo-dev)

## AI Assistance

This repository used GitHub Copilot during debugging and development to help identify and fix validation and middleware issues. Commits and documentation may reference Copilot where applicable.

