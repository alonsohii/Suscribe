@echo off
echo ========================================
echo   Deteniendo Sistema de Suscripciones
echo ========================================
echo.

echo Deteniendo RabbitMQ...
docker-compose down

echo Deteniendo Backend...
taskkill /FI "WINDOWTITLE eq Backend API*" /T /F >nul 2>&1

echo Deteniendo Frontend...
taskkill /FI "WINDOWTITLE eq Frontend React*" /T /F >nul 2>&1

echo.
echo Todos los servicios han sido detenidos.
pause
