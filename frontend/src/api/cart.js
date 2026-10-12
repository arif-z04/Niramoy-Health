import { apiClient, getOrCreateGuestToken, clearGuestToken } from './client.js';

export const cartApi = {
  async getCart() {
    const res = await apiClient('/cart');
    return res.data;
  },

  async addItem(productId, quantity = 1, variantId = null) {
    const res = await apiClient('/cart/items', {
      method: 'POST',
      body: JSON.stringify({ productId, quantity, variantId }),
    });
    return res.data;
  },

  async updateItem(itemId, quantity) {
    const res = await apiClient(`/cart/items/${itemId}`, {
      method: 'PUT',
      body: JSON.stringify({ quantity }),
    });
    return res.data;
  },

  async removeItem(itemId) {
    const res = await apiClient(`/cart/items/${itemId}`, {
      method: 'DELETE',
    });
    return res.data;
  },

  async clearCart() {
    const res = await apiClient('/cart', {
      method: 'DELETE',
    });
    return res.data;
  },

  async mergeCart(guestToken) {
    const token = guestToken || localStorage.getItem('niramoy_guest_token');
    if (!token) return null;
    const res = await apiClient(`/cart/merge?guestToken=${encodeURIComponent(token)}`, {
      method: 'POST',
      headers: {
        'X-Guest-Token': token,
      },
    });
    clearGuestToken();
    return res.data || res;
  },

  async applyCoupon(couponCode) {
    const res = await apiClient('/cart/apply-coupon', {
      method: 'POST',
      body: JSON.stringify({ couponCode }),
    });
    return res.data;
  },

  async removeCoupon() {
    const res = await apiClient('/cart/remove-coupon', {
      method: 'DELETE',
    });
    return res.data;
  },
};
