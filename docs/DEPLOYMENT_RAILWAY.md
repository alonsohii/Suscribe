# 🚀 Guía de Deployment - Railway

## 📋 Requisitos Previos

1. Cuenta en [Railway.app](https://railway.app)
2. Repositorio en GitHub
3. Railway CLI instalado (opcional)

---

## 🔧 Configuración Inicial

### 1. Crear Proyecto en Railway

```bash
# Opción 1: Desde la web
1. Ve a https://railway.app
2. Click en "New Project"
3. Selecciona "Deploy from GitHub repo"
4. Conecta tu repositorio

# Opción 2: Desde CLI
railway login
railway init
railway link
```

### 2. Agregar Servicios

Railway detectará automáticamente:
- ✅ **API Backend** (Dockerfile en Suscripcion.Api/)
- ✅ **Frontend React** (package.json en suscripcion-frontend/)

Debes agregar manualmente:
- 🐰 **RabbitMQ** (desde Railway Marketplace)
- 🗄️ **PostgreSQL** (desde Railway Marketplace)

---

## 🔐 Variables de Entorno

### Backend (API)

```bash
# Base de datos
ConnectionStrings__DefaultConnection=postgresql://user:pass@host:5432/dbname

# RabbitMQ
RabbitMq__HostName=rabbitmq-host
RabbitMq__Port=5672
RabbitMq__UserName=guest
RabbitMq__Password=guest
RabbitMq__QueueName=subscription-queue

# CORS
AllowedOrigins=https://tu-frontend.railway.app

# ASP.NET
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
```

### Frontend (React)

```bash
VITE_API_URL=https://tu-api.railway.app
```

---

## 📦 Configuración de Servicios

### API Backend

**railway.json** (ya creado en la raíz):
```json
{
  "build": {
    "builder": "DOCKERFILE",
    "dockerfilePath": "Suscripcion.Api/Dockerfile"
  },
  "deploy": {
    "startCommand": "dotnet Suscripcion.Api.dll",
    "healthcheckPath": "/health",
    "restartPolicyType": "ON_FAILURE"
  }
}
```

### Frontend

Railway detecta automáticamente `package.json` y ejecuta:
```bash
npm install
npm run build
npm run preview  # o configura start command
```

---

## 🔄 CI/CD con GitHub Actions

El archivo `.github/workflows/ci-cd.yml` ya está configurado.

### Configurar Secret en GitHub

1. Ve a tu repositorio en GitHub
2. Settings → Secrets and variables → Actions
3. New repository secret:
   - Name: `RAILWAY_TOKEN`
   - Value: Tu token de Railway (obtenerlo en railway.app/account/tokens)

### Flujo Automático

```
Push a main → Tests → Deploy a Railway
     ↓           ↓            ↓
   Commit    Backend      Producción
             Frontend
```

---

## 🗄️ Migración de Base de Datos

### Opción 1: Desde Railway CLI

```bash
railway run dotnet ef database update --project Suscripcion.Infrastructure
```

### Opción 2: Automático en Startup

Agregar en `Program.cs`:
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```

---

## 🌐 Dominios Personalizados

### Configurar Dominio

1. En Railway, ve a tu servicio
2. Settings → Domains
3. Generate Domain (Railway te da uno gratis)
4. O agrega tu dominio personalizado

### Actualizar CORS

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://tu-dominio.railway.app",
            "https://tu-dominio-custom.com"
        )
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});
```

---

## 📊 Monitoreo

### Logs en Railway

```bash
# Ver logs en tiempo real
railway logs

# Logs de un servicio específico
railway logs --service api
```

### Métricas

Railway Dashboard muestra:
- CPU usage
- Memory usage
- Network traffic
- Request count

---

## 🔧 Troubleshooting

### Error: "Application failed to start"

```bash
# Verificar logs
railway logs

# Verificar variables de entorno
railway variables

# Verificar build
railway run dotnet --version
```

### Error: "Database connection failed"

1. Verifica que PostgreSQL esté corriendo
2. Verifica ConnectionString en variables de entorno
3. Verifica que las migraciones se aplicaron

### Error: "RabbitMQ connection timeout"

1. Verifica que RabbitMQ esté corriendo
2. Verifica hostname y puerto en variables de entorno
3. Verifica que el servicio tenga acceso a RabbitMQ


---

## 🔗 Links Útiles

- [Railway Docs](https://docs.railway.app)
- [Railway CLI](https://docs.railway.app/develop/cli)
- [Railway Templates](https://railway.app/templates)
- [Railway Discord](https://discord.gg/railway)

---

**Última actualización:** 2025
