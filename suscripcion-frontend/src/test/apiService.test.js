import { subscriptionService } from '../services/api';

describe('API Service', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('registerUser', () => {
    test('debe registrar usuario exitosamente', async () => {
      const mockResponse = { data: { userId: 1, message: 'Usuario registrado' } };
      global.mockAxiosInstance.post.mockResolvedValueOnce(mockResponse);

      const result = await subscriptionService.registerUser('Juan', 'juan@test.com');

      expect(global.mockAxiosInstance.post).toHaveBeenCalledWith('/api/user/register', {
        name: 'Juan',
        email: 'juan@test.com',
      });
      expect(result).toEqual(mockResponse.data);
    });

    test('debe lanzar error en registro fallido', async () => {
      global.mockAxiosInstance.post.mockRejectedValueOnce(new Error('Solicitud incorrecta'));

      await expect(subscriptionService.registerUser('', 'invalido'))
        .rejects.toThrow('Solicitud incorrecta');
    });

    test('debe registrar múltiples usuarios', async () => {
      const users = [
        { name: 'Usuario1', email: 'usuario1@test.com' },
        { name: 'Usuario2', email: 'usuario2@test.com' },
      ];

      for (const user of users) {
        global.mockAxiosInstance.post.mockResolvedValueOnce({ data: { userId: 1 } });
        await subscriptionService.registerUser(user.name, user.email);
      }

      expect(global.mockAxiosInstance.post).toHaveBeenCalledTimes(2);
    });
  });

  describe('subscribe', () => {
    test('debe crear suscripción exitosamente', async () => {
      const mockResponse = { data: { message: 'Suscripción en cola' } };
      global.mockAxiosInstance.post.mockResolvedValueOnce(mockResponse);

      const result = await subscriptionService.subscribe(1, 'credit_card');

      expect(global.mockAxiosInstance.post).toHaveBeenCalledWith('/api/suscripcion/subscribe', {
        userId: 1,
        paymentMethod: 'credit_card',
      });
      expect(result).toEqual(mockResponse.data);
    });

    test('debe lanzar error en suscripción fallida', async () => {
      global.mockAxiosInstance.post.mockRejectedValueOnce(new Error('Conflicto'));

      await expect(subscriptionService.subscribe(1, 'invalido'))
        .rejects.toThrow('Conflicto');
    });

    test('debe manejar diferentes métodos de pago', async () => {
      const methods = ['credit_card', 'paypal', 'bank_transfer'];

      for (const method of methods) {
        global.mockAxiosInstance.post.mockResolvedValueOnce({ data: { message: 'OK' } });
        await subscriptionService.subscribe(1, method);
      }

      expect(global.mockAxiosInstance.post).toHaveBeenCalledTimes(3);
    });
  });

  describe('getSubscription', () => {
    test('debe obtener suscripción exitosamente', async () => {
      const mockResponse = { data: { userId: 1, status: 'Activa' } };
      global.mockAxiosInstance.get.mockResolvedValueOnce(mockResponse);

      const result = await subscriptionService.getSubscription(1);

      expect(global.mockAxiosInstance.get).toHaveBeenCalledWith('/api/suscripcion/1');
      expect(result).toEqual(mockResponse.data);
    });

    test('debe lanzar error al no encontrar suscripción', async () => {
      global.mockAxiosInstance.get.mockRejectedValueOnce(new Error('No encontrado'));

      await expect(subscriptionService.getSubscription(999))
        .rejects.toThrow('No encontrado');
    });

    test('debe obtener múltiples suscripciones', async () => {
      const userIds = [1, 2, 3];

      for (const id of userIds) {
        global.mockAxiosInstance.get.mockResolvedValueOnce({ data: { userId: id } });
        await subscriptionService.getSubscription(id);
      }

      expect(global.mockAxiosInstance.get).toHaveBeenCalledTimes(3);
    });
  });
});
