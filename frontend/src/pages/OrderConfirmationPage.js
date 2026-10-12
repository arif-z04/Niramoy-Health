import { ordersApi } from '../api/orders.js';
import { paymentsApi } from '../api/payments.js';
import { toast } from '../state/toastStore.js';
import { escapeHtml, formatCurrency, formatDate, getStatusBadgeClass } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export async function renderOrderConfirmationPage(orderId) {
  const container = document.createElement('div');
  container.className = 'max-w-2xl mx-auto px-4 sm:px-6 py-12 space-y-6';

  container.innerHTML = `
    <div class="animate-pulse p-12 text-center text-xs text-slate-400 bg-white rounded-3xl border border-slate-200">
      Loading order confirmation...
    </div>
  `;

  try {
    const order = await ordersApi.getOrderById(orderId);
    if (!order) {
      container.innerHTML = `<div class="p-8 text-center text-xs text-rose-600 bg-white rounded-3xl">Order not found.</div>`;
      return container;
    }

    renderConfirmationContent(container, order);
  } catch (err) {
    container.innerHTML = `
      <div class="p-8 text-center bg-white rounded-3xl border border-rose-200 text-rose-700 text-xs">
        Failed to load confirmation: ${escapeHtml(err.message)}
      </div>
    `;
  }

  return container;
}

function renderConfirmationContent(container, order) {
  const orderStatusBadge = getStatusBadgeClass(order.orderStatus);
  const paymentStatusBadge = getStatusBadgeClass(order.paymentStatus);
  const isMfs = order.paymentMethodCode === 'bkash' || order.paymentMethodCode === 'nagad';
  const needsPayment = order.paymentStatus === 'unpaid' && isMfs;

  container.innerHTML = `
    <div class="bg-white rounded-3xl border border-slate-200/80 p-6 sm:p-10 shadow-card text-center space-y-6">
      
      <!-- Success Icon -->
      <div class="w-20 h-20 rounded-full bg-emerald-50 text-emerald-600 border-4 border-emerald-100 flex items-center justify-center mx-auto">
        <i data-lucide="check" class="w-10 h-10 stroke-[3]"></i>
      </div>

      <!-- Main Heading -->
      <div class="space-y-1">
        <span class="text-xs font-bold text-brand-700 uppercase tracking-widest">Order Placed Successfully</span>
        <h1 class="text-2xl sm:text-3xl font-black text-slate-900 tracking-tight">Thank You For Your Order!</h1>
        <p class="text-xs text-slate-500 max-w-md mx-auto mt-1">
          Your order has been recorded. Our pharmacy fulfillment team has received your prescription and requested items.
        </p>
      </div>

      <!-- Order Details Summary Box -->
      <div class="bg-slate-50 rounded-2xl p-5 border border-slate-200/60 text-xs text-slate-700 space-y-3 max-w-lg mx-auto text-left">
        <div class="flex justify-between items-center border-b border-slate-200/60 pb-2.5">
          <span class="text-slate-500 font-medium">Order Number</span>
          <span class="font-bold text-slate-900 font-mono text-sm">${escapeHtml(order.orderNumber)}</span>
        </div>
        <div class="flex justify-between items-center">
          <span class="text-slate-500 font-medium">Date &amp; Time</span>
          <span class="font-semibold text-slate-800">${formatDate(order.placedAt || order.createdAt)}</span>
        </div>
        <div class="flex justify-between items-center">
          <span class="text-slate-500 font-medium">Order Status</span>
          <span class="px-2.5 py-0.5 rounded-full text-[10px] font-bold border ${orderStatusBadge} uppercase">
            ${escapeHtml(order.orderStatus)}
          </span>
        </div>
        <div class="flex justify-between items-center">
          <span class="text-slate-500 font-medium">Payment Status</span>
          <span class="px-2.5 py-0.5 rounded-full text-[10px] font-bold border ${paymentStatusBadge} uppercase" id="conf-pay-status-badge">
            ${escapeHtml(order.paymentStatus)}
          </span>
        </div>
        <div class="flex justify-between items-center">
          <span class="text-slate-500 font-medium">Payment Method</span>
          <span class="font-semibold text-slate-800">${escapeHtml(order.paymentMethodName || 'Cash on Delivery')}</span>
        </div>
        <div class="flex justify-between items-center border-t border-slate-200/60 pt-2.5 text-sm font-extrabold text-slate-900">
          <span>Amount Payable</span>
          <span class="text-brand-700 text-base font-black">${formatCurrency(order.totalAmount)}</span>
        </div>
      </div>

      <!-- MFS Simulator / Payment Execution Box -->
      ${needsPayment ? `
        <div id="mfs-simulator-box" class="p-5 rounded-2xl bg-gradient-to-br from-pink-50 to-orange-50 border border-pink-200 text-left space-y-3 max-w-lg mx-auto">
          <div class="flex items-center gap-2 text-xs font-bold text-pink-900">
            <i data-lucide="smartphone" class="w-4 h-4 text-pink-600"></i>
            <span>${order.paymentMethodName} Sandbox Simulator</span>
          </div>
          <p class="text-[11px] text-pink-800 leading-relaxed">
            In development sandbox mode, you can simulate instantaneous payment verification through the test gateway.
          </p>
          <button type="button" id="simulate-mfs-btn" class="btn-primary w-full py-2.5 text-xs font-bold bg-pink-600 hover:bg-pink-700 text-white shadow-sm">
            Pay ${formatCurrency(order.totalAmount)} via ${order.paymentMethodName} (Sandbox)
          </button>
        </div>
      ` : `
        <div class="p-4 bg-emerald-50 border border-emerald-200 rounded-2xl text-xs text-emerald-800 max-w-lg mx-auto flex items-center justify-center gap-2">
          <i data-lucide="shield-check" class="w-4 h-4 text-emerald-600"></i>
          <span>${order.paymentMethodCode === 'cod' ? 'Payment will be collected in cash upon delivery.' : 'Payment has been successfully verified!'}</span>
        </div>
      `}

      <!-- Action Buttons -->
      <div class="flex flex-col sm:flex-row items-center justify-center gap-3 pt-2">
        <a href="#/account/orders/${order.id}" class="btn-primary text-xs py-2.5 px-6 font-bold w-full sm:w-auto">
          View Order Tracking &rarr;
        </a>
        <a href="#/products" class="btn-secondary text-xs py-2.5 px-5 w-full sm:w-auto">
          Continue Shopping
        </a>
      </div>

    </div>
  `;

  renderIcons(container);

  // Setup MFS Simulator Button
  const simBtn = container.querySelector('#simulate-mfs-btn');
  simBtn?.addEventListener('click', async () => {
    simBtn.disabled = true;
    simBtn.innerHTML = `<i data-lucide="loader" class="w-3.5 h-3.5 animate-spin"></i> Processing Payment...`;
    renderIcons(simBtn);

    try {
      // 1. Initiate payment
      const initRes = await paymentsApi.initiatePayment(order.id);
      
      // 2. Execute payment
      const execRes = await paymentsApi.executePayment(
        initRes.paymentPublicId, 
        initRes.gatewayPaymentId, 
        `TRX-SANDBOX-${Date.now()}`
      );

      toast.success('Payment completed successfully!');
      
      // Reload page to reflect updated 'paid' / 'confirmed' status
      const updatedOrder = await ordersApi.getOrderById(order.id);
      renderConfirmationContent(container, updatedOrder);
    } catch (err) {
      toast.error(err.message || 'Payment simulation failed');
      simBtn.disabled = false;
      simBtn.innerHTML = `Retry Payment Simulation`;
    }
  });
}
