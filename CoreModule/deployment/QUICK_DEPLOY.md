# Quick Deployment Guide

## Prerequisites Installation (One-time setup)

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET 8.0
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
export PATH=$PATH:$HOME/.dotnet
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc

# Install Node.js 18
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# Install PostgreSQL
sudo apt install postgresql postgresql-contrib -y
sudo systemctl enable postgresql
sudo systemctl start postgresql

# Install Nginx
sudo apt install nginx -y
sudo systemctl enable nginx
sudo systemctl start nginx
```

## Database Setup

```bash
sudo -u postgres psql
```

In PostgreSQL:
```sql
CREATE DATABASE encryptzERPCore;
CREATE USER encryptzuser WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE encryptzERPCore TO encryptzuser;
\q
```

## Deployment Steps

### 1. Clone/Upload Project

```bash
cd /opt
sudo mkdir -p encryptzERP
# Upload your project files or clone from Git
```

### 2. Configure API

```bash
cd /opt/encryptzERP/CoreModule/API/encryptzERP
sudo cp appsettings.example.json appsettings.json
sudo nano appsettings.json
```

Update:
- ConnectionStrings.DefaultConnection
- JwtSettings.SecretKey (generate strong key)
- JwtSettings.Issuer/Audience
- EmailSettings

### 3. Configure Frontend

```bash
cd /opt/encryptzERP/CoreModule/UI/Admin/encryptz.Admin
nano src/environments/environment.prod.ts
```

Update `apiUrl` to your production API URL.

### 4. Install Systemd Service

```bash
sudo cp /opt/encryptzERP/CoreModule/deployment/encryptz-api.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable encryptz-api
```

### 5. Install Nginx Configs

```bash
# Frontend
sudo cp /opt/encryptzERP/CoreModule/deployment/nginx-encryptz-admin.conf /etc/nginx/sites-available/encryptz-admin
sudo nano /etc/nginx/sites-available/encryptz-admin  # Update domain
sudo ln -s /etc/nginx/sites-available/encryptz-admin /etc/nginx/sites-enabled/

# API
sudo cp /opt/encryptzERP/CoreModule/deployment/nginx-encryptz-api.conf /etc/nginx/sites-available/encryptz-api
sudo nano /etc/nginx/sites-available/encryptz-api  # Update domain
sudo ln -s /etc/nginx/sites-available/encryptz-api /etc/nginx/sites-enabled/

# Test and reload
sudo nginx -t
sudo systemctl reload nginx
```

### 6. Update CORS in Program.cs

Edit `/opt/encryptzERP/CoreModule/API/encryptzERP/Program.cs`:
- Update `allowedOrigins` array with your production domains

### 7. Run Deployment

```bash
cd /opt/encryptzERP/CoreModule
sudo ./deployment/deploy.sh
```

### 8. Start Services

```bash
sudo systemctl start encryptz-api
sudo systemctl status encryptz-api
```

## Verify Deployment

```bash
# Check API
curl http://localhost:5000/api/health

# Check services
sudo systemctl status encryptz-api
sudo systemctl status nginx
sudo systemctl status postgresql

# Check logs
sudo journalctl -u encryptz-api -f
```

## SSL Setup (Optional but Recommended)

```bash
sudo apt install certbot python3-certbot-nginx -y
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com
sudo certbot --nginx -d api.yourdomain.com
```

## Common Commands

```bash
# Restart API
sudo systemctl restart encryptz-api

# View API logs
sudo journalctl -u encryptz-api -f

# Reload Nginx
sudo systemctl reload nginx

# Test Nginx config
sudo nginx -t

# Rebuild and redeploy
cd /opt/encryptzERP/CoreModule
sudo ./deployment/deploy.sh
```

