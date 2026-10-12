import { apiClient } from './client.js';

export const paymentsApi = {
  async initiatePayment(orderId) {
    const res = await apiClient('/payments/initiate', {
      method: 'POST',
      body: JSON.stringify({ orderId }),
    });
    return res.data;
  },

  async executePayment(paymentPublicId, gatewayPaymentId = null, providerTrxId = null) {
    const res = await apiClient('/payments/execute', {
      method: 'POST',
      body: JSON.stringify({ paymentPublicId, gatewayPaymentId, providerTrxId }),
    });
    return res.data;
  },
};
