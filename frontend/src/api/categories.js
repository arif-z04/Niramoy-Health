import { apiClient } from './client.js';

export const categoriesApi = {
  async getCategories() {
    const res = await apiClient('/categories');
    return res.data;
  },

  async getCategoryBySlug(slug) {
    const res = await apiClient(`/categories/${slug}`);
    return res.data;
  },
};
