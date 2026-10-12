import { apiClient, setTokens, clearTokens, clearGuestToken } from './client.js';

export const authApi = {
  async register(data) {
    const res = await apiClient('/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (res.data) {
      setTokens(res.data.accessToken, res.data.refreshToken);
      localStorage.setItem('niramoy_user', JSON.stringify(res.data.user));
    }
    return res.data;
  },

  async login(phoneOrEmail, password) {
    const res = await apiClient('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ identifier: phoneOrEmail, phoneOrEmail, password }),
    });
    if (res.data) {
      setTokens(res.data.accessToken, res.data.refreshToken);
      localStorage.setItem('niramoy_user', JSON.stringify(res.data.user));
    }
    return res.data;
  },

  async logout() {
    try {
      const refreshToken = localStorage.getItem('niramoy_refresh_token');
      if (refreshToken) {
        await apiClient('/auth/revoke', {
          method: 'POST',
          body: JSON.stringify({ refreshToken }),
        });
      }
    } catch (e) {
      // Ignore network errors on logout
    } finally {
      clearTokens();
    }
  },

  async getMe() {
    const res = await apiClient('/auth/me');
    if (res.data) {
      localStorage.setItem('niramoy_user', JSON.stringify(res.data));
    }
    return res.data;
  },

  async updateProfile(data) {
    const res = await apiClient('/auth/profile', {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    if (res.data) {
      localStorage.setItem('niramoy_user', JSON.stringify(res.data));
    }
    return res.data;
  },

  async changePassword(currentPassword, newPassword) {
    const res = await apiClient('/auth/change-password', {
      method: 'POST',
      body: JSON.stringify({ currentPassword, newPassword }),
    });
    return res.data;
  },
};
