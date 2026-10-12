import { apiClient } from './client.js';

export const adminApi = {
  async getDashboard() {
    const res = await apiClient('/admin/dashboard');
    return res.data;
  },

  async getOrders(page = 1, pageSize = 20, status = null) {
    const query = new URLSearchParams({ page, pageSize });
    if (status) query.append('status', status);
    const res = await apiClient(`/admin/orders?${query.toString()}`);
    return res.data;
  },

  async updateOrderStatus(id, status, notes = '') {
    const res = await apiClient(`/admin/orders/${id}/status`, {
      method: 'PUT',
      body: JSON.stringify({ status, notes }),
    });
    return res.data;
  },

  async getPrescriptions(page = 1, pageSize = 20, status = null) {
    const query = new URLSearchParams({ page, pageSize });
    if (status) query.append('status', status);
    const res = await apiClient(`/admin/prescriptions?${query.toString()}`);
    return res.data;
  },

  async getInventory(page = 1, pageSize = 20, lowStockOnly = false) {
    const query = new URLSearchParams({ page, pageSize, lowStockOnly });
    const res = await apiClient(`/admin/inventory?${query.toString()}`);
    return res.data;
  },

  async adjustInventory(productId, quantityChange, reason, referenceNote = '') {
    const res = await apiClient('/admin/inventory/adjust', {
      method: 'POST',
      body: JSON.stringify({ productId, quantityChange, reason, referenceNote }),
    });
    return res.data;
  },

  async createProduct(data) {
    const res = await apiClient('/admin/products', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    return res.data;
  },

  async updateProduct(id, data) {
    const res = await apiClient(`/admin/products/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    return res.data;
  },
};

export const supportApi = {
  async getTickets(page = 1, pageSize = 20) {
    const res = await apiClient(`/support/tickets?page=${page}&pageSize=${pageSize}`);
    return res.data;
  },

  async getTicketById(id) {
    const res = await apiClient(`/support/tickets/${id}`);
    return res.data;
  },

  async createTicket(subject, message, priority = 'medium', orderId = null) {
    const res = await apiClient('/support/tickets', {
      method: 'POST',
      body: JSON.stringify({ subject, message, priority, orderId }),
    });
    return res.data;
  },

  async addMessage(ticketId, message) {
    const res = await apiClient(`/support/tickets/${ticketId}/messages`, {
      method: 'POST',
      body: JSON.stringify({ message }),
    });
    return res.data;
  },

  async updateStatus(ticketId, status) {
    const res = await apiClient(`/support/tickets/${ticketId}/status`, {
      method: 'PUT',
      body: JSON.stringify({ status }),
    });
    return res.data;
  },
};
