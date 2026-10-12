import { authStore } from '../state/authStore.js';
import { adminApi } from '../api/admin.js';
import { prescriptionsApi } from '../api/prescriptions.js';
import { toast } from '../state/toastStore.js';
import { escapeHtml, formatCurrency, formatDate, getStatusBadgeClass } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export async function renderAdminDashboardPage() {
  const container = document.createElement('div');
  container.className = 'max-w-7xl mx-auto px-4 sm:px-6 py-8 space-y-8';

  const authState = authStore.getState();
  if (!authState.isAdmin && !authState.isPharmacist) {
    container.innerHTML = `
      <div class="bg-white rounded-3xl border border-rose-200 p-12 text-center shadow-card space-y-4 max-w-md mx-auto">
        <div class="w-16 h-16 rounded-full bg-rose-50 text-rose-600 flex items-center justify-center mx-auto">
          <i data-lucide="shield-alert" class="w-8 h-8"></i>
        </div>
        <h2 class="text-xl font-bold text-slate-900">Restricted Access</h2>
        <p class="text-xs text-slate-500">This management portal requires Administrator or Pharmacist privileges.</p>
        <a href="#/login?redirect=admin" class="btn-primary text-xs py-2 px-5 inline-flex">Sign In with Staff Account</a>
      </div>
    `;
    renderIcons(container);
    return container;
  }

  container.innerHTML = `
    <!-- Top Header Bar -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-200/80 pb-5">
      <div>
        <div class="flex items-center gap-2">
          <span class="px-2 py-0.5 rounded bg-brand-700 text-white font-bold text-[10px] uppercase tracking-wider">STAFF PORTAL</span>
          <h1 class="text-2xl font-black text-slate-900">Pharmacy Administration</h1>
        </div>
        <p class="text-xs text-slate-500 mt-0.5">Real-time fulfillment, clinical prescription verification, and catalog control</p>
      </div>

      <div class="flex items-center gap-2">
        <button type="button" id="admin-refresh-btn" class="btn-secondary text-xs py-2 px-3">
          <i data-lucide="refresh-cw" class="w-3.5 h-3.5"></i> Refresh Data
        </button>
      </div>
    </div>

    <!-- KPIs Row (Loaded from /api/v1/admin/dashboard) -->
    <div id="admin-kpis-grid" class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-2.5 sm:gap-3">
      ${Array(6).fill(0).map(() => `
        <div class="bg-white rounded-2xl border border-slate-200/60 p-3 sm:p-4 animate-pulse space-y-2">
          <div class="h-3 bg-slate-200 rounded w-1/2"></div>
          <div class="h-6 bg-slate-200 rounded w-3/4"></div>
        </div>
      `).join('')}
    </div>

    <!-- Tab Navigation -->
    <div class="border-b border-slate-200 overflow-x-auto no-scrollbar">
      <nav class="flex gap-4 sm:gap-6 text-xs font-bold whitespace-nowrap min-w-0" id="admin-tabs-nav">
        <button type="button" class="admin-tab-btn active pb-3 text-brand-700 border-b-2 border-brand-700 flex items-center gap-1.5" data-tab="orders">
          <i data-lucide="package" class="w-4 h-4"></i> Orders Fulfillment
        </button>
        <button type="button" class="admin-tab-btn pb-3 text-slate-500 hover:text-slate-800 border-b-2 border-transparent flex items-center gap-1.5" data-tab="prescriptions">
          <i data-lucide="clipboard-check" class="w-4 h-4"></i> Prescription Queue
        </button>
        <button type="button" class="admin-tab-btn pb-3 text-slate-500 hover:text-slate-800 border-b-2 border-transparent flex items-center gap-1.5" data-tab="inventory">
          <i data-lucide="boxes" class="w-4 h-4"></i> Stock &amp; Inventory
        </button>
      </nav>
    </div>

    <!-- Tab Panes -->
    <div id="admin-tab-content">
      <!-- Orders Tab -->
      <div id="tab-orders" class="space-y-4">
        <div class="flex items-center justify-between text-xs">
          <h3 class="font-bold text-slate-900">Recent Customer Orders</h3>
          <span class="text-slate-500" id="admin-orders-count">Loading orders...</span>
        </div>
        <div id="admin-orders-table-container" class="bg-white rounded-2xl border border-slate-200/80 shadow-sm overflow-hidden">
          <div class="p-8 text-center text-xs text-slate-400">Loading orders...</div>
        </div>
      </div>

      <!-- Prescriptions Tab (Hidden by default) -->
      <div id="tab-prescriptions" class="hidden space-y-4">
        <div class="flex items-center justify-between text-xs">
          <h3 class="font-bold text-slate-900">Clinical Prescription Verification Queue</h3>
          <span class="text-slate-500" id="admin-rx-count">Loading prescriptions...</span>
        </div>
        <div id="admin-rx-table-container" class="space-y-3">
          <div class="p-8 text-center text-xs text-slate-400">Loading prescriptions...</div>
        </div>
      </div>

      <!-- Inventory Tab (Hidden by default) -->
      <div id="tab-inventory" class="hidden space-y-4">
        <div class="flex items-center justify-between text-xs">
          <h3 class="font-bold text-slate-900">Product Inventory &amp; Stock Levels</h3>
          <button type="button" id="trigger-stock-adj-btn" class="btn-primary text-xs py-1.5 px-3">
            + Adjust Stock
          </button>
        </div>
        <div id="admin-inventory-table-container" class="bg-white rounded-2xl border border-slate-200/80 shadow-sm overflow-hidden">
          <div class="p-8 text-center text-xs text-slate-400">Loading inventory...</div>
        </div>
      </div>
    </div>
  `;

  renderIcons(container);
  setupAdminDashboard(container);

  return container;
}

async function setupAdminDashboard(container) {
  // Tabs switching
  const tabBtns = container.querySelectorAll('.admin-tab-btn');
  tabBtns.forEach(btn => {
    btn.addEventListener('click', () => {
      tabBtns.forEach(b => {
        b.classList.remove('active', 'text-brand-700', 'border-brand-700');
        b.classList.add('text-slate-500', 'border-transparent');
      });
      btn.classList.add('active', 'text-brand-700', 'border-brand-700');
      btn.classList.remove('text-slate-500', 'border-transparent');

      const targetTab = btn.dataset.tab;
      container.querySelector('#tab-orders').classList.toggle('hidden', targetTab !== 'orders');
      container.querySelector('#tab-prescriptions').classList.toggle('hidden', targetTab !== 'prescriptions');
      container.querySelector('#tab-inventory').classList.toggle('hidden', targetTab !== 'inventory');
    });
  });

  // Refresh button
  container.querySelector('#admin-refresh-btn')?.addEventListener('click', () => {
    loadAllAdminData(container);
  });

  // Load initial data
  await loadAllAdminData(container);
}

async function loadAllAdminData(container) {
  await Promise.all([
    loadDashboardKpis(container),
    loadAdminOrders(container),
    loadAdminPrescriptions(container),
    loadAdminInventory(container),
  ]);
}

async function loadDashboardKpis(container) {
  const kpiGrid = container.querySelector('#admin-kpis-grid');
  try {
    const kpi = await adminApi.getDashboard();
    kpiGrid.innerHTML = `
      <div class="bg-white rounded-2xl border border-slate-200/70 p-4 shadow-sm">
        <div class="text-[11px] font-semibold text-slate-400 uppercase">Total Orders</div>
        <div class="text-xl font-black text-slate-900 mt-1">${kpi.totalOrders ?? 0}</div>
      </div>
      <div class="bg-white rounded-2xl border border-slate-200/70 p-4 shadow-sm">
        <div class="text-[11px] font-semibold text-slate-400 uppercase">Total Sales</div>
        <div class="text-xl font-black text-brand-700 mt-1">${formatCurrency(kpi.totalRevenue ?? 0)}</div>
      </div>
      <div class="bg-white rounded-2xl border border-slate-200/70 p-4 shadow-sm">
        <div class="text-[11px] font-semibold text-slate-400 uppercase">Pending Orders</div>
        <div class="text-xl font-black text-amber-600 mt-1">${kpi.pendingOrdersCount ?? 0}</div>
      </div>
      <div class="bg-white rounded-2xl border border-slate-200/70 p-4 shadow-sm">
        <div class="text-[11px] font-semibold text-slate-400 uppercase">Pending Rx Reviews</div>
        <div class="text-xl font-black text-rose-600 mt-1">${kpi.pendingPrescriptionsCount ?? 0}</div>
      </div>
      <div class="bg-white rounded-2xl border border-slate-200/70 p-4 shadow-sm">
        <div class="text-[11px] font-semibold text-slate-400 uppercase">Low Stock Alerts</div>
        <div class="text-xl font-black text-orange-600 mt-1">${kpi.lowStockCount ?? 0}</div>
      </div>
      <div class="bg-white rounded-2xl border border-slate-200/70 p-4 shadow-sm">
        <div class="text-[11px] font-semibold text-slate-400 uppercase">Open Support</div>
        <div class="text-xl font-black text-blue-600 mt-1">${kpi.openTicketsCount ?? 0}</div>
      </div>
    `;
  } catch (err) {
    console.warn('KPI load error:', err);
  }
}

async function loadAdminOrders(container) {
  const tableContainer = container.querySelector('#admin-orders-table-container');
  try {
    const res = await adminApi.getOrders(1, 20);
    const orders = res?.items || [];
    container.querySelector('#admin-orders-count').textContent = `${orders.length} orders listed`;

    if (orders.length === 0) {
      tableContainer.innerHTML = `<p class="p-8 text-center text-xs text-slate-400">No orders found.</p>`;
    } else {
      tableContainer.innerHTML = `
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs text-slate-700">
            <thead class="bg-slate-50 text-[10px] uppercase font-bold text-slate-500 border-b border-slate-200/60">
              <tr>
                <th class="p-3">Order #</th>
                <th class="p-3">Customer</th>
                <th class="p-3">Status</th>
                <th class="p-3">Payment</th>
                <th class="p-3 text-right">Total</th>
                <th class="p-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              ${orders.map(o => `
                <tr class="hover:bg-slate-50/60">
                  <td class="p-3 font-mono font-bold text-slate-900">${escapeHtml(o.orderNumber)}</td>
                  <td class="p-3">${escapeHtml(o.customerName)} <span class="text-slate-400 font-mono">(${escapeHtml(o.customerPhone)})</span></td>
                  <td class="p-3">
                    <span class="px-2 py-0.5 rounded text-[10px] font-bold border ${getStatusBadgeClass(o.orderStatus)} uppercase">${escapeHtml(o.orderStatus)}</span>
                  </td>
                  <td class="p-3">
                    <span class="px-2 py-0.5 rounded text-[10px] font-bold border ${getStatusBadgeClass(o.paymentStatus)} uppercase">${escapeHtml(o.paymentStatus)}</span>
                  </td>
                  <td class="p-3 text-right font-extrabold text-slate-900">${formatCurrency(o.totalAmount)}</td>
                  <td class="p-3 text-right">
                    <select class="admin-change-status-select form-input text-[11px] py-1 px-2 w-auto inline-block" data-order-id="${o.id}">
                      <option value="pending" ${o.orderStatus === 'pending' ? 'selected' : ''}>Pending</option>
                      <option value="confirmed" ${o.orderStatus === 'confirmed' ? 'selected' : ''}>Confirmed</option>
                      <option value="processing" ${o.orderStatus === 'processing' ? 'selected' : ''}>Processing</option>
                      <option value="packed" ${o.orderStatus === 'packed' ? 'selected' : ''}>Packed</option>
                      <option value="shipped" ${o.orderStatus === 'shipped' ? 'selected' : ''}>Shipped</option>
                      <option value="delivered" ${o.orderStatus === 'delivered' ? 'selected' : ''}>Delivered</option>
                      <option value="cancelled" ${o.orderStatus === 'cancelled' ? 'selected' : ''}>Cancelled</option>
                    </select>
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      `;

      // Status change handlers
      tableContainer.querySelectorAll('.admin-change-status-select').forEach(sel => {
        sel.addEventListener('change', async () => {
          const orderId = sel.dataset.orderId;
          const newStatus = sel.value;
          try {
            await adminApi.updateOrderStatus(orderId, newStatus, `Status updated to ${newStatus} by admin`);
            toast.success(`Order #${orderId} status updated to ${newStatus}!`);
            loadDashboardKpis(container);
          } catch (err) {
            toast.error(err.message || 'Status update failed');
          }
        });
      });
    }
  } catch (err) {
    tableContainer.innerHTML = `<p class="p-4 text-xs text-rose-600">Failed to load orders: ${err.message}</p>`;
  }
}

async function loadAdminPrescriptions(container) {
  const containerEl = container.querySelector('#admin-rx-table-container');
  try {
    const res = await adminApi.getPrescriptions(1, 20);
    const prescriptions = res?.items || [];
    container.querySelector('#admin-rx-count').textContent = `${prescriptions.length} in queue`;

    if (prescriptions.length === 0) {
      containerEl.innerHTML = `<div class="p-8 text-center bg-white rounded-2xl border border-slate-200 text-xs text-slate-400">Prescription review queue is clear.</div>`;
    } else {
      containerEl.innerHTML = prescriptions.map(rx => `
        <div class="bg-white rounded-2xl border border-slate-200/80 p-5 shadow-sm space-y-3">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <div>
              <span class="text-xs font-bold text-slate-900 font-mono">Rx #${rx.id}</span>
              <span class="text-[11px] text-slate-400 ml-2">${formatDate(rx.createdAt)}</span>
            </div>
            <span class="px-2 py-0.5 rounded text-[10px] font-bold border ${getStatusBadgeClass(rx.status)} uppercase">${escapeHtml(rx.status)}</span>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-3 text-xs text-slate-600">
            <div><strong>Patient:</strong> ${escapeHtml(rx.patientName)} (${rx.patientAge}y, ${escapeHtml(rx.patientGender || '')})</div>
            <div><strong>Phone:</strong> ${escapeHtml(rx.patientPhone)}</div>
            <div><strong>Doctor:</strong> ${escapeHtml(rx.doctorName || 'Not specified')}</div>
            <div><strong>File:</strong> <span class="font-mono text-slate-500">${escapeHtml(rx.originalFilename || 'prescription')}</span></div>
          </div>

          ${rx.status === 'submitted' || rx.status === 'under_review' ? `
            <div class="pt-2 border-t border-slate-100 flex items-center justify-end gap-2">
              <button type="button" class="admin-rx-action-btn btn-secondary text-rose-600 hover:text-rose-700 text-xs py-1.5 px-3" data-id="${rx.id}" data-action="rejected">
                Reject Rx
              </button>
              <button type="button" class="admin-rx-action-btn btn-primary text-xs py-1.5 px-4 font-bold" data-id="${rx.id}" data-action="approved">
                Approve Rx
              </button>
            </div>
          ` : `
            <div class="text-[11px] text-slate-500 italic pt-1">
              Reviewed status: ${escapeHtml(rx.status)} • Notes: ${escapeHtml(rx.clinicalNotes || 'Verified')}
            </div>
          `}
        </div>
      `).join('');

      containerEl.querySelectorAll('.admin-rx-action-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
          const rxId = btn.dataset.id;
          const action = btn.dataset.action;
          const notes = prompt(`Clinical notes for ${action} prescription #${rxId}:`, action === 'approved' ? 'Verified against BMDC registered doctor prescription.' : 'Unclear photograph or illegible handwriting.');
          if (notes !== null) {
            try {
              await prescriptionsApi.reviewPrescription(rxId, action, notes);
              toast.success(`Prescription #${rxId} marked as ${action}!`);
              loadAdminPrescriptions(container);
              loadDashboardKpis(container);
            } catch (err) {
              toast.error(err.message || 'Review action failed');
            }
          }
        });
      });
    }

    renderIcons(containerEl);
  } catch (err) {
    containerEl.innerHTML = `<p class="p-4 text-xs text-rose-600">Failed to load prescriptions: ${err.message}</p>`;
  }
}

async function loadAdminInventory(container) {
  const containerEl = container.querySelector('#admin-inventory-table-container');
  try {
    const res = await adminApi.getInventory(1, 20);
    const items = res?.items || [];

    if (items.length === 0) {
      containerEl.innerHTML = `<p class="p-8 text-center text-xs text-slate-400">No inventory records found.</p>`;
    } else {
      containerEl.innerHTML = `
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs text-slate-700">
            <thead class="bg-slate-50 text-[10px] uppercase font-bold text-slate-500 border-b border-slate-200/60">
              <tr>
                <th class="p-3">Product Name</th>
                <th class="p-3">SKU</th>
                <th class="p-3">Available Stock</th>
                <th class="p-3">Reserved</th>
                <th class="p-3">Threshold</th>
                <th class="p-3 text-right">Quick Adjust</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              ${items.map(it => `
                <tr class="hover:bg-slate-50/60">
                  <td class="p-3 font-bold text-slate-900">${escapeHtml(it.productName)}</td>
                  <td class="p-3 font-mono text-slate-500">${escapeHtml(it.sku)}</td>
                  <td class="p-3 font-extrabold ${it.stockQuantity <= it.reorderLevel ? 'text-rose-600' : 'text-slate-900'}">${it.stockQuantity}</td>
                  <td class="p-3 text-slate-500">${it.reservedQuantity ?? 0}</td>
                  <td class="p-3 text-slate-500">${it.reorderLevel ?? 10}</td>
                  <td class="p-3 text-right">
                    <button type="button" class="quick-stock-btn text-xs font-semibold text-brand-600 hover:text-brand-700" data-product-id="${it.productId}">
                      + Add Stock
                    </button>
                  </td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      `;

      containerEl.querySelectorAll('.quick-stock-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
          const productId = parseInt(btn.dataset.productId, 10);
          const addQty = prompt('Enter quantity to add to stock (e.g. 50):', '50');
          if (addQty) {
            const num = parseInt(addQty, 10);
            if (!isNaN(num) && num > 0) {
              try {
                await adminApi.adjustInventory(productId, num, 'purchase_receipt', 'Restocked by admin warehouse audit');
                toast.success('Inventory stock updated!');
                loadAdminInventory(container);
                loadDashboardKpis(container);
              } catch (e) {
                toast.error(e.message || 'Failed to adjust stock');
              }
            }
          }
        });
      });
    }
  } catch (err) {
    containerEl.innerHTML = `<p class="p-4 text-xs text-rose-600">Failed to load inventory: ${err.message}</p>`;
  }
}
