import { cartApi } from '../api/cart.js';
import { toast } from './toastStore.js';

class CartStore {
  constructor() {
    this.cart = {
      items: [],
      subtotalAmount: 0,
      discountAmount: 0,
      deliveryFee: 0,
      totalAmount: 0,
      couponCode: null,
    };
    this.isLoading = false;
    this.isDrawerOpen = false;
    this.listeners = new Set();
  }

  get itemCount() {
    return (this.cart?.items || []).reduce((sum, item) => sum + (item.quantity || 0), 0);
  }

  get hasPrescriptionItems() {
    return (this.cart?.items || []).some(item => item.isPrescriptionRequired);
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
      cart: this.cart,
      itemCount: this.itemCount,
      hasPrescriptionItems: this.hasPrescriptionItems,
      isLoading: this.isLoading,
      isDrawerOpen: this.isDrawerOpen,
    };
  }

  openDrawer() {
    this.isDrawerOpen = true;
    this.notify();
  }

  closeDrawer() {
    this.isDrawerOpen = false;
    this.notify();
  }

  toggleDrawer() {
    this.isDrawerOpen = !this.isDrawerOpen;
    this.notify();
  }

  normalizeCart(data) {
    if (!data) return { items: [], subtotalAmount: 0, discountAmount: 0, deliveryFee: 0, totalAmount: 0, couponCode: null };
    const rawItems = data.items || [];
    const items = rawItems.map(it => ({
      id: it.id,
      productId: it.productId,
      productName: it.name || it.productName || 'Medicine',
      productImageUrl: it.primaryImageUrl || it.productImageUrl || '/assets/medicine-placeholder.png',
      strength: it.strength || '',
      unitPrice: it.currentPrice ?? it.unitPrice ?? 0,
      quantity: it.quantity,
      totalPrice: it.itemTotal ?? it.totalPrice ?? ((it.currentPrice ?? it.unitPrice ?? 0) * it.quantity),
      isPrescriptionRequired: it.isPrescriptionRequired ?? false,
      availableStock: it.availableStock ?? 999,
    }));

    return {
      id: data.id,
      items,
      subtotalAmount: data.subtotal ?? data.subtotalAmount ?? 0,
      discountAmount: data.discountAmount ?? 0,
      deliveryFee: data.deliveryFee ?? 0,
      totalAmount: data.grandTotal ?? data.totalAmount ?? 0,
      couponCode: data.appliedCouponCode ?? data.couponCode ?? null,
      requiresPrescription: data.requiresPrescription ?? false,
    };
  }

  async fetchCart() {
    this.isLoading = true;
    this.notify();
    try {
      const data = await cartApi.getCart();
      this.cart = this.normalizeCart(data);
    } catch (e) {
      console.error('Failed to load cart:', e);
    } finally {
      this.isLoading = false;
      this.notify();
    }
  }

  async addItem(productId, quantity = 1, showToast = true) {
    this.isLoading = true;
    this.notify();
    try {
      const data = await cartApi.addItem(productId, quantity);
      this.cart = this.normalizeCart(data);
      if (showToast) {
        toast.success('Item added to cart!');
      }
      return data;
    } catch (e) {
      toast.error(e.message || 'Failed to add item to cart');
      throw e;
    } finally {
      this.isLoading = false;
      this.notify();
    }
  }

  async updateQuantity(itemId, quantity) {
    if (quantity <= 0) {
      return this.removeItem(itemId);
    }
    this.isLoading = true;
    this.notify();
    try {
      const data = await cartApi.updateItem(itemId, quantity);
      this.cart = this.normalizeCart(data);
      return data;
    } catch (e) {
      toast.error(e.message || 'Failed to update quantity');
      throw e;
    } finally {
      this.isLoading = false;
      this.notify();
    }
  }

  async removeItem(itemId) {
    this.isLoading = true;
    this.notify();
    try {
      const data = await cartApi.removeItem(itemId);
      this.cart = this.normalizeCart(data);
      toast.info('Item removed from cart');
      return data;
    } catch (e) {
      toast.error(e.message || 'Failed to remove item');
      throw e;
    } finally {
      this.isLoading = false;
      this.notify();
    }
  }

  async applyCoupon(couponCode) {
    this.isLoading = true;
    this.notify();
    try {
      const data = await cartApi.applyCoupon(couponCode);
      this.cart = this.normalizeCart(data);
      toast.success(`Coupon '${couponCode}' applied!`);
      return data;
    } catch (e) {
      toast.error(e.message || 'Failed to apply coupon');
      throw e;
    } finally {
      this.isLoading = false;
      this.notify();
    }
  }

  async removeCoupon() {
    this.isLoading = true;
    this.notify();
    try {
      const data = await cartApi.removeCoupon();
      this.cart = this.normalizeCart(data);
      toast.info('Coupon removed');
      return data;
    } catch (e) {
      toast.error(e.message || 'Failed to remove coupon');
      throw e;
    } finally {
      this.isLoading = false;
      this.notify();
    }
  }

  async clear() {
    this.isLoading = true;
    this.notify();
    try {
      await cartApi.clearCart();
      this.cart = {
        items: [],
        subtotalAmount: 0,
        discountAmount: 0,
        deliveryFee: 0,
        totalAmount: 0,
      };
    } catch (e) {
      console.error('Failed to clear cart:', e);
    } finally {
      this.isLoading = false;
      this.notify();
    }
  }
}

export const cartStore = new CartStore();
