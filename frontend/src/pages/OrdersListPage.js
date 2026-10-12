import { authStore } from '../state/authStore.js';
import { ordersApi } from '../api/orders.js';
import { escapeHtml, formatCurrency, formatDate, getStatusBadgeClass } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export async function renderOrdersListPage() {
  const container = document.createElement('div');
  container.className = 'max-w-5xl mx-auto px-4 sm:px-6 py-8 space-y-6';

  const authState = authStore.getState();
  if (!authState.isAuthenticated) {
    container.innerHTML = `
      <div class="bg-white rounded-3xl border border-slate-200/80 p-8 text-center shadow-card space-y-4">
        <h2 class="text-xl font-bold text-slate-900">Sign in to view your orders</h2>
        <a href="#/login?redirect=account/orders" class="btn-primary text-xs py-2 px-5 inline-flex">Sign In</a>
      </div>
    `;
    return container;
  }

  container.innerHTML = `
    <!-- Header -->
    <div class="border-b border-slate-200/80 pb-4">
      <h1 class="text-2xl font-bold text-slate-900">My Orders</h1>
      <p class="text-xs text-slate-500">Track and view your medicine orders, deliveries, and receipts</p>
    </div>

    <!-- Orders list -->
    <div id="orders-list-container" class="space-y-4">
      <div class="p-8 text-center text-xs text-slate-400">Loading your orders...</div>
    </div>
  `;

  renderIcons(container);

  loadOrders(container);

  return container;
}

async function loadOrders(container) {
  const listEl = container.querySelector('#orders-list-container');
  try {
    const data = await ordersApi.getMyOrders();
    const orders = data?.items || [];

    if (orders.length === 0) {
      listEl.innerHTML = `
        <div class="p-12 text-center bg-white rounded-3xl border border-slate-200/80 shadow-sm space-y-4">
          <div class="w-14 h-14 rounded-2xl bg-brand-50 text-brand-600 flex items-center justify-center mx-auto">
            <i data-lucide="package" class="w-7 h-7"></i>
          </div>
          <div>
            <h3 class="text-sm font-bold text-slate-800">No Orders Placed Yet</h3>
            <p class="text-xs text-slate-500 mt-1 max-w-sm mx-auto">You haven't completed any healthcare purchases yet. Start browsing to place your first order.</p>
          </div>
          <a href="#/products" class="btn-primary text-xs py-2 px-5 inline-flex font-bold">
            Start Shopping &rarr;
          </a>
        </div>
      `;
    } else {
      listEl.innerHTML = orders.map(order => {
        const orderStatusBadge = getStatusBadgeClass(order.orderStatus);
        const paymentStatusBadge = getStatusBadgeClass(order.paymentStatus);

        return `
          <div class="bg-white rounded-2xl border border-slate-200/80 p-5 shadow-sm space-y-4 hover:border-slate-300 transition">
            
            <!-- Order Header -->
            <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2 border-b border-slate-100 pb-3">
              <div class="space-y-0.5">
                <div class="flex items-center gap-2">
                  <span class="text-xs font-bold text-slate-900 font-mono">${escapeHtml(order.orderNumber)}</span>
                  <span class="px-2 py-0.5 rounded-full text-[10px] font-bold border ${orderStatusBadge} uppercase">
                    ${escapeHtml(order.orderStatus)}
                  </span>
                  <span class="px-2 py-0.5 rounded-full text-[10px] font-bold border ${paymentStatusBadge} uppercase">
                    ${escapeHtml(order.paymentStatus)}
                  </span>
                </div>
                <div class="text-[11px] text-slate-400">Placed on ${formatDate(order.placedAt || order.createdAt)}</div>
              </div>

              <div class="text-left sm:text-right">
                <div class="text-sm font-extrabold text-slate-900">${formatCurrency(order.totalAmount)}</div>
                <div class="text-[11px] text-slate-500">${order.items?.length || 0} items • ${escapeHtml(order.deliveryMethodName || 'Delivery')}</div>
              </div>
            </div>

            <!-- Items snippet -->
            <div class="divide-y divide-slate-100 text-xs">
              ${(order.items || []).slice(0, 3).map(it => `
                <div class="py-2 flex items-center justify-between">
                  <span class="text-slate-800 font-medium truncate max-w-md">
                    ${escapeHtml(it.productName)} <span class="text-slate-400">× ${it.quantity}</span>
                  </span>
                  <span class="font-semibold text-slate-900">${formatCurrency(it.totalPrice)}</span>
                </div>
              `).join('')}
              ${(order.items?.length || 0) > 3 ? `
                <div class="pt-2 text-[11px] text-slate-500 italic">+ ${(order.items?.length || 0) - 3} more items</div>
              ` : ''}
            </div>

            <!-- Footer actions -->
            <div class="flex items-center justify-between pt-2 border-t border-slate-100">
              <a href="#/account/orders/${order.id}" class="btn-secondary text-xs py-1.5 px-3 font-semibold">
                View Order Details &rarr;
              </a>
              ${order.orderStatus === 'pending' ? `
                <span class="text-[11px] text-amber-700 font-medium">Awaiting fulfillment</span>
              ` : ''}
            </div>

          </div>
        `;
      }).join('');
    }

    renderIcons(listEl);
  } catch (err) {
    listEl.innerHTML = `<p class="text-xs text-rose-600 p-2">Error loading orders: ${err.message}</p>`;
  }
}
