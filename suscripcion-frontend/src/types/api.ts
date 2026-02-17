export interface RegisterUserRequest {
  name: string;
  email: string;
}

export interface RegisterUserResponse {
  userId: number;
  message: string;
}

export interface SubscribeRequest {
  userId: number;
  paymentMethod: string;
}

export interface SubscribeResponse {
  status: string;
}

export interface Subscription {
  subscriptionId: number;
  userId: number;
  userEmail: string;
  status: 'Active' | 'Cancelled' | 'Expired' | 'PaymentFailed';
  createdAt: string;
}
