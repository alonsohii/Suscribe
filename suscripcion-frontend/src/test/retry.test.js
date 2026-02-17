describe('Lógica de Reintentos de API - CÓDIGO REAL', () => {
  let successHandler;
  let errorHandler;

  beforeAll(() => {
    const { subscriptionService } = require('../services/api');
    successHandler = global.mockAxiosInstance._successHandler;
    errorHandler = global.mockAxiosInstance._errorHandler;
  });

  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('debe pasar respuestas exitosas sin modificar', () => {
    const response = { data: 'test', status: 200 };
    const result = successHandler(response);
    expect(result).toEqual(response);
  });

  test('debe inicializar contador de reintentos en error', async () => {
    const error = {
      code: 'ECONNABORTED',
      config: {},
    };

    jest.spyOn(global.mockAxiosInstance, 'post').mockResolvedValueOnce({ data: 'success' });

    try {
      await errorHandler(error);
    } catch (e) {}

    expect(error.config.retry).toBe(1);
  });

  test('debe incrementar contador de reintentos', async () => {
    const error = {
      code: 'ECONNABORTED',
      config: { retry: 1 },
    };

    jest.spyOn(global.mockAxiosInstance, 'post').mockResolvedValueOnce({ data: 'success' });

    try {
      await errorHandler(error);
    } catch (e) {}

    expect(error.config.retry).toBe(2);
  });

  test('debe dejar de reintentar después de 3 intentos', async () => {
    const error = {
      code: 'ECONNABORTED',
      config: { retry: 3 },
    };

    await expect(errorHandler(error)).rejects.toEqual(error);
  });

  test('no debe reintentar errores que no son timeout', async () => {
    const error = {
      code: 'NETWORK_ERROR',
      config: { retry: 0 },
    };

    await expect(errorHandler(error)).rejects.toEqual(error);
  });
});
