/**
 * Utility functions for currency, dates, sanitization, and debouncing
 */

export function formatCurrency(amount) {
  const num = typeof amount === 'number' ? amount : parseFloat(amount || 0);
  return `৳${num.toFixed(2)}`;
}

export function formatDate(dateString) {
  if (!dateString) return '';
  const d = new Date(dateString);
  return d.toLocaleDateString('en-GB', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function escapeHtml(str) {
  if (!str) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}

export function debounce(func, wait = 300) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

export function getStatusBadgeClass(status) {
  switch (status?.toLowerCase()) {
    case 'confirmed':
    case 'delivered':
    case 'paid':
    case 'captured':
    case 'approved':
    case 'resolved':
      return 'bg-emerald-50 text-emerald-700 border-emerald-200';
    case 'pending':
    case 'submitted':
    case 'under_review':
    case 'open':
    case 'processing':
    case 'packed':
    case 'shipped':
    case 'out_for_delivery':
      return 'bg-amber-50 text-amber-700 border-amber-200';
    case 'cancelled':
    case 'failed':
    case 'rejected':
    case 'unpaid':
      return 'bg-rose-50 text-rose-700 border-rose-200';
    case 'refunded':
      return 'bg-purple-50 text-purple-700 border-purple-200';
    default:
      return 'bg-slate-50 text-slate-700 border-slate-200';
  }
}
