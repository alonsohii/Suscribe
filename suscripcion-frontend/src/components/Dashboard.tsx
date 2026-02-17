import { useEffect, useState } from 'react';
import { Container, Card, CardContent, CardHeader, Chip, Button, CircularProgress, Alert } from '@mui/material';
import { subscriptionService } from '../services/api';
import { useAsync } from '../hooks/useAsync';
import { usePolling } from '../hooks/usePolling';
import type { Subscription } from '../types/api';
import { dashboardStyles } from '../styles/dashboardStyles';

interface DashboardProps {
  userId: number;
  fromManage?: boolean;
}

export const Dashboard = ({ userId, fromManage = false }: DashboardProps) => {
  const { data, error, execute, isLoading } = useAsync<Subscription>();
  const [cancelling, setCancelling] = useState(false);
  const [pollingFailed, setPollingFailed] = useState(false);

  usePolling(
    async () => {
      try {
        return await subscriptionService.getSubscription(userId);
      } catch (err: any) {
        // Si es 404, significa que aún no se creó (sigue intentando)
        if (err.response?.status === 404) {
          throw err; // Reintenta
        }
        // Otros errores (500, network, etc.) también reintentan
        throw err;
      }
    },
    (result) => execute(Promise.resolve(result)),
    () => {
      setPollingFailed(true);
      execute(Promise.reject(new Error('La suscripción no se pudo crear después de 20 segundos. Verifica que el método de pago sea válido o recarga la página.')));
    },
    !!userId && !data && !pollingFailed
  );

  if (isLoading || (!data && !error && !pollingFailed)) {
    return (
      <Container maxWidth="sm" sx={dashboardStyles.loadingContainer}>
        <CircularProgress />
        <p>Procesando pago...</p>
        <p style={{ color: '#666', fontSize: '14px' }}>Esto puede tomar unos segundos</p>
      </Container>
    );
  }

  if (error) {
    return (
      <Container maxWidth="sm" sx={dashboardStyles.container}>
        <Alert severity="error">
          <strong>Error al procesar el pago</strong>
          <p>{error}</p>
          <p><strong>Posibles causas:</strong></p>
          <ul>
            <li>Método de pago inválido o rechazado</li>
            <li>Error en el servidor</li>
          </ul>
          <Button variant="contained" onClick={() => window.location.href = '/'} sx={dashboardStyles.retryButton}>
            Volver a Intentar
          </Button>
        </Alert>
      </Container>
    );
  }

  if (!data) return null;

  const statusConfig = {
    Active: { color: 'success' as const, text: 'ACTIVA' },
    Cancelled: { color: 'error' as const, text: 'CANCELADA' },
    Expired: { color: 'warning' as const, text: 'EXPIRADA' },
    PaymentFailed: { color: 'error' as const, text: 'PAGO RECHAZADO' }
  };

  const config = statusConfig[data.status] || statusConfig.Active;

  const handleCancel = async () => {
    if (!window.confirm('¿Estás seguro de que deseas cancelar tu suscripción?')) return;
    
    setCancelling(true);
    try {
      await subscriptionService.cancelSubscription(userId);
      window.location.reload();
    } catch (err) {
      alert('Error al cancelar la suscripción. Intenta de nuevo.');
      setCancelling(false);
    }
  };

  return (
    <Container maxWidth="sm" sx={dashboardStyles.container}>
      <h2>{fromManage && data.userEmail ? `Suscripción de ${data.userEmail}` : 'Mi Cuenta'}</h2>
      
      <Card sx={dashboardStyles.card}>
        <CardHeader title="Estado de Suscripcion" />
        <CardContent>
          <div style={dashboardStyles.chipContainer}>
            <Chip label={config.text} color={config.color} size="large" />
          </div>

          <p><strong>Plan:</strong> Premium</p>
          <p><strong>Desde:</strong> {new Date(data.createdAt).toLocaleDateString('es-ES')}</p>
          <p><strong>ID Usuario:</strong> {data.userId}</p>

          {data.status === 'Active' && (
            <Button 
              variant="contained" 
              color="error" 
              fullWidth 
              sx={dashboardStyles.cancelButton}
              onClick={handleCancel}
              disabled={cancelling}
            >
              {cancelling ? 'Cancelando...' : 'Cancelar Suscripcion'}
            </Button>
          )}
        </CardContent>
      </Card>

      {data.status === 'PaymentFailed' && (
        <Alert severity="error" sx={dashboardStyles.alert}>
          <strong>Pago Rechazado</strong>
          <p>Tu metodo de pago fue rechazado. Por favor, intenta con otro metodo de pago.</p>
        </Alert>
      )}
    </Container>
  );
};
