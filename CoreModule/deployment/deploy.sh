#!/bin/bash

# encryptzERP Deployment Script
# This script automates the deployment process on Linux servers

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
PROJECT_ROOT="/opt/encryptzERP"
API_DIR="$PROJECT_ROOT/CoreModule/API/encryptzERP"
FRONTEND_DIR="$PROJECT_ROOT/CoreModule/UI/Admin/encryptz.Admin"
API_PUBLISH_DIR="$PROJECT_ROOT/api-publish"
FRONTEND_DIST_DIR="/var/www/encryptz-admin"
SERVICE_USER="www-data"
SERVICE_NAME="encryptz-api"

# Functions
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

check_command() {
    if ! command -v $1 &> /dev/null; then
        print_error "$1 is not installed. Please install it first."
        exit 1
    fi
}

# Check if running as root for certain operations
check_root() {
    if [ "$EUID" -ne 0 ]; then 
        print_error "Please run as root (use sudo)"
        exit 1
    fi
}

# Step 1: Check prerequisites
check_prerequisites() {
    print_info "Checking prerequisites..."
    check_command dotnet
    check_command node
    check_command npm
    check_command psql
    check_command nginx
    
    print_info "All prerequisites are installed."
}

# Step 2: Deploy API
deploy_api() {
    print_info "Deploying API..."
    
    if [ ! -d "$API_DIR" ]; then
        print_error "API directory not found: $API_DIR"
        exit 1
    fi
    
    cd "$API_DIR"
    
    # Restore dependencies
    print_info "Restoring NuGet packages..."
    dotnet restore
    
    # Build
    print_info "Building API..."
    dotnet build --configuration Release
    
    # Publish
    print_info "Publishing API..."
    dotnet publish --configuration Release --output "$API_PUBLISH_DIR"
    
    # Set permissions
    print_info "Setting permissions..."
    chown -R $SERVICE_USER:$SERVICE_USER "$API_PUBLISH_DIR"
    chmod +x "$API_PUBLISH_DIR/encryptzERP.dll"
    
    print_info "API deployment completed."
}

# Step 3: Deploy Frontend
deploy_frontend() {
    print_info "Deploying Frontend..."
    
    if [ ! -d "$FRONTEND_DIR" ]; then
        print_error "Frontend directory not found: $FRONTEND_DIR"
        exit 1
    fi
    
    cd "$FRONTEND_DIR"
    
    # Install dependencies
    print_info "Installing npm packages..."
    npm install
    
    # Build
    print_info "Building frontend for production..."
    npm run build -- --configuration production
    
    # Create web directory if it doesn't exist
    mkdir -p "$FRONTEND_DIST_DIR"
    
    # Copy build output
    print_info "Copying build output..."
    cp -r dist/encryptz.admin/* "$FRONTEND_DIST_DIR/"
    
    # Set permissions
    print_info "Setting permissions..."
    chown -R $SERVICE_USER:$SERVICE_USER "$FRONTEND_DIST_DIR"
    chmod -R 755 "$FRONTEND_DIST_DIR"
    
    print_info "Frontend deployment completed."
}

# Step 4: Restart Services
restart_services() {
    print_info "Restarting services..."
    
    # Restart API service
    if systemctl is-active --quiet $SERVICE_NAME; then
        print_info "Restarting $SERVICE_NAME..."
        systemctl restart $SERVICE_NAME
    else
        print_warning "$SERVICE_NAME is not running. Starting it..."
        systemctl start $SERVICE_NAME
    fi
    
    # Reload Nginx
    print_info "Reloading Nginx..."
    nginx -t && systemctl reload nginx
    
    print_info "Services restarted."
}

# Step 5: Verify Deployment
verify_deployment() {
    print_info "Verifying deployment..."
    
    # Check API service
    if systemctl is-active --quiet $SERVICE_NAME; then
        print_info "✓ API service is running"
    else
        print_error "✗ API service is not running"
        systemctl status $SERVICE_NAME
    fi
    
    # Check Nginx
    if systemctl is-active --quiet nginx; then
        print_info "✓ Nginx is running"
    else
        print_error "✗ Nginx is not running"
    fi
    
    # Check API endpoint (if accessible)
    if curl -f -s http://localhost:5000/api/health > /dev/null 2>&1; then
        print_info "✓ API is responding"
    else
        print_warning "⚠ API health check failed (this might be normal if health endpoint doesn't exist)"
    fi
}

# Main execution
main() {
    print_info "Starting deployment process..."
    
    # Check if running as root for service operations
    if [ "$EUID" -ne 0 ]; then
        print_warning "Not running as root. Some operations may require sudo."
    fi
    
    # Run deployment steps
    check_prerequisites
    deploy_api
    deploy_frontend
    
    if [ "$EUID" -eq 0 ]; then
        restart_services
        verify_deployment
    else
        print_warning "Skipping service restart (requires root). Please run manually:"
        echo "  sudo systemctl restart $SERVICE_NAME"
        echo "  sudo systemctl reload nginx"
    fi
    
    print_info "Deployment completed!"
}

# Parse command line arguments
case "${1:-}" in
    api)
        check_prerequisites
        deploy_api
        if [ "$EUID" -eq 0 ]; then
            restart_services
        fi
        ;;
    frontend)
        check_prerequisites
        deploy_frontend
        if [ "$EUID" -eq 0 ]; then
            systemctl reload nginx
        fi
        ;;
    *)
        main
        ;;
esac

