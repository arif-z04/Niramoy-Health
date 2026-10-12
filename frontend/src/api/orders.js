import { apiClient } from './client.js';

export const ordersApi = {
  async checkout(checkoutData) {
    const res = await apiClient('/orders/checkout', {
      method: 'POST',
      body: JSON.stringify(checkoutData),
    });
    return res.data || res;
  },

  async getMyOrders(page = 1, pageSize = 10, status = null) {
    const query = new URLSearchParams({ pageNumber: page, page, pageSize });
    if (status) query.append('status', status);
    const res = await apiClient(`/orders?${query.toString()}`);
    return res.data || res;
  },

  async getOrderById(id) {
    const res = await apiClient(`/orders/${id}`);
    return res.data || res;
  },

  async cancelOrder(id, reason) {
    const res = await apiClient(`/orders/${id}/cancel`, {
      method: 'POST',
      body: JSON.stringify({ reason }),
    });
    return res.data || res;
  },
};
