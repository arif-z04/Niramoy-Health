/**
 * Toast Notification State Manager
 */

class ToastStore {
  constructor() {
    this.toasts = [];
    this.listeners = new Set();
  }

  subscribe(listener) {
    this.listeners.add(listener);
    listener(this.toasts);
    return () => this.listeners.delete(listener);
  }

  notify() {
    this.listeners.forEach(listener => listener([...this.toasts]));
  }

  show(message, type = 'info', duration = 4000) {
    const id = Date.now() + Math.random().toString(36).substring(2, 9);
    const toast = { id, message, type };
    this.toasts.push(toast);
    this.notify();

    if (duration > 0) {
      setTimeout(() => {
        this.remove(id);
      }, duration);
    }
    return id;
  }

  success(message, duration = 4000) {
    return this.show(message, 'success', duration);
  }

  error(message, duration = 5000) {
    return this.show(message, 'error', duration);
  }

  warning(message, duration = 4500) {
    return this.show(message, 'warning', duration);
  }

  info(message, duration = 4000) {
    return this.show(message, 'info', duration);
  }

  remove(id) {
    this.toasts = this.toasts.filter(t => t.id !== id);
    this.notify();
  }
}

export const toast = new ToastStore();
