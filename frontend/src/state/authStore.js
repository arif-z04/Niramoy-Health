import { authApi } from '../api/auth.js';
import { cartApi } from '../api/cart.js';
import { clearTokens } from '../api/client.js';
import { cartStore } from './cartStore.js';

class AuthStore {
  constructor() {
    this.user = null;
    this.listeners = new Set();
    this.init();
  }

  init() {
    const saved = localStorage.getItem('niramoy_user');
    if (saved) {
      try {
        this.user = JSON.parse(saved);
      } catch (e) {
        this.user = null;
      }
    }

    window.addEventListener('auth:expired', () => {
      this.user = null;
      this.notify();
    });

    window.addEventListener('auth:refreshed', (e) => {
      if (e.detail) {
        this.user = e.detail;
        this.notify();
      }
    });
  }

  get isAuthenticated() {
    if (!this.user) {
      const saved = localStorage.getItem('niramoy_user');
      if (saved) {
        try { this.user = JSON.parse(saved); } catch (e) {}
      }
    }
    return !!this.user && !!localStorage.getItem('niramoy_access_token');
  }

  get roles() {
    if (!this.user) {
      const saved = localStorage.getItem('niramoy_user');
      if (saved) {
        try { this.user = JSON.parse(saved); } catch (e) {}
      }
    }
    return this.user?.roles || [];
  }

  get isAdmin() {
    return this.roles.includes('admin') || this.roles.includes('super_admin');
  }

  get isPharmacist() {
    return this.roles.includes('pharmacist') || this.isAdmin;
  }

  subscribe(listener) {
    this.listeners.add(listener);
    listener(this.getState());
    return () => this.listeners.delete(listener);
  }

  notify() {
    const state = this.getState();
    this.listeners.forEach(l => l(state));
  }

  getState() {
    return {
      user: this.user,
      isAuthenticated: this.isAuthenticated,
      roles: this.roles,
      isAdmin: this.isAdmin,
      isPharmacist: this.isPharmacist,
    };
  }

  async login(phoneOrEmail, password) {
    const res = await authApi.login(phoneOrEmail, password);
    this.user = res.user;
    this.notify();

    // Automatically merge guest cart if one existed!
    try {
      const mergedCart = await cartApi.mergeCart();
      if (mergedCart) {
        cartStore.cart = cartStore.normalizeCart(mergedCart);
        cartStore.notify();
      } else {
        await cartStore.fetchCart();
      }
    } catch (e) {
      console.warn('Cart merge non-critical notice:', e);
      await cartStore.fetchCart();
    }

    return res;
  }

  async register(data) {
    const res = await authApi.register(data);
    this.user = res.user;
    this.notify();

    try {
      await cartApi.mergeCart();
    } catch (e) {
      console.warn('Cart merge non-critical notice:', e);
    }

    return res;
  }

  async logout() {
    await authApi.logout();
    this.user = null;
    this.notify();
  }

  async loadMe() {
    if (!this.isAuthenticated) return null;
    try {
      const user = await authApi.getMe();
      this.user = user;
      this.notify();
      return user;
    } catch (e) {
      if (e.status === 401) {
        this.user = null;
        this.notify();
      }
      return null;
    }
  }

  setUser(user) {
    this.user = user;
    if (user) {
      localStorage.setItem('niramoy_user', JSON.stringify(user));
    } else {
      localStorage.removeItem('niramoy_user');
    }
    this.notify();
  }
}

export const authStore = new AuthStore();
