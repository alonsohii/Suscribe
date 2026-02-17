import { useState } from 'react';

// Estados posibles: inicial, cargando, éxito o error
type Status = 'inicial' | 'loading' | 'success' | 'error';

/**
 * Hook para manejar llamadas asíncronas fácilmente.
 * Te da el estado (loading, success, error), los datos y funciones para ejecutar promesas.
 */
export const useAsync = <T = any>() => {
  const [status, setStatus] = useState<Status>('inicial');
  const [data, setData] = useState<T | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Ejecuta una promesa y maneja automáticamente loading, success y error
  const execute = async (promise: Promise<T>): Promise<T> => {
    setStatus('loading');
    setError(null);
    
    try {
      const result = await promise;
      setData(result);
      setStatus('success');
      return result;
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Error desconocido');
      setStatus('error');
      throw err;
    }
  };

  // Vuelve todo al estado inicial
  const reset = () => {
    setStatus('inicial');
    setData(null);
    setError(null);
  };

  return { status, data, error, execute, reset, isLoading: status === 'loading' };
};
