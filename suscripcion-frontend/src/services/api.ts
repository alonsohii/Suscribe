import axios from 'axios';
import type { RegisterUserResponse, SubscribeResponse, Subscription } from '../types/api';

const getApiUrl = () => {
  return (typeof import.meta !== 'undefined' && import.meta.env?.VITE_API_URL) || process.env.VITE_API_URL || '/api';
};

const api = axios.create({
  baseURL: getApiUrl(),
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' }
});

// Reintenta automáticamente hasta 3 veces si hay timeout, esperando 1s, 2s y 3s entre intentos
api.interceptors.response.use(
  response => response,
  async error => {
    const config = error.config;
    config.retry = config.retry || 0;
    
    const isTimeout = error.code === 'ECONNABORTED';
    const canRetry = config.retry < 3;
    
    if (canRetry && isTimeout) {
      config.retry++;
      await new Promise(resolve => setTimeout(resolve, 1000 * config.retry));
      return api(config);
    }
    
    return Promise.reject(error);
  }
);

export const subscriptionService = {
  async registerUser(name: string, email: string): Promise<RegisterUserResponse> {
    const { data } = await api.post<RegisterUserResponse>('/user/register', { name, email });
    return data;
  },

  async subscribe(userId: number, paymentMethod: string): Promise<SubscribeResponse> {
    const { data } = await api.post<SubscribeResponse>('/suscripcion/subscribe', { userId, paymentMethod });
    return data;
  },

  async getSubscription(userId: number): Promise<Subscription> {
    const { data } = await api.get<Subscription>(`/suscripcion/${userId}`);
    return data;
  },

  async getAllSubscriptions(): Promise<Subscription[]> {
    const { data } = await api.get<Subscription[]>('/suscripcion');
    return data;
  },

  async cancelSubscription(userId: number): Promise<{ message: string }> {
    const { data } = await api.post<{ message: string }>(`/suscripcion/${userId}/cancel`);
    return data;
  }
};
