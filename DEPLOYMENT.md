# Deployment Guide for Momsdeklaration API

## Prerequisites

1. **.NET 8.0 SDK** - Download from [Microsoft .NET](https://dotnet.microsoft.com/download)
2. **Valid Skatteverket API Credentials**:
   - Client ID
   - Client Secret
   - Organization certificate (.pfx format)
3. **SSL Certificate** for HTTPS (production)

## Configuration Setup

### 1. Environment Variables

Set the following environment variables:

```bash
# Required
SKATTEVERKET_CLIENT_ID=your-client-id
SKATTEVERKET_CLIENT_SECRET=your-client-secret
CERTIFICATE_PASSWORD=your-certificate-password

# Optional
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80
```

### 2. appsettings.json Configuration

Update `appsettings.json` or `appsettings.Production.json`:

```json
{
  "SkatteverketAPI": {
    "UseTestEnvironment": false,
    "OAuth": {
      "ClientId": "${SKATTEVERKET_CLIENT_ID}",
      "ClientSecret": "${SKATTEVERKET_CLIENT_SECRET}"
    },
    "Certificate": {
      "Path": "/certificates/organization.pfx",
      "Password": "${CERTIFICATE_PASSWORD}",
      "Thumbprint": "your-cert-thumbprint"
    }
  }
}
```

### 3. Certificate Setup

Place your organization certificate in the appropriate directory:

**Development:**
```
MomsdeklarationAPI/certificates/test-cert.pfx
```

**Production:**
```
/etc/ssl/certs/organization.pfx
```

## Deployment Options

### Option 1: Docker Deployment (Recommended)

1. **Build and run with Docker Compose:**
```bash
# Clone the repository
git clone <repository-url>
cd MomsdeklarationAPI

# Create certificates directory
mkdir certificates

# Copy your certificate to certificates directory
cp /path/to/your/certificate.pfx ./certificates/

# Build and start services
docker-compose up -d
```

2. **Verify deployment:**
```bash
# Check health
curl http://localhost:8080/health

# Access Swagger UI
curl http://localhost:8080/swagger
```

### Option 2: Traditional Deployment

1. **Build the application:**
```bash
cd MomsdeklarationAPI
dotnet restore
dotnet build --configuration Release
dotnet publish --configuration Release --output ./publish
```

2. **Deploy to server:**
```bash
# Copy published files to server
scp -r ./publish/* user@server:/var/www/momsdeklaration/

# Install as systemd service (Linux)
sudo cp momsdeklaration.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable momsdeklaration
sudo systemctl start momsdeklaration
```

### Option 3: Azure App Service

1. **Prepare for Azure:**
```bash
# Install Azure CLI
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Login to Azure
az login

# Create resource group
az group create --name rg-momsdeklaration --location westeurope

# Create App Service plan
az appservice plan create --name plan-momsdeklaration --resource-group rg-momsdeklaration --sku P1V3 --is-linux
```

2. **Deploy to Azure:**
```bash
# Create web app
az webapp create --resource-group rg-momsdeklaration --plan plan-momsdeklaration --name momsdeklaration-api --runtime "DOTNETCORE:8.0"

# Configure app settings
az webapp config appsettings set --resource-group rg-momsdeklaration --name momsdeklaration-api --settings \
  SKATTEVERKET_CLIENT_ID="your-client-id" \
  SKATTEVERKET_CLIENT_SECRET="your-client-secret" \
  ASPNETCORE_ENVIRONMENT="Production"

# Deploy code
az webapp deployment source config-zip --resource-group rg-momsdeklaration --name momsdeklaration-api --src publish.zip
```

## Security Configuration

### 1. SSL/TLS Setup

**For production, configure HTTPS:**

```json
{
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Path": "/certificates/ssl-cert.pfx",
        "Password": "ssl-cert-password"
      }
    }
  }
}
```

### 2. Firewall Configuration

Open required ports:
```bash
# Allow HTTPS
sudo ufw allow 443

# Allow HTTP (if needed)
sudo ufw allow 80

# Allow health checks
sudo ufw allow 8080
```

### 3. Certificate Store Configuration

For Windows servers, install certificates in the certificate store:

```powershell
# Import certificate to Personal store
Import-PfxCertificate -FilePath "C:\certificates\organization.pfx" -CertStoreLocation Cert:\LocalMachine\My -Password (ConvertTo-SecureString "password" -AsPlainText -Force)
```

## Monitoring and Logging

### 1. Log Configuration

Logs are written to:
- Console (always)
- Files: `/app/logs/` or `./logs/` (configurable)

### 2. Health Checks

Monitor application health:
```bash
# Basic health check
curl http://localhost:8080/health

# Detailed health check (if enabled)
curl http://localhost:8080/health/detailed
```

### 3. Application Insights (Azure)

Enable Application Insights for Azure deployments:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "your-connection-string"
  }
}
```

## Load Balancing

For high availability, use a load balancer:

### Nginx Configuration:

```nginx
upstream momsdeklaration_backend {
    server 127.0.0.1:8080;
    server 127.0.0.1:8081;
}

server {
    listen 80;
    server_name momsdeklaration.yourdomain.com;
    
    location / {
        proxy_pass http://momsdeklaration_backend;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
    
    location /health {
        proxy_pass http://momsdeklaration_backend;
        access_log off;
    }
}
```

## Troubleshooting

### Common Issues:

1. **Certificate errors:**
   - Verify certificate path and password
   - Check certificate expiration
   - Ensure certificate has correct permissions

2. **Authentication failures:**
   - Verify Client ID and Client Secret
   - Check token endpoint URLs
   - Ensure network connectivity to Skatteverket

3. **Performance issues:**
   - Check memory and CPU usage
   - Review log files for errors
   - Monitor database connections (if applicable)

### Log Analysis:

```bash
# View recent logs
tail -f /app/logs/momsdeklaration-$(date +%Y%m%d).txt

# Search for errors
grep -i "error\|exception" /app/logs/momsdeklaration-*.txt

# Check authentication issues
grep -i "auth" /app/logs/momsdeklaration-*.txt
```

## Backup and Recovery

### 1. Configuration Backup

Backup critical configuration files:
```bash
# Create backup directory
mkdir -p /backups/momsdeklaration

# Backup configurations
cp appsettings.Production.json /backups/momsdeklaration/
cp certificates/*.pfx /backups/momsdeklaration/ # Store securely!
```

### 2. Application Backup

Create application backup:
```bash
# Create application archive
tar -czf momsdeklaration-backup-$(date +%Y%m%d).tar.gz /var/www/momsdeklaration/
```

## Security Checklist

- [ ] SSL/TLS certificates properly configured
- [ ] Client credentials securely stored
- [ ] Organization certificates protected
- [ ] Network access restricted to necessary ports
- [ ] Logging configured but sensitive data excluded
- [ ] Regular security updates applied
- [ ] Backup procedures implemented
- [ ] Monitor logs for suspicious activity

## Support

For deployment issues:

1. Check the application logs
2. Verify configuration settings
3. Test network connectivity
4. Review Skatteverket API documentation
5. Contact system administrator

## Performance Tuning

### Production Settings:

```json
{
  "SkatteverketAPI": {
    "Timeout": 30,
    "RetryCount": 3,
    "RetryDelaySeconds": 2
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "MomsdeklarationAPI": "Information"
    }
  }
}
```

### Resource Requirements:

- **Minimum**: 2 CPU cores, 4GB RAM
- **Recommended**: 4 CPU cores, 8GB RAM
- **Storage**: 50GB minimum for logs and certificates