import { useState } from 'react';
import { TextField, Button, Alert, Container, Card, CardContent, CardHeader, MenuItem } from '@mui/material';
import { subscriptionService } from '../services/api';
import { useAsync } from '../hooks/useAsync';
import type { RegisterUserResponse, SubscribeResponse } from '../types/api';
import { subscriptionFormStyles } from '../styles/subscriptionFormStyles';

interface SubscriptionFormProps {
  onSuccess: (userId: number) => void;
}

interface FormData {
  name: string;
  email: string;
  paymentMethod: string;
}

interface FormErrors {
  name?: string;
  email?: string;
}

export const SubscriptionForm = ({ onSuccess }: SubscriptionFormProps) => {
  const [formData, setFormData] = useState<FormData>({ name: '', email: '', paymentMethod: 'credit_card' });
  const [errors, setErrors] = useState<FormErrors>({});
  const { status, error, execute, isLoading } = useAsync<RegisterUserResponse | SubscribeResponse>();

  const validate = () => {
    const newErrors: FormErrors = {};
    if (!formData.name.trim()) newErrors.name = 'El nombre es requerido';
    if (!formData.email.trim()) newErrors.email = 'El email es requerido';
    if (!/\S+@\S+\.\S+/.test(formData.email)) newErrors.email = 'Email invalido';
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      const user = await execute(subscriptionService.registerUser(formData.name, formData.email));
      await execute(subscriptionService.subscribe(user.userId, formData.paymentMethod));
      
      // Backend retorna 202 Accepted con status "Processing"
      // Mostrar dashboard inmediatamente, el Dashboard hara polling
      onSuccess(user.userId);
    } catch (err) {
      console.error('Error:', err);
    }
  };

  return (
    <Container maxWidth="sm" sx={subscriptionFormStyles.container}>
      <Card>
        <CardHeader title="Suscribete a Premium" sx={subscriptionFormStyles.cardHeader} />
        <CardContent>
          <form onSubmit={handleSubmit}>
            <TextField
              fullWidth
              label="Nombre"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              disabled={isLoading}
              error={!!errors.name}
              helperText={errors.name}
              margin="normal"
            />

            <TextField
              fullWidth
              label="Email"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              disabled={isLoading}
              error={!!errors.email}
              helperText={errors.email}
              margin="normal"
            />

            <TextField
              fullWidth
              select
              label="Metodo de Pago"
              value={formData.paymentMethod}
              onChange={(e) => setFormData({ ...formData, paymentMethod: e.target.value })}
              disabled={isLoading}
              margin="normal"
            >
              <MenuItem value="credit_card">Tarjeta de Crédito</MenuItem>
              <MenuItem value="paypal">PayPal</MenuItem>
              <MenuItem value="invalid_method">Método Inválido (Fallará)</MenuItem>
            </TextField>

            <Button
              fullWidth
              variant="contained"
              type="submit"
              disabled={isLoading}
              sx={subscriptionFormStyles.submitButton}
            >
              {isLoading ? 'Procesando...' : 'Suscribirse'}
            </Button>

            {status === 'success' && (
              <Alert severity="success" sx={subscriptionFormStyles.alert}>
                Suscripcion procesada! Consultando estado...
              </Alert>
            )}

            {error && (
              <Alert severity="error" sx={subscriptionFormStyles.alert}>
                {error}
              </Alert>
            )}
          </form>
        </CardContent>
      </Card>
    </Container>
  );
};
