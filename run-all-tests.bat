@echo off
echo ========================================
echo   EJECUTANDO TODOS LOS TESTS
echo ========================================
echo.

echo [1/2] Tests Backend (.NET)...
echo.
dotnet test Suscripcion.Test/Suscripcion.Test.csproj --collect:"XPlat Code Coverage"

echo.
echo ========================================
echo [2/2] Tests Frontend (Docker)...
echo ========================================
echo.

cd suscripcion-frontend
docker build -f Dockerfile.test -t suscripcion-frontend-tests .
docker run --rm suscripcion-frontend-tests
cd ..

echo.
echo ========================================
echo   TODOS LOS TESTS COMPLETADOS
echo ========================================
pause
