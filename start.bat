@echo off
echo ========================================
echo   INICIANDO SISTEMA DE SUSCRIPCIONES
echo ========================================
echo.

echo Deteniendo procesos previos...
taskkill /FI "WindowTitle eq Backend API*" /T /F >nul 2>&1
taskkill /FI "WindowTitle eq Frontend React*" /T /F >nul 2>&1
docker-compose down >nul 2>&1
echo.

echo [1/5] Iniciando RabbitMQ y MySQL...
docker-compose up -d
echo RabbitMQ: http://localhost:15672 (guest/guest)
echo MySQL: localhost:3306 (suscripcion_user/suscripcion_pass)
echo.

echo Esperando a que MySQL este listo...
timeout /t 10 /nobreak >nul

echo [2/5] Restaurando Backend .NET...
cd Suscripcion.Api
dotnet restore
echo.

echo [3/5] Aplicando migraciones a la base de datos...
dotnet ef database update --project ../Suscripcion.Infrastructure --startup-project .
echo.

echo [4/5] Iniciando Backend .NET...
start "Backend API" cmd /k "dotnet run"
cd ..
echo Backend: http://localhost:5000
echo.

timeout /t 10 /nobreak >nul

echo [5/5] Iniciando Frontend React...
cd suscripcion-frontend
echo Instalando dependencias...
call npm install
start "Frontend React" cmd /k "npm run dev"
cd ..
echo Frontend: http://localhost:5173
echo.

echo ========================================
echo   SISTEMA INICIADO
echo ========================================
echo.
echo RabbitMQ:  http://localhost:15672
echo MySQL:     localhost:3306
echo Backend:   http://localhost:5000/swagger
echo Frontend:  http://localhost:5173
echo.
echo Presiona cualquier tecla para detener...
pause >nul

echo.
echo Deteniendo...
taskkill /FI "WindowTitle eq Backend API*" /T /F >nul 2>&1
taskkill /FI "WindowTitle eq Frontend React*" /T /F >nul 2>&1
docker-compose down
echo Detenido.
