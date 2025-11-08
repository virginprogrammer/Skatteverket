# Momsdeklaration API

🇸🇪 **A comprehensive ASP.NET Core Web API for integrating with Swedish Tax Authority (Skatteverket) VAT Declaration (Momsdeklaration) system.**

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/License-Private-red)]()
[![Docker](https://img.shields.io/badge/Docker-Ready-green)](https://www.docker.com)

## 🚀 Features

### Core Functionality
- ✅ **Complete VAT Declaration Management** - Draft creation, validation, submission, and retrieval
- ✅ **OAuth 2.0 Client Credentials Grant** - Secure authentication with Skatteverket
- ✅ **Certificate-based Authentication** - Organization certificate support
- ✅ **Multi-organization Support** - Handle declarations for multiple companies
- ✅ **Real-time Validation** - Business rule validation and error handling
- ✅ **Lock Management** - Draft locking for signing workflows

### Security & Compliance
- 🔒 **Enterprise Security** - JWT authentication, certificate validation, rate limiting
- 📋 **Comprehensive Auditing** - Full API call tracking and security event logging  
- 🛡️ **Data Protection** - Encrypted sensitive data storage and secure string handling
- 🔐 **Certificate Management** - Advanced X.509 certificate validation and chain verification
- 🚨 **Security Headers** - HSTS, CSP, XSS protection, and more

### Infrastructure & Operations
- 📊 **Health Monitoring** - Memory, disk, and external service health checks
- 📝 **Structured Logging** - Serilog with correlation ID tracking
- ⚡ **Performance Optimized** - Retry policies, circuit breakers, and caching
- 🐳 **Container Ready** - Docker support with multi-stage builds
- 📈 **Production Ready** - Rate limiting, error handling, and monitoring

### Developer Experience
- 📚 **OpenAPI/Swagger** - Complete API documentation with examples
- 🧪 **Input Validation** - FluentValidation with Swedish business rules
- 🔄 **Auto-mapping** - AutoMapper integration for clean data transforms
- 📦 **Dependency Injection** - Clean architecture with service abstractions

## 🏗️ Architecture

Built following **Clean Architecture** principles with clear separation of concerns:

```
├── Controllers/           # API endpoints and HTTP handling
├── Models/               
│   ├── DTOs/             # Data transfer objects
│   ├── Requests/         # API request models
│   ├── Responses/        # API response models
│   └── Validators/       # FluentValidation validators
├── Services/             # Business logic layer
├── Authentication/       # OAuth and certificate auth
├── Configuration/        # App configuration and settings
├── Middleware/           # Cross-cutting concerns
└── Utils/               # Utilities and extensions
```

## 🛠️ Prerequisites

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download)
- **Skatteverket API Credentials**:
  - Client ID 
  - Client Secret
  - Organization certificate (.pfx format)
- **SSL Certificate** (production deployments)

## ⚙️ Configuration

### Environment Setup

Create `appsettings.json` or set environment variables:

```json
{
  "SkatteverketAPI": {
    "UseTestEnvironment": true,
    "OAuth": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    },
    "Certificate": {
      "Path": "certificates/organization.pfx",
      "Password": "your-certificate-password"
    }
  }
}
```

### Environment Variables (Recommended for Production)

```bash
export SKATTEVERKET_CLIENT_ID="your-client-id"
export SKATTEVERKET_CLIENT_SECRET="your-client-secret" 
export CERTIFICATE_PASSWORD="your-cert-password"
export ASPNETCORE_ENVIRONMENT="Production"
```

## 🚀 Quick Start

### Development Setup

```bash
# Clone and navigate to project
git clone <repository-url>
cd MomsdeklarationAPI

# Restore dependencies
dotnet restore

# Run the application  
dotnet run --project MomsdeklarationAPI

# Access Swagger UI
open https://localhost:7000
```

### Docker Deployment

```bash
# Using Docker Compose (recommended)
docker-compose up -d

# Or build manually
docker build -t momsdeklaration-api .
docker run -p 8080:8080 momsdeklaration-api
```

## 📚 API Endpoints

### Core VAT Declaration Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/ping` | Health check ping |
| `POST` | `/api/utkast` | Get multiple drafts |
| `POST` | `/api/utkast/{redovisare}/{period}` | Create/update draft |
| `GET` | `/api/utkast/{redovisare}/{period}` | Get specific draft |
| `DELETE` | `/api/utkast/{redovisare}/{period}` | Delete draft |
| `POST` | `/api/kontrollera/{redovisare}/{period}` | Validate draft |
| `PUT` | `/api/las/{redovisare}/{period}` | Lock for signing |
| `DELETE` | `/api/las/{redovisare}/{period}` | Unlock draft |
| `POST` | `/api/inlamnat` | Get submitted declarations |
| `GET` | `/api/inlamnat/{redovisare}/{period}` | Get specific submission |
| `POST` | `/api/beslutat` | Get decided declarations |
| `GET` | `/api/beslutat/{redovisare}/{period}` | Get specific decision |

### Monitoring & Health

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/health` | Application health status |
| `GET` | `/swagger` | API documentation |

## 📊 Data Models

### VAT Declaration (Momsuppgift)

```json
{
  "momspliktigForsaljning": 100000,      // Taxable sales
  "momsForsaljningUtgaendeHog": 25000,   // Outgoing VAT 25%
  "momsForsaljningUtgaendeMedel": 6000,  // Outgoing VAT 12%
  "momsForsaljningUtgaendeLag": 3000,    // Outgoing VAT 6%
  "ingaendeMomsAvdrag": 5000,           // Input VAT deduction
  "summaMoms": 29000                    // Total VAT to pay/receive
}
```

### Request Example

```json
{
  "momsuppgift": {
    "momspliktigForsaljning": 100000,
    "momsForsaljningUtgaendeHog": 25000,
    "ingaendeMomsAvdrag": 5000,
    "summaMoms": 20000
  },
  "kommentar": "Monthly VAT declaration"
}
```

## 🔒 Security Features

### Authentication Flow
1. **Certificate Authentication** - Client presents organization certificate
2. **OAuth Token Exchange** - Client credentials grant with Skatteverket
3. **JWT Token Validation** - Bearer token for API requests
4. **Request Correlation** - Unique correlation ID for request tracking

### Security Measures
- **Rate Limiting** - 100 requests per 15 minutes per client
- **Input Validation** - All inputs validated using FluentValidation
- **Audit Logging** - All operations logged with user context
- **Data Encryption** - Sensitive data encrypted at rest
- **Security Headers** - HSTS, CSP, XSS protection enabled

## 🔧 Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Code Quality

```bash
# Format code
dotnet format

# Analyze code
dotnet analyze
```

### Local Development

The application uses test endpoints by default in development:
- Test API base URL: `https://test.app.skatteverket.se/momsdeklaration/v1`
- Test OAuth endpoint: `https://test-orgoauth2.skatteverket.se/oauth2/v1/org/token`

Set `UseTestEnvironment: false` for production endpoints.

## 📈 Monitoring & Observability

### Health Checks
- **Application Health** - `/health` endpoint
- **External Dependencies** - Skatteverket API connectivity
- **System Resources** - Memory, disk space, CPU usage

### Logging
- **Structured Logging** - JSON-formatted logs with Serilog
- **Correlation Tracking** - Request correlation IDs
- **Security Events** - Authentication and authorization events
- **Performance Metrics** - Request duration and error rates

### Metrics
- API response times
- Error rates by endpoint
- Authentication success/failure rates
- Certificate validation events

## 🚀 Deployment

### Production Deployment Options

1. **Docker (Recommended)**
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```

2. **Azure App Service**
   ```bash
   az webapp create --name momsdeklaration-api --plan myplan
   az webapp deployment source config-zip --src release.zip
   ```

3. **Traditional Server**
   ```bash
   dotnet publish -c Release -o ./publish
   # Copy to server and configure as system service
   ```

See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed deployment instructions.

## 🔧 Configuration Reference

### Complete Configuration Example

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "SkatteverketAPI": {
    "BaseUrl": "https://app.skatteverket.se/momsdeklaration/v1",
    "TestBaseUrl": "https://test.app.skatteverket.se/momsdeklaration/v1",
    "UseTestEnvironment": false,
    "OAuth": {
      "ClientId": "${CLIENT_ID}",
      "ClientSecret": "${CLIENT_SECRET}",
      "TokenEndpoint": "https://orgoauth2.skatteverket.se/oauth2/v1/org/token",
      "Scope": "momsdeklaration:read momsdeklaration:write"
    },
    "Certificate": {
      "Path": "/certificates/organization.pfx",
      "Password": "${CERT_PASSWORD}",
      "ValidationMode": "ChainTrust"
    },
    "Timeout": 30,
    "RetryCount": 3
  }
}
```

## 🧪 Testing

### API Testing with cURL

```bash
# Health check
curl http://localhost:5000/health

# Get draft (requires authentication)
curl -X GET "http://localhost:5000/api/utkast/1234567890/202412" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Create draft
curl -X POST "http://localhost:5000/api/utkast/1234567890/202412" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "momsuppgift": {
      "momspliktigForsaljning": 100000,
      "momsForsaljningUtgaendeHog": 25000
    }
  }'
```

## 📝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Standards
- Follow C# coding conventions
- Include unit tests for new features
- Update documentation for API changes
- Ensure security best practices

## 🆘 Troubleshooting

### Common Issues

**Certificate Errors:**
```bash
# Verify certificate
openssl pkcs12 -in certificate.pfx -noout -info

# Check certificate permissions
ls -la certificates/
```

**Authentication Issues:**
```bash
# Check logs for auth errors
grep -i "auth" logs/momsdeklaration-*.txt

# Verify client credentials
curl -X POST "https://test-orgoauth2.skatteverket.se/oauth2/v1/org/token" \
  -d "grant_type=client_credentials&client_id=YOUR_ID&client_secret=YOUR_SECRET"
```

**API Connectivity:**
```bash
# Test Skatteverket connectivity
curl https://test.app.skatteverket.se/momsdeklaration/v1/ping
```

## 📋 Compliance & Standards

- ✅ **Swedish Tax Authority (Skatteverket) API Compliance**
- ✅ **GDPR Data Protection Compliance**
- ✅ **OAuth 2.0 Security Standards**
- ✅ **X.509 Certificate Standards**
- ✅ **OpenAPI 3.0 Specification**

## 🤝 Support

- **Documentation**: See [DEPLOYMENT.md](DEPLOYMENT.md) for deployment guide
- **Issues**: Report bugs via GitHub Issues  
- **Security**: Report security issues privately to security@yourcompany.com

## 📄 License

This project is proprietary software. All rights reserved.

---

**Built with ❤️ for Swedish businesses using .NET 8.0**