import { apiClient } from './client.js';

export const productsApi = {
  async getProducts(params = {}) {
    const query = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        query.append(key, value);
      }
    });
    const res = await apiClient(`/products?${query.toString()}`);
    return res?.data || res || { items: [] };
  },

  async getProductById(id) {
    const res = await apiClient(`/products/${id}`);
    return res.data;
  },

  async getProductBySlug(slug) {
    const res = await apiClient(`/products/slug/${slug}`);
    return res.data;
  },

  async getFeatured(limit = 10) {
    const res = await apiClient(`/products/featured?limit=${limit}`);
    return res.data;
  },

  async getDiscounted(limit = 10) {
    const res = await apiClient(`/products/discounted?limit=${limit}`);
    return res.data;
  },
};
