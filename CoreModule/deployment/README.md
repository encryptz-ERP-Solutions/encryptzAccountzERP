# Deployment Files

This directory contains deployment scripts and configuration files for Linux server deployment.

## Files

- **deploy.sh** - Automated deployment script
- **encryptz-api.service** - Systemd service file for the API
- **nginx-encryptz-admin.conf** - Nginx configuration for frontend
- **nginx-encryptz-api.conf** - Nginx configuration for API

## Quick Start

### 1. Make deploy script executable

```bash
chmod +x deployment/deploy.sh
```

### 2. Install Systemd Service

```bash
sudo cp deployment/encryptz-api.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable encryptz-api
```

### 3. Install Nginx Configurations

```bash
# Frontend
sudo cp deployment/nginx-encryptz-admin.conf /etc/nginx/sites-available/encryptz-admin
sudo ln -s /etc/nginx/sites-available/encryptz-admin /etc/nginx/sites-enabled/

# API
sudo cp deployment/nginx-encryptz-api.conf /etc/nginx/sites-available/encryptz-api
sudo ln -s /etc/nginx/sites-available/encryptz-api /etc/nginx/sites-enabled/

# Edit configurations to update domain names
sudo nano /etc/nginx/sites-available/encryptz-admin
sudo nano /etc/nginx/sites-available/encryptz-api

# Test and reload
sudo nginx -t
sudo systemctl reload nginx
```

### 4. Run Deployment

```bash
# Full deployment (requires root for service management)
sudo ./deployment/deploy.sh

# Deploy only API
sudo ./deployment/deploy.sh api

# Deploy only frontend
sudo ./deployment/deploy.sh frontend
```

## Configuration

Before deploying, make sure to:

1. Update domain names in Nginx configurations
2. Configure `appsettings.json` with production values
3. Update `environment.prod.ts` with production API URL
4. Update CORS origins in `Program.cs`

See the main [LINUX_DEPLOYMENT_GUIDE.md](../LINUX_DEPLOYMENT_GUIDE.md) for detailed instructions.

