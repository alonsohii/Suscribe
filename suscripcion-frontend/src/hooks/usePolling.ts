import { useEffect } from 'react';

/**
 * Hook que ejecuta una función cada 2 segundos hasta que tenga éxito o falle 10 veces.
 * Útil para consultar el estado de algo que se está procesando.
 */
export const usePolling = (
  fn: () => Promise<any>,
  onSuccess: (data: any) => void,
  onError: () => void,
  enabled = true
) => {
  useEffect(() => {
    if (!enabled) return;

    let attempts = 0;

    // Intenta ejecutar la función
    const poll = async () => {
      try {
        const result = await fn();
        clearInterval(id); // Éxito → detiene el polling
        onSuccess(result);
      } catch {
        if (++attempts >= 10) { // Falló 10 veces → se rinde
          clearInterval(id);
          onError();
        }
      }
    };

    poll(); 
    const id = setInterval(poll, 2000); // Luego cada 2 segundos
    return () => clearInterval(id); 
  }, [enabled]);
};
