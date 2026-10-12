import { ordersApi } from '../api/orders.js';
import { toast } from '../state/toastStore.js';
import { escapeHtml, formatCurrency, formatDate, getStatusBadgeClass } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export async function renderOrderDetailsPage(orderId) {
  const container = document.createElement('div');
  container.className = 'max-w-4xl mx-auto px-4 sm:px-6 py-8 space-y-6';

  container.innerHTML = `
    <div class="animate-pulse p-8 text-center text-xs text-slate-400 bg-white rounded-3xl border border-slate-200">
      Loading order details...
    </div>
  `;

  try {
    const order = await ordersApi.getOrderById(orderId);
    if (!order) {
      container.innerHTML = `<div class="p-8 text-center text-xs text-rose-600 bg-white rounded-3xl">Order not found.</div>`;
      return container;
    }

    renderContent(container, order);
  } catch (err) {
    container.innerHTML = `
      <div class="p-8 text-center bg-white rounded-3xl border border-rose-200 text-rose-700 text-xs">
        Failed to load order: ${escapeHtml(err.message)}
      </div>
    `;
  }

  return container;
}

function renderContent(container, order) {
  const orderStatusBadge = getStatusBadgeClass(order.orderStatus);
  const paymentStatusBadge = getStatusBadgeClass(order.paymentStatus);
  const canCancel = order.orderStatus === 'pending';

  // Lifecycle steps
  const steps = [
    { key: 'pending', label: 'Order Placed' },
    { key: 'confirmed', label: 'Confirmed' },
    { key: 'processing', label: 'Processing' },
    { key: 'packed', label: 'Packed' },
    { key: 'shipped', label: 'In Transit' },
    { key: 'delivered', label: 'Delivered' },
  ];

  const currentStepIndex = steps.findIndex(s => s.key === order.orderStatus);

  container.innerHTML = `
    <!-- Top Back Link & Actions -->
    <div class="flex items-center justify-between">
      <a href="#/account/orders" class="text-xs font-semibold text-brand-600 hover:text-brand-700 flex items-center gap-1">
        &larr; Back to Orders
      </a>
      ${canCancel ? `
        <button type="button" id="cancel-order-btn" class="btn-secondary text-rose-600 hover:text-rose-700 text-xs py-1.5 px-3">
          Cancel Order
        </button>
      ` : ''}
    </div>

    <!-- Order Header Card -->
    <div class="bg-white rounded-3xl border border-slate-200/80 p-4 sm:p-8 shadow-card space-y-5 sm:space-y-6">
      
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-slate-100 pb-4 sm:pb-5">
        <div>
          <div class="flex flex-wrap items-center gap-1.5 sm:gap-2">
            <h1 class="text-lg sm:text-2xl font-black text-slate-900 font-mono">${escapeHtml(order.orderNumber)}</h1>
            <span class="px-2 py-0.5 rounded-full text-[10px] sm:text-xs font-bold border ${orderStatusBadge} uppercase">
              ${escapeHtml(order.orderStatus)}
            </span>
            <span class="px-2 py-0.5 rounded-full text-[10px] sm:text-xs font-bold border ${paymentStatusBadge} uppercase">
              ${escapeHtml(order.paymentStatus)}
            </span>
          </div>
          <p class="text-xs text-slate-500 mt-1">Placed on ${formatDate(order.placedAt || order.createdAt)}</p>
        </div>

        <div class="text-left sm:text-right">
          <div class="text-xl sm:text-2xl font-black text-brand-700">${formatCurrency(order.totalAmount)}</div>
          <div class="text-xs text-slate-500">${order.items?.length || 0} items • ${escapeHtml(order.deliveryMethodName || 'Delivery')}</div>
        </div>
      </div>

      <!-- Order Status Stepper -->
      ${order.orderStatus !== 'cancelled' ? `
        <div class="space-y-3 pt-2">
          <h4 class="text-xs font-bold text-slate-700 uppercase tracking-wider">Tracking Status</h4>
          <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-2 sm:gap-2.5">
            ${steps.map((st, i) => {
              const isPast = currentStepIndex >= i;
              const isCurrent = currentStepIndex === i;
              return `
                <div class="text-center space-y-1">
                  <div class="h-2 rounded-full ${isPast ? 'bg-brand-600' : 'bg-slate-200'} ${isCurrent ? 'ring-2 ring-brand-400' : ''}"></div>
                  <span class="text-[10px] font-semibold block ${isPast ? 'text-brand-800' : 'text-slate-400'}">${st.label}</span>
                </div>
              `;
            }).join('')}
          </div>
        </div>
      ` : `
        <div class="p-3 bg-rose-50 border border-rose-200 rounded-xl text-xs text-rose-800">
          <strong>This order was cancelled.</strong> Reason: ${escapeHtml(order.cancelReason || 'Customer requested')}
        </div>
      `}

      <!-- Delivery & Payment Information -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4 border-t border-slate-100 text-xs">
        <div class="space-y-1">
          <h4 class="font-bold text-slate-900 uppercase tracking-wider text-[11px] text-slate-400">Shipping Address</h4>
          ${order.address ? `
            <div class="font-bold text-slate-800">${escapeHtml(order.address.recipientName)} (${escapeHtml(order.address.recipientPhone)})</div>
            <div class="text-slate-600">${escapeHtml(order.address.areaDetails)}, ${escapeHtml(order.address.upazila)}, ${escapeHtml(order.address.district)}, ${escapeHtml(order.address.division)}</div>
            ${order.address.deliveryInstructions ? `<div class="italic text-slate-500 mt-1">Instructions: ${escapeHtml(order.address.deliveryInstructions)}</div>` : ''}
          ` : `
            <div class="text-slate-500">Address details on file</div>
          `}
        </div>

        <div class="space-y-1">
          <h4 class="font-bold text-slate-900 uppercase tracking-wider text-[11px] text-slate-400">Payment &amp; Delivery Method</h4>
          <div class="text-slate-800 font-semibold">${escapeHtml(order.paymentMethodName || 'Payment Provider')}</div>
          <div class="text-slate-600">${escapeHtml(order.deliveryMethodName || 'Standard Delivery')}</div>
          ${order.payments && order.payments.length > 0 ? `
            <div class="text-[11px] text-slate-500 pt-1 font-mono">
              TrxID: ${escapeHtml(order.payments[0]?.transactionId || 'Awaiting Transaction')}
            </div>
          ` : ''}
        </div>
      </div>

      <!-- Line Items Table -->
      <div class="space-y-3 pt-4 border-t border-slate-100">
        <h4 class="text-xs font-bold text-slate-900 uppercase tracking-wider">Ordered Items</h4>
        <div class="divide-y divide-slate-100 border border-slate-200/80 rounded-2xl overflow-hidden">
          ${(order.items || []).map(it => `
            <div class="p-4 flex items-center justify-between gap-4 bg-white">
              <div class="flex items-center gap-3 min-w-0">
                <div class="w-10 h-10 rounded-lg bg-slate-50 border border-slate-200/60 p-1 flex-shrink-0 flex items-center justify-center">
                  <img src="${escapeHtml(it.productImageUrl || '/assets/medicine-placeholder.png')}" alt="" class="max-h-full max-w-full object-contain" onerror="this.src='data:image/svg+xml;utf8,<svg xmlns=\\'http://www.w3.org/2000/svg\\' width=\\'24\\' height=\\'24\\' viewBox=\\'0 0 24 24\\' fill=\\'none\\' stroke=\\'%2394a3b8\\' stroke-width=\\'2\\'><path d=\\'m10.5 20.5 10-10a4.95 4.95 0 1 0-7-7l-10 10a4.95 4.95 0 1 0 7 7Z\\'/></svg>'"/>
                </div>
                <div class="min-w-0">
                  <div class="text-xs font-bold text-slate-900 truncate">${escapeHtml(it.productName)}</div>
                  <div class="text-[11px] text-slate-500">${escapeHtml(it.strength || '')} • SKU: ${escapeHtml(it.productSku || 'MED')}</div>
                </div>
              </div>

              <div class="text-right flex-shrink-0">
                <div class="text-xs font-bold text-slate-900">${formatCurrency(it.totalPrice)}</div>
                <div class="text-[11px] text-slate-400">${formatCurrency(it.unitPriceSnapshot)} × ${it.quantity}</div>
              </div>
            </div>
          `).join('')}
        </div>
      </div>

      <!-- Financial Totals Breakdown -->
      <div class="border-t border-slate-100 pt-4 flex justify-end">
        <div class="w-full sm:w-72 space-y-2 text-xs text-slate-600">
          <div class="flex justify-between">
            <span>Subtotal</span>
            <span class="font-semibold text-slate-900">${formatCurrency(order.subtotalAmount)}</span>
          </div>
          ${order.discountAmount > 0 ? `
            <div class="flex justify-between text-emerald-600">
              <span>Discount ${order.couponCode ? `(${escapeHtml(order.couponCode)})` : ''}</span>
              <span class="font-semibold">-${formatCurrency(order.discountAmount)}</span>
            </div>
          ` : ''}
          <div class="flex justify-between">
            <span>Delivery Fee</span>
            <span class="font-semibold text-slate-900">${formatCurrency(order.deliveryFee)}</span>
          </div>
          <div class="border-t border-slate-200 pt-2 flex justify-between text-sm font-extrabold text-slate-900">
            <span>Grand Total</span>
            <span class="text-base text-brand-700">${formatCurrency(order.totalAmount)}</span>
          </div>
        </div>
      </div>

    </div>
  `;

  renderIcons(container);

  // Setup Cancel Order
  container.querySelector('#cancel-order-btn')?.addEventListener('click', async () => {
    const reason = prompt('Please state the reason for cancelling this order:', 'Change of mind');
    if (reason) {
      try {
        await ordersApi.cancelOrder(order.id, reason);
        toast.info('Order cancelled.');
        window.location.reload();
      } catch (err) {
        toast.error(err.message || 'Failed to cancel order');
      }
    }
  });
}
