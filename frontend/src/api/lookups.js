import { apiClient } from './client.js';

export const lookupsApi = {
  async getDivisions() {
    const res = await apiClient('/lookups/divisions');
    return res.data;
  },

  async getDistricts(divisionId = null) {
    const url = divisionId ? `/lookups/districts?divisionId=${divisionId}` : '/lookups/districts';
    const res = await apiClient(url);
    return res.data;
  },

  async getUpazilas(districtId = null) {
    const url = districtId ? `/lookups/upazilas?districtId=${districtId}` : '/lookups/upazilas';
    const res = await apiClient(url);
    return res.data;
  },

  async getPaymentMethods() {
    const res = await apiClient('/lookups/payment-methods');
    return res.data;
  },

  async getDeliveryMethods() {
    const res = await apiClient('/lookups/delivery-methods');
    return res.data;
  },

  async getDosageForms() {
    const res = await apiClient('/lookups/dosage-forms');
    return res.data;
  },

  async getManufacturers() {
    const res = await apiClient('/lookups/manufacturers');
    return res.data;
  },

  async getGenerics() {
    const res = await apiClient('/lookups/generics');
    return res.data;
  },
};

export const addressesApi = {
  async getAddresses() {
    const res = await apiClient('/addresses');
    return res.data;
  },

  async createAddress(data) {
    const res = await apiClient('/addresses', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    return res.data;
  },

  async updateAddress(id, data) {
    const res = await apiClient(`/addresses/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    return res.data;
  },

  async deleteAddress(id) {
    const res = await apiClient(`/addresses/${id}`, {
      method: 'DELETE',
    });
    return res.data;
  },

  async setDefaultAddress(id) {
    const res = await apiClient(`/addresses/${id}/default`, {
      method: 'POST',
    });
    return res.data;
  },
};
