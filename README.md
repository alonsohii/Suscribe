# 📋 Decisiones de Arquitectura - Sistema de Suscripciones

## 🎯 Decisiones Tecnológicas Principales

### 1. **C# como Lenguaje Principal**

**Decisión:** Usar C# para el backend

**Razones:**
- **Ecosistema maduro**: .NET ofrece un framework robusto y probado en producción
- **Rendimiento**: C# con .NET 8 ofrece excelente rendimiento y optimizaciones
- **Tipado fuerte**: Reduce errores en tiempo de compilación
- **Comunidad**: Gran comunidad y abundante documentación
- **Integración empresarial**: Ampliamente usado en entornos corporativos

---

### 2. **.NET 8 (LTS)**

**Decisión:** Usar .NET 8 en lugar de versiones anteriores

**Razones:**
- **Long Term Support (LTS)**: 
- **Rendimiento mejorado**: Hasta 20% más rápido que .NET 6
- **Características modernas**: 
  - Minimal APIs mejoradas
  - Mejor manejo de JSON
- **Seguridad**: Actualizaciones de seguridad garantizadas

---

### 3. **Arquitectura en Capas (Clean Architecture)**

**Decisión:** Implementar arquitectura en 4 capas

```
┌─────────────────────────────────────┐
│         Suscripcion.Api             │  ← Capa de Presentación
├─────────────────────────────────────┤
│      Suscripcion.Application        │  ← Capa de Aplicación
├─────────────────────────────────────┤
│     Suscripcion.Infrastructure      │  ← Capa de Infraestructura
├─────────────────────────────────────┤
│       Sucripcion.Domain             │  ← Capa de Dominio
└─────────────────────────────────────┘
```

**Estructura de carpetas:**

```
Suscripcion/
├── Suscripcion.Api/              # API REST (.NET 8)
│   ├── Controllers/
│   ├── Middleware/
│   └── Program.cs
├── Suscripcion.Application/      # Lógica de aplicación
│   ├── DTOs/
│   ├── Suscripciones/
│   ├── Users/
│   ├── Services/
│   └── Messages/
├── Suscripcion.Infrastructure/   # Implementaciones técnicas
│   ├── Consumers/
│   ├── Messaging/
│   ├── Payments/
│   ├── Webhooks/
│   ├── Persistence/
│   └── Migrations/
├── Sucripcion.Domain/            # Entidades de negocio
│   ├── Entities/
│   ├── Enums/
│   ├── ValueObjects/
│   └── Interfaces/
├── Suscripcion.Test/             # Tests (xUnit)
│   ├── UnitTests/
│   └── IntegrationTests/
├── suscripcion-frontend/         # React + TypeScript + Vite
│   └── src/
│       ├── components/
│       ├── services/
│       ├── styles/
│       └── types/
├── docs/
├── docker-compose.yml
├── start.bat
└── Suscripcion.slnx
```

**Razones:**
- **Separación de responsabilidades**: Cada capa tiene un propósito claro
- **Testabilidad**: Fácil crear tests unitarios sin dependencias externas
- **Mantenibilidad**: Cambios en una capa no afectan a las demás
- **Escalabilidad**: Fácil agregar nuevas funcionalidades
- **Independencia de frameworks**: El dominio no depende de tecnologías específicas
- **Regla de dependencia**: Las capas internas no conocen las externas

**Responsabilidades por capa:**

| Capa | Responsabilidad | Ejemplos |
|------|----------------|----------|
| **Domain** | Entidades y lógica de negocio | `Subscription`, `User`, `Email` |
| **Application** | Casos de uso y DTOs | `SubscriptionHandler`, `RegisterUserHandler` |
| **Infrastructure** | Implementaciones técnicas | `AppDbContext`, `RabbitMqPublisher` |
| **Api** | Endpoints y configuración | `SuscripcionController`, `Program.cs` |

---

### 4. **RabbitMQ para Mensajería Asíncrona**

**Decisión:** Usar RabbitMQ como message broker

**Razones:**
- **Procesamiento asíncrono**: Desacopla la creación de suscripción del procesamiento de pago
- **Resiliencia**: Si el pago falla, el mensaje puede reintentarse
- **Escalabilidad**: Múltiples consumidores pueden procesar mensajes en paralelo
- **Dead Letter Queue**: Manejo automático de mensajes fallidos
- **Confiabilidad**: Garantiza entrega de mensajes (durabilidad)
- **Experiencia de usuario**: Respuesta inmediata al usuario sin esperar el pago

**Flujo implementado:**
```
Usuario → API → RabbitMQ → Consumer → Payment Gateway → Webhook
         (202)   (queue)   (async)      (validate)      (notify)
```

**Alternativas consideradas:**
- ❌ **Procesamiento síncrono**: Bloquearía la respuesta al usuario
- ❌ **Azure Service Bus**: Mayor costo, overkill para este caso
- ❌ **Kafka**: Demasiado complejo para el volumen esperado

---

### 5. **Entity Framework Core como ORM**

**Decisión:** Usar EF Core para acceso a datos

**Razones:**
- **Productividad**: Menos código boilerplate que ADO.NET
- **Migraciones**: Control de versiones del esquema de base de datos
- **LINQ**: Queries type-safe y legibles
- **Integración**: Funciona perfectamente con .NET 8

---

### 6. **MySQL como Base de Datos**

**Decisión:** Usar MySQL (Railway en producción)

**Razones:**
- **Transacciones ACID**: Garantiza consistencia de datos
- **Relaciones**: Modelo relacional claro (User → Subscription)
- **Madurez**: Base de datos probada y confiable
- **Integración**: Excelente soporte con EF Core
- **Cloud-friendly**: Fácil deployment en Railway, Google Cloud, etc.
- **Open Source**: Sin costos de licenciamiento

**Alternativas consideradas:**
- ❌ **SQL Server**: Requiere licencia, más costoso en cloud
- ❌ **PostgreSQL**: Funcionalidad similar, MySQL más común
- ❌ **MongoDB**: No necesitamos flexibilidad de esquema

---

### 7. **Docker para Infraestructura**

**Decisión:** Usar Docker Compose para orquestar servicios

**Razones:**
- **Reproducibilidad**: Mismo entorno en desarrollo y producción
- **Aislamiento**: Cada servicio en su propio contenedor
- **Facilidad**: Un comando levanta toda la infraestructura
- **Portabilidad**: Funciona en cualquier sistema con Docker
- **Versionado**: Configuración como código (docker-compose.yml)

**Servicios dockerizados:**
- RabbitMQ (con management UI)
- MySQL
- API Backend
- Frontend React

---

### 8. **React + TypeScript para Frontend**

**Decisión:** Usar React con TypeScript

**Razones:**
- **Popularidad**: Framework más usado, fácil encontrar desarrolladores
- **Componentes**: Reutilización y modularidad
- **TypeScript**: Tipado fuerte reduce errores
- **Ecosistema**: Material-UI para componentes profesionales


---

### 9. **Vite como Build Tool**

**Decisión:** Usar Vite en lugar de Create React App

**Razones:**

- **TypeScript**: Soporte nativo sin configuración adicional

---

### 10. **Axios con Retry Pattern**

**Decisión:** Implementar reintentos automáticos en el cliente HTTP

**Razones:**
- **Resiliencia**: Maneja fallos de red transitorios
- **Backoff exponencial**: Espera 1s, 2s, 3s entre reintentos
- **Configurable**: Hasta 3 reintentos antes de mostrar error

**Implementación:**
```typescript
// Interceptor que reintenta automáticamente en timeouts
api.interceptors.response.use(
  response => response,
  async error => {
    if (error.config.retry < 3 && error.code === 'ECONNABORTED') {
      error.config.retry++;
      await delay(1000 * error.config.retry);
      return api(error.config);
    }
    return Promise.reject(error);
  }
);
```

---

### 11. **Polly para Resiliencia**

**Decisión:** Usar Polly para políticas de reintentos

**Razones:**
- **Resiliencia**: Maneja fallos transitorios automáticamente
- **Configurabilidad**: Políticas de retry personalizables
- **Integración**: Funciona perfectamente con .NET
- **Patrones**: Circuit breaker, timeout, retry

**Implementado en:**
- Conexión a RabbitMQ (5 reintentos, 3s delay)
- Llamadas HTTP a webhooks

---

### 12. **xUnit + FluentAssertions para Testing**

**Decisión:** Usar xUnit como framework de testing

**Razones:**
- **Moderno**: Diseñado para .NET Core desde el inicio
- **Extensibilidad**: Fácil crear fixtures y helpers
- **FluentAssertions**: Assertions legibles y expresivas

**Cobertura:**
- Tests unitarios: Lógica de negocio aislada
- Tests de integración: API completa con base de datos

---

## ⚠️ Manejo de Errores y Transacciones

### ¿Qué pasa si la DB cae a mitad de la transacción?

**Estrategia implementada:**

1. **Transacciones explícitas con rollback:**
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try {
    await _repository.SaveChangesAsync();
    await transaction.CommitAsync();
} catch {
    await transaction.RollbackAsync();
    throw;
}
```

2. **RabbitMQ con ACK/NACK:**
- Si la transacción falla, el mensaje NO se confirma (NACK)
- RabbitMQ reintenta el mensaje automáticamente
- Garantiza que no se pierden suscripciones

3. **Dead Letter Queue:**
- Mensajes que fallan múltiples veces van a cola de errores
- Permite análisis manual y reprocesamiento

4. **Idempotencia:**
- Validación de duplicados antes de crear suscripción
- Evita crear múltiples suscripciones por el mismo usuario

**Escenarios cubiertos:**
- ✅ DB cae durante transacción → Rollback automático
- ✅ Webhook falla → Reintento con Polly (3 intentos)
- ✅ RabbitMQ no disponible → API continúa funcionando
- ✅ Pago falla → Suscripción con estado PaymentFailed

---

## 🚀 Escalabilidad: 10,000 Suscripciones/Segundo

### Estrategia de Escalamiento

**Arquitectura actual (hasta ~100 req/s):**
```
Cliente → API → RabbitMQ → Consumer → DB
```

## 🔄 CI/CD Pipeline

### GitHub Actions

**Pipeline implementado:**

1. **Test Stage (en cada push):**
```yaml
- Backend: dotnet test con cobertura
- Frontend: npm test con Jest
- Cobertura mínima: 80% backend, 70% frontend
```

2. **Deploy Stage (solo en main):**
```yaml
- Railway detecta push a main
- Build automático con Dockerfile
- Deploy a producción
- Health check antes de activar
```

**Ventajas:**
- ✅ Tests automáticos en cada PR
- ✅ Deploy automático al mergear a main
- ✅ Rollback fácil si falla health check
- ✅ Historial de deploys en Railway

### Railway Deployment

**Configuración:**
- **Backend**: Dockerfile en `Suscripcion.Api/`
- **Frontend**: Nixpacks (detección automática de Node.js)
- **Variables de entorno**: Configuradas en Railway dashboard
- **Servicios**: MySQL y RabbitMQ como servicios de Railway

**Flujo de deployment:**
```
Git push → GitHub → Railway Webhook → Build → Deploy → Health Check → Live
```

---

## 🔒 Decisiones de Seguridad

### CORS Configurado
- Permite requests desde el frontend (localhost:5173)
- Preparado para agregar dominios de producción

### Validaciones
- Validación de email con regex
- Límites de longitud en campos
- Validación de datos en múltiples capas

---

## 📊 Decisiones de Observabilidad

### Logging
- Console logging para desarrollo
- Preparado para agregar Serilog/Application Insights

### Debugging
- Mensajes de debug en consumer para troubleshooting
- Middleware de manejo global de excepciones

---

## 🚀 Decisiones de Deployment

### Ejecución Local

**Inicio rápido con un solo comando:**
```bash
start.bat
```

Este script automatiza:
1. Levanta RabbitMQ y MySQL con Docker Compose
2. Restaura dependencias del backend (.NET)
3. Aplica migraciones a la base de datos
4. Inicia el backend en http://localhost:5000
5. Instala dependencias e inicia el frontend en http://localhost:5173

**URLs disponibles:**
- Frontend: http://localhost:5173
- Backend API: http://localhost:5000/swagger
- RabbitMQ Management: http://localhost:15672 (guest/guest)
- MySQL: localhost:3306 (suscripcion_user/suscripcion_pass)

**Detener todo:**
Presiona cualquier tecla en la ventana de `start.bat` o ejecuta:
```bash
stop.bat
```

### Scripts de Inicio
- `start.bat`: Levanta toda la infraestructura
- `stop.bat`: Detiene todos los servicios
- `run-all-tests.bat`: Ejecuta todos los tests

### Configuración
- `appsettings.json` para configuración base
- `appsettings.Development.json` para desarrollo
- Variables de entorno para producción

---

## 📈 Escalabilidad Futura

### Preparado para:
- **Múltiples consumidores**: RabbitMQ permite escalar horizontalmente
- **Caché**: Redis puede agregarse fácilmente

---

### DRY (Don't Repeat Yourself)
- Reutilización de DTOs
- Helpers compartidos
- Configuración centralizada


---

**Versión del sistema:** 1.0
