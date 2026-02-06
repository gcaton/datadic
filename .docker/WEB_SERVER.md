# Web Server Integration

## Overview

The DataDic Docker environment now includes an Nginx web server that automatically serves the generated documentation, providing an instant browsable interface.

## Components

### Nginx Alpine Container
- **Image**: `nginx:alpine` (minimal footprint, ~10MB)
- **Port**: 8080 (mapped to container port 80)
- **Auto-start**: Starts automatically with SQL Server
- **Restart policy**: Unless stopped manually

### Volume Mapping
- **Host**: `./output/` (generated documentation)
- **Container**: `/usr/share/nginx/html` (read-only)
- **Benefit**: Real-time updates - regenerate docs and refresh browser

## Features

### Configuration (`nginx.conf`)
- **Directory listing**: Enabled for browsing file structure
- **Security headers**: X-Frame-Options, X-Content-Type-Options, X-XSS-Protection
- **Static caching**: 1-hour cache for CSS, JS, images
- **Error handling**: Custom 404 page support
- **Logging**: Access and error logs available

### Access Points
- **Main documentation**: http://localhost:8080
- **Individual tables**: http://localhost:8080/tables/[schema]_[table].html
- **Users**: http://localhost:8080/users/[username].html
- **Jobs**: http://localhost:8080/jobs/[jobname].html

## Usage

### Starting the Web Server

```bash
# Start all containers (includes web server)
just init

# Or start without database initialization
just up
```

### Viewing Documentation

```bash
# Method 1: Use the helper command
just open-docs

# Method 2: Direct browser access
# Navigate to: http://localhost:8080
```

### Updating Documentation

```bash
# Regenerate documentation
just run

# Refresh browser to see changes
# No need to restart the web server!
```

### Viewing Logs

```bash
# Web server logs only
just web-logs

# All container logs
just logs-all

# SQL Server logs only
just logs
```

## Architecture

```
┌─────────────────────────────────────────────┐
│  Host Machine                               │
│                                             │
│  ┌────────────────┐      ┌───────────────┐ │
│  │   DataDic App  │      │ Docker Compose│ │
│  │                │      │               │ │
│  │ Generates HTML ├─────►│ SQL Server    │ │
│  │ to ./output/   │      │ :1433         │ │
│  └────────────────┘      │               │ │
│                          │ Nginx Server  │ │
│                          │ :8080         │ │
│                          │   ▲           │ │
│                          │   │           │ │
│                          │   └─Volume────┼─┼─ ./output/
│                          └───────────────┘ │
│                                             │
│  Browser: http://localhost:8080            │
└─────────────────────────────────────────────┘
```

## Benefits

1. **Instant Access**: No need to navigate file system
2. **Proper Rendering**: CSS and assets load correctly
3. **Easy Sharing**: Share `http://localhost:8080` with team
4. **Auto-Update**: Regenerate docs, refresh browser
5. **Portable**: Same experience on any machine running Docker

## Customization

### Changing Port

Edit `.docker/docker-compose.yml`:

```yaml
webserver:
  ports:
    - "9090:80"  # Change 8080 to 9090 (or any available port)
```

### Custom Nginx Configuration

Edit `.docker/nginx.conf` to customize:
- Cache duration
- Security headers
- Error pages
- Logging format

### Disable Web Server

Comment out or remove the `webserver` service in `docker-compose.yml`:

```yaml
# webserver:
#   image: nginx:alpine
#   ...
```

## Troubleshooting

### Port Already in Use

```bash
# Check what's using port 8080
sudo lsof -i :8080

# Either stop that process or change DataDic port in docker-compose.yml
```

### Documentation Not Showing

```bash
# Check if output directory exists and has content
ls -la output/

# Verify web server is running
just status

# Check web server logs
just web-logs

# Regenerate documentation
just run
```

### Permission Issues

```bash
# Ensure output directory is readable
chmod -R 755 output/

# Check Docker volume permissions
docker exec datadic-webserver ls -la /usr/share/nginx/html
```

## Production Considerations

For production deployment, consider:

1. **HTTPS**: Add TLS/SSL certificates
2. **Authentication**: Add basic auth or OAuth
3. **Reverse Proxy**: Place behind Traefik, Nginx Proxy Manager, or similar
4. **Caching**: Configure CDN or advanced caching strategies
5. **Monitoring**: Add health checks and monitoring
6. **Backup**: Regularly backup generated documentation

## Technical Details

### Container Specs
- **Image Size**: ~43MB (nginx:alpine)
- **Memory**: ~5-10MB typical usage
- **CPU**: Negligible
- **Startup Time**: < 1 second

### Network
- **Bridge Network**: Shared with SQL Server
- **Dependencies**: Starts after SQL Server
- **Isolation**: Read-only volume mount for security

### Files
- **docker-compose.yml**: Service definition
- **nginx.conf**: Nginx configuration
- **Volume**: `../output:/usr/share/nginx/html:ro`

## Future Enhancements

Potential additions:
- [ ] Dark mode toggle
- [ ] Search functionality
- [ ] PDF export endpoint
- [ ] API for programmatic access
- [ ] Live reload on file changes
- [ ] Authentication layer
- [ ] Metrics dashboard
