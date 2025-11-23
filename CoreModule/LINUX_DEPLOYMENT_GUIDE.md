# Linux Server Deployment Guide

This guide will help you deploy the encryptzERP application to a Linux server.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Server Setup](#server-setup)
3. [Database Setup](#database-setup)
4. [Backend API Deployment](#backend-api-deployment)
5. [Frontend Deployment](#frontend-deployment)
6. [Nginx Configuration](#nginx-configuration)
7. [SSL/HTTPS Setup](#sslhttps-setup)
8. [Service Management](#service-management)
9. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software
- **.NET 8.0 SDK** - For building and running the API
- **Node.js 18+ and npm** - For building the Angular frontend
- **PostgreSQL 12+** - Database server
- **Nginx** - Web server and reverse proxy
- **Git** - For cloning the repository

### Server Requirements
- **OS**: Ubuntu 20.04+ / Debian 11+ / CentOS 8+ / RHEL 8+
- **RAM**: Minimum 2GB (4GB+ recommended)
- **CPU**: 2+ cores recommended
- **Disk**: 20GB+ free space
- **Network**: Ports 80, 443, 5000-5007 (or your chosen ports)

---

## Server Setup

### 1. Update System Packages

```bash
sudo apt update && sudo apt upgrade -y  # Ubuntu/Debian
# OR
sudo yum update -y  # CentOS/RHEL
```

### 2. Install .NET 8.0 SDK

```bash
# Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
export PATH=$PATH:$HOME/.dotnet

# Add to ~/.bashrc or ~/.zshrc for persistence
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc

# Verify installation
dotnet --version
```

### 3. Install Node.js and npm

```bash
# Using NodeSource repository (recommended)
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs  # Ubuntu/Debian

# OR using nvm (Node Version Manager)
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.0/install.sh | bash
source ~/.bashrc
nvm install 18
nvm use 18

# Verify installation
node --version
npm --version
```

### 4. Install PostgreSQL

```bash
# Ubuntu/Debian
sudo apt install postgresql postgresql-contrib -y

# CentOS/RHEL
sudo yum install postgresql-server postgresql-contrib -y
sudo postgresql-setup --initdb
sudo systemctl enable postgresql
sudo systemctl start postgresql

# Verify installation
psql --version
```

### 5. Install Nginx

```bash
# Ubuntu/Debian
sudo apt install nginx -y

# CentOS/RHEL
sudo yum install nginx -y

# Start and enable Nginx
sudo systemctl enable nginx
sudo systemctl start nginx

# Verify installation
nginx -v
```

### 6. Install Git

```bash
sudo apt install git -y  # Ubuntu/Debian
# OR
sudo yum install git -y  # CentOS/RHEL
```

---

## Database Setup

### 1. Create PostgreSQL Database and User

```bash
# Switch to postgres user
sudo -u postgres psql

# In PostgreSQL prompt, run:
CREATE DATABASE encryptzERPCore;
CREATE USER encryptzuser WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE encryptzERPCore TO encryptzuser;
\q
```

### 2. Configure PostgreSQL for Remote Access (if needed)

Edit `/etc/postgresql/*/main/postgresql.conf`:
```conf
listen_addresses = '*'  # or specific IP
```

Edit `/etc/postgresql/*/main/pg_hba.conf`:
```
host    encryptzERPCore    encryptzuser    0.0.0.0/0    md5
```

Restart PostgreSQL:
```bash
sudo systemctl restart postgresql
```

### 3. Run Database Migrations

```bash
# Navigate to your project directory
cd /path/to/CoreModule

# Run SQL migration scripts from migrations/sql/ directory
# Execute them in order using psql:
psql -h localhost -U encryptzuser -d encryptzERPCore -f migrations/sql/2025_11_20_create_refresh_tokens_table.sql
# ... continue with other migration files
```

---

## Backend API Deployment

### 1. Clone/Upload Project

```bash
# If using Git
git clone <your-repo-url> /opt/encryptzERP
cd /opt/encryptzERP/CoreModule

# OR upload your project files to /opt/encryptzERP
```

### 2. Configure appsettings.json

```bash
cd /opt/encryptzERP/CoreModule/API/encryptzERP
cp appsettings.example.json appsettings.json
nano appsettings.json  # or use your preferred editor
```

Update the following:
- **ConnectionStrings.DefaultConnection**: Your PostgreSQL connection string
- **JwtSettings.SecretKey**: Generate a strong secret key (at least 32 characters)
- **JwtSettings.Issuer/Audience**: Your domain names
- **EmailSettings**: Your SMTP configuration
- **Kestrel.Endpoints**: Configure your API port (e.g., `http://localhost:5000`)

### 3. Build and Publish the API

```bash
cd /opt/encryptzERP/CoreModule/API/encryptzERP

# Restore dependencies
dotnet restore

# Build the project
dotnet build --configuration Release

# Publish for Linux
dotnet publish --configuration Release --output /opt/encryptzERP/api-publish
```

### 4. Create Systemd Service

Create `/etc/systemd/system/encryptz-api.service`:

```ini
[Unit]
Description=encryptzERP API Service
After=network.target postgresql.service

[Service]
Type=notify
User=www-data
Group=www-data
WorkingDirectory=/opt/encryptzERP/api-publish
ExecStart=/usr/bin/dotnet /opt/encryptzERP/api-publish/encryptzERP.dll
Restart=always
RestartSec=10
SyslogIdentifier=encryptz-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_ROOT=/usr/share/dotnet

[Install]
WantedBy=multi-user.target
```

### 5. Start the API Service

```bash
# Reload systemd
sudo systemctl daemon-reload

# Enable service to start on boot
sudo systemctl enable encryptz-api

# Start the service
sudo systemctl start encryptz-api

# Check status
sudo systemctl status encryptz-api

# View logs
sudo journalctl -u encryptz-api -f
```

---

## Frontend Deployment

### 1. Navigate to Frontend Directory

```bash
cd /opt/encryptzERP/CoreModule/UI/Admin/encryptz.Admin
```

### 2. Update Environment Configuration

Edit `src/environments/environment.prod.ts`:

```typescript
export const environment = {
    production: true,
    apiUrl: 'https://api.yourdomain.com/'  // Your API URL
};
```

### 3. Install Dependencies and Build

```bash
# Install dependencies
npm install

# Build for production
npm run build -- --configuration production

# The output will be in dist/encryptz.admin/
```

### 4. Copy Build Output to Web Directory

```bash
# Create web directory
sudo mkdir -p /var/www/encryptz-admin

# Copy build output
sudo cp -r dist/encryptz.admin/* /var/www/encryptz-admin/

# Set permissions
sudo chown -R www-data:www-data /var/www/encryptz-admin
sudo chmod -R 755 /var/www/encryptz-admin
```

---

## Nginx Configuration

### 1. Create Nginx Configuration for Frontend

Create `/etc/nginx/sites-available/encryptz-admin`:

```nginx
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;

    root /var/www/encryptz-admin;
    index index.html;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/x-javascript application/xml+rss application/json;

    # Angular routing - serve index.html for all routes
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache static assets
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
}
```

### 2. Create Nginx Configuration for API

Create `/etc/nginx/sites-available/encryptz-api`:

```nginx
server {
    listen 80;
    server_name api.yourdomain.com;

    # API proxy
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
}
```

### 3. Enable Sites and Test Configuration

```bash
# Enable sites
sudo ln -s /etc/nginx/sites-available/encryptz-admin /etc/nginx/sites-enabled/
sudo ln -s /etc/nginx/sites-available/encryptz-api /etc/nginx/sites-enabled/

# Remove default site (optional)
sudo rm /etc/nginx/sites-enabled/default

# Test Nginx configuration
sudo nginx -t

# Reload Nginx
sudo systemctl reload nginx
```

### 4. Update CORS in Program.cs

Update the `allowedOrigins` array in `API/encryptzERP/Program.cs` to include your production domains:

```csharp
var allowedOrigins = new[]
{
    "https://yourdomain.com",
    "https://www.yourdomain.com",
    "https://api.yourdomain.com"
};
```

Rebuild and restart the API after this change.

---

## SSL/HTTPS Setup

### Option 1: Using Let's Encrypt (Free SSL)

```bash
# Install Certbot
sudo apt install certbot python3-certbot-nginx -y  # Ubuntu/Debian

# Obtain SSL certificate
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com
sudo certbot --nginx -d api.yourdomain.com

# Certbot will automatically update your Nginx config
# Certificates auto-renew via cron job
```

### Option 2: Using Custom SSL Certificate

Update your Nginx configs to include SSL:

```nginx
server {
    listen 443 ssl http2;
    server_name yourdomain.com;

    ssl_certificate /path/to/certificate.crt;
    ssl_certificate_key /path/to/private.key;
    
    # SSL configuration
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    # ... rest of configuration
}

# Redirect HTTP to HTTPS
server {
    listen 80;
    server_name yourdomain.com;
    return 301 https://$server_name$request_uri;
}
```

---

## Service Management

### API Service Commands

```bash
# Start
sudo systemctl start encryptz-api

# Stop
sudo systemctl stop encryptz-api

# Restart
sudo systemctl restart encryptz-api

# Status
sudo systemctl status encryptz-api

# View logs
sudo journalctl -u encryptz-api -f
sudo journalctl -u encryptz-api --since "1 hour ago"
```

### Nginx Commands

```bash
# Start
sudo systemctl start nginx

# Stop
sudo systemctl stop nginx

# Restart
sudo systemctl restart nginx

# Reload (without downtime)
sudo systemctl reload nginx

# Test configuration
sudo nginx -t
```

### PostgreSQL Commands

```bash
# Start
sudo systemctl start postgresql

# Stop
sudo systemctl stop postgresql

# Restart
sudo systemctl restart postgresql

# Status
sudo systemctl status postgresql
```

---

## Troubleshooting

### API Not Starting

1. **Check logs:**
   ```bash
   sudo journalctl -u encryptz-api -n 50
   ```

2. **Verify .NET installation:**
   ```bash
   dotnet --version
   which dotnet
   ```

3. **Check file permissions:**
   ```bash
   ls -la /opt/encryptzERP/api-publish
   ```

4. **Verify database connection:**
   ```bash
   psql -h localhost -U encryptzuser -d encryptzERPCore
   ```

### Frontend Not Loading

1. **Check Nginx error logs:**
   ```bash
   sudo tail -f /var/log/nginx/error.log
   ```

2. **Verify file permissions:**
   ```bash
   ls -la /var/www/encryptz-admin
   ```

3. **Check Nginx configuration:**
   ```bash
   sudo nginx -t
   ```

### Database Connection Issues

1. **Test PostgreSQL connection:**
   ```bash
   psql -h localhost -U encryptzuser -d encryptzERPCore
   ```

2. **Check PostgreSQL logs:**
   ```bash
   sudo tail -f /var/log/postgresql/postgresql-*.log
   ```

3. **Verify connection string in appsettings.json**

### CORS Errors

1. **Update CORS origins in Program.cs** to include your production domain
2. **Rebuild and restart the API**
3. **Check browser console for specific CORS error messages**

### Port Already in Use

```bash
# Find process using port
sudo lsof -i :5000
sudo netstat -tulpn | grep :5000

# Kill process if needed
sudo kill -9 <PID>
```

---

## Security Best Practices

1. **Firewall Configuration:**
   ```bash
   # Ubuntu/Debian (UFW)
   sudo ufw allow 22/tcp
   sudo ufw allow 80/tcp
   sudo ufw allow 443/tcp
   sudo ufw enable
   ```

2. **Keep Software Updated:**
   ```bash
   sudo apt update && sudo apt upgrade -y
   ```

3. **Use Strong Passwords:**
   - Database passwords
   - JWT secret keys
   - SMTP passwords

4. **Regular Backups:**
   - Database backups
   - Application files
   - Configuration files

5. **Monitor Logs:**
   ```bash
   # Set up log rotation
   sudo logrotate -f /etc/logrotate.conf
   ```

---

## Deployment Checklist

- [ ] Server prerequisites installed (.NET, Node.js, PostgreSQL, Nginx)
- [ ] Database created and migrations run
- [ ] appsettings.json configured with production values
- [ ] API built and published
- [ ] Systemd service created and running
- [ ] Frontend built for production
- [ ] Frontend files copied to web directory
- [ ] Nginx configured for frontend and API
- [ ] CORS origins updated in Program.cs
- [ ] SSL certificates installed (if using HTTPS)
- [ ] Firewall configured
- [ ] Services set to start on boot
- [ ] Logs monitored and verified
- [ ] Test all endpoints and frontend functionality

---

## Quick Reference

### Project Structure
```
/opt/encryptzERP/
├── CoreModule/
│   ├── API/encryptzERP/          # API source
│   └── UI/Admin/encryptz.Admin/   # Frontend source
├── api-publish/                   # Published API
└── /var/www/encryptz-admin/      # Frontend build output
```

### Important Files
- API Config: `/opt/encryptzERP/CoreModule/API/encryptzERP/appsettings.json`
- API Service: `/etc/systemd/system/encryptz-api.service`
- Frontend Config: `/opt/encryptzERP/CoreModule/UI/Admin/encryptz.Admin/src/environments/environment.prod.ts`
- Nginx Frontend: `/etc/nginx/sites-available/encryptz-admin`
- Nginx API: `/etc/nginx/sites-available/encryptz-api`

### Default Ports
- API: 5000 (internal, proxied by Nginx)
- Frontend: 80/443 (via Nginx)
- PostgreSQL: 5432

---

## Support

For issues or questions:
1. Check application logs: `sudo journalctl -u encryptz-api -f`
2. Check Nginx logs: `sudo tail -f /var/log/nginx/error.log`
3. Verify all services are running: `sudo systemctl status encryptz-api nginx postgresql`

