import { cartStore } from '../state/cartStore.js';
import { escapeHtml, formatCurrency } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export function createCartDrawer() {
  const container = document.createElement('div');
  container.id = 'cart-drawer-root';
  container.className = 'fixed inset-0 z-50 pointer-events-none transition-opacity duration-300';

  container.innerHTML = `
    <!-- Backdrop -->
    <div id="cart-drawer-backdrop" class="fixed inset-0 bg-slate-950/40 backdrop-blur-sm opacity-0 transition-opacity duration-300 pointer-events-none"></div>

    <!-- Slide-over panel -->
    <div id="cart-drawer-panel" class="fixed inset-y-0 right-0 max-w-md w-full bg-white shadow-2xl flex flex-col transform translate-x-full transition-transform duration-300 ease-in-out pointer-events-auto">
      
      <!-- Drawer Header -->
      <div class="px-5 py-4 border-b border-slate-200/80 flex items-center justify-between bg-slate-50/50">
        <div class="flex items-center gap-2">
          <div class="w-8 h-8 rounded-lg bg-brand-100 text-brand-700 flex items-center justify-center">
            <i data-lucide="shopping-bag" class="w-4 h-4"></i>
          </div>
          <div>
            <h3 class="text-sm font-bold text-slate-900">Your Cart</h3>
            <p class="text-[11px] text-slate-500" id="cart-drawer-item-count">0 items</p>
          </div>
        </div>
        <button id="cart-drawer-close-btn" class="p-2 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition">
          <i data-lucide="x" class="w-5 h-5"></i>
        </button>
      </div>

      <!-- Prescription Warning Banner (if applicable) -->
      <div id="cart-prescription-banner" class="hidden px-4 py-2.5 bg-rose-50 border-b border-rose-200/80 text-rose-800 text-xs flex items-center gap-2">
        <i data-lucide="alert-circle" class="w-4 h-4 text-rose-600 flex-shrink-0"></i>
        <span>Contains prescription medicines. Valid prescription required at checkout.</span>
      </div>

      <!-- Drawer Body: Items list or Empty state -->
      <div class="flex-1 overflow-y-auto p-5 divide-y divide-slate-100" id="cart-drawer-items-list">
        <!-- Rendered items go here -->
      </div>

      <!-- Drawer Footer: Coupon, Summary, Checkout -->
      <div class="border-t border-slate-200/80 bg-slate-50/50 p-5 space-y-4" id="cart-drawer-footer">
        
        <!-- Coupon Form -->
        <div id="cart-coupon-section" class="space-y-2">
          <form id="cart-coupon-form" class="flex gap-2">
            <input 
              type="text" 
              id="cart-coupon-input" 
              placeholder="Promo / Coupon code" 
              class="form-input text-base sm:text-xs uppercase font-mono py-2 flex-1"
            />
            <button type="submit" id="cart-coupon-btn" class="btn-secondary text-xs px-3 py-2 flex-shrink-0 min-h-[40px]">
              Apply
            </button>
          </form>
          <div id="cart-coupon-applied-badge" class="hidden flex items-center justify-between text-xs bg-emerald-50 text-emerald-800 px-3 py-1.5 rounded-lg border border-emerald-200">
            <span class="flex items-center gap-1 font-semibold">
              <i data-lucide="tag" class="w-3.5 h-3.5 text-emerald-600"></i>
              <span id="applied-coupon-name">COUPON</span>
            </span>
            <button type="button" id="remove-coupon-btn" class="text-emerald-700 hover:text-rose-600 text-xs font-medium">Remove</button>
          </div>
        </div>

        <!-- Price breakdown -->
        <div class="space-y-1.5 text-xs text-slate-600">
          <div class="flex justify-between">
            <span>Subtotal</span>
            <span class="font-semibold text-slate-900" id="cart-subtotal">৳0.00</span>
          </div>
          <div class="flex justify-between text-emerald-600" id="cart-discount-row">
            <span>Discount</span>
            <span class="font-semibold" id="cart-discount">-৳0.00</span>
          </div>
          <div class="flex justify-between">
            <span class="flex items-center gap-1">
              Delivery Fee
              <i data-lucide="info" class="w-3 h-3 text-slate-400" title="Calculated at checkout based on location"></i>
            </span>
            <span class="font-semibold text-slate-900" id="cart-delivery-fee">৳0.00</span>
          </div>
          <div class="border-t border-slate-200 pt-2 flex justify-between text-sm font-bold text-slate-900">
            <span>Estimated Total</span>
            <span class="text-base text-brand-700 font-extrabold" id="cart-total">৳0.00</span>
          </div>
        </div>

        <!-- Checkout Action -->
        <div class="space-y-2 pt-1">
          <a href="#/checkout" id="cart-checkout-btn" class="btn-primary w-full text-sm py-3 font-bold shadow-md shadow-brand-600/20 text-center block">
            Proceed to Checkout &rarr;
          </a>
          <button type="button" id="cart-continue-btn" class="w-full text-xs text-center text-slate-500 hover:text-slate-800 font-medium py-1">
            Continue Shopping
          </button>
        </div>
      </div>
    </div>
  `;

  // Attach events
  setupCartDrawerEvents(container);

  // Subscribe to cart updates
  cartStore.subscribe(state => renderCartDrawerState(container, state));

  return container;
}

function setupCartDrawerEvents(container) {
  const backdrop = container.querySelector('#cart-drawer-backdrop');
  const panel = container.querySelector('#cart-drawer-panel');
  const closeBtn = container.querySelector('#cart-drawer-close-btn');
  const continueBtn = container.querySelector('#cart-continue-btn');
  const checkoutBtn = container.querySelector('#cart-checkout-btn');

  const closeDrawer = () => cartStore.closeDrawer();

  backdrop?.addEventListener('click', closeDrawer);
  closeBtn?.addEventListener('click', closeDrawer);
  continueBtn?.addEventListener('click', closeDrawer);
  checkoutBtn?.addEventListener('click', () => {
    closeDrawer();
  });

  // Close Cart on Escape
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && cartStore.getState().isDrawerOpen) {
      closeDrawer();
    }
  });

  // Apply Coupon
  const couponForm = container.querySelector('#cart-coupon-form');
  const couponInput = container.querySelector('#cart-coupon-input');
  couponForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const code = couponInput?.value.trim();
    if (code) {
      try {
        await cartStore.applyCoupon(code);
        couponInput.value = '';
      } catch (err) {
        // error toasted
      }
    }
  });

  // Remove Coupon
  container.querySelector('#remove-coupon-btn')?.addEventListener('click', async () => {
    try {
      await cartStore.removeCoupon();
    } catch (err) {
      // error toasted
    }
  });
}

function renderCartDrawerState(container, state) {
  const backdrop = container.querySelector('#cart-drawer-backdrop');
  const panel = container.querySelector('#cart-drawer-panel');

  if (state.isDrawerOpen) {
    container.classList.remove('pointer-events-none');
    backdrop?.classList.remove('opacity-0', 'pointer-events-none');
    backdrop?.classList.add('opacity-100');
    panel?.classList.remove('translate-x-full');
    document.body.classList.add('overflow-hidden');
  } else {
    container.classList.add('pointer-events-none');
    backdrop?.classList.add('opacity-0', 'pointer-events-none');
    backdrop?.classList.remove('opacity-100');
    panel?.classList.add('translate-x-full');
    document.body.classList.remove('overflow-hidden');
  }

  // Count
  const itemCountEl = container.querySelector('#cart-drawer-item-count');
  if (itemCountEl) {
    itemCountEl.textContent = `${state.itemCount} ${state.itemCount === 1 ? 'item' : 'items'}`;
  }

  // Prescription banner
  const rxBanner = container.querySelector('#cart-prescription-banner');
  if (state.hasPrescriptionItems) {
    rxBanner?.classList.remove('hidden');
  } else {
    rxBanner?.classList.add('hidden');
  }

  // Items list
  const listEl = container.querySelector('#cart-drawer-items-list');
  const footerEl = container.querySelector('#cart-drawer-footer');
  const items = state.cart?.items || [];

  if (items.length === 0) {
    listEl.innerHTML = `
      <div class="h-full flex flex-col items-center justify-center text-center p-6 space-y-4">
        <div class="w-16 h-16 rounded-full bg-slate-100 text-slate-400 flex items-center justify-center">
          <i data-lucide="shopping-cart" class="w-8 h-8"></i>
        </div>
        <div>
          <h4 class="font-bold text-sm text-slate-800">Your cart is empty</h4>
          <p class="text-xs text-slate-500 mt-1 max-w-xs">Looks like you haven't added any medicines or health products to your basket yet.</p>
        </div>
        <a href="#/products" class="btn-primary text-xs py-2 px-4" onclick="document.querySelector('#cart-drawer-close-btn').click()">
          Browse Products
        </a>
      </div>
    `;
    footerEl?.classList.add('hidden');
  } else {
    footerEl?.classList.remove('hidden');

    listEl.innerHTML = items.map(item => `
      <div class="py-3.5 flex items-start gap-3 group">
        <!-- Thumbnail -->
        <div class="w-14 h-14 rounded-lg bg-slate-50 border border-slate-200/70 p-1 flex-shrink-0 flex items-center justify-center overflow-hidden">
          <img 
            src="${escapeHtml(item.productImageUrl || '/assets/medicine-placeholder.png')}" 
            alt="${escapeHtml(item.productName)}" 
            class="max-h-full max-w-full object-contain"
            onerror="this.src='data:image/svg+xml;utf8,<svg xmlns=\\'http://www.w3.org/2000/svg\\' width=\\'32\\' height=\\'32\\' viewBox=\\'0 0 24 24\\' fill=\\'none\\' stroke=\\'%2394a3b8\\' stroke-width=\\'2\\'><path d=\\'m10.5 20.5 10-10a4.95 4.95 0 1 0-7-7l-10 10a4.95 4.95 0 1 0 7 7Z\\'/></svg>'"
          />
        </div>

        <!-- Info -->
        <div class="flex-1 min-w-0">
          <div class="flex items-start justify-between gap-1">
            <h4 class="text-xs font-bold text-slate-900 leading-tight truncate">${escapeHtml(item.productName)}</h4>
            <button type="button" class="cart-item-remove-btn text-slate-400 hover:text-rose-600 p-0.5" data-item-id="${item.id}" title="Remove item">
              <i data-lucide="trash-2" class="w-3.5 h-3.5"></i>
            </button>
          </div>
          
          <div class="text-[11px] text-slate-500 mt-0.5 truncate">
            ${escapeHtml(item.strength || '')} • Unit: ${formatCurrency(item.unitPrice)}
          </div>

          ${item.isPrescriptionRequired ? `
            <span class="inline-block px-1.5 py-0.2 rounded text-[10px] font-bold bg-rose-50 text-rose-700 border border-rose-200 mt-1">
              Rx Required
            </span>
          ` : ''}

          <!-- Quantity Stepper & Line Total -->
          <div class="flex items-center justify-between mt-2">
            <div class="inline-flex items-center border border-slate-200 rounded-lg bg-white overflow-hidden shadow-sm">
              <button type="button" class="cart-qty-btn p-1 px-2 text-slate-600 hover:bg-slate-100 disabled:opacity-40" data-item-id="${item.id}" data-action="decrease" ${item.quantity <= 1 ? '' : ''}>
                <i data-lucide="minus" class="w-3 h-3"></i>
              </button>
              <span class="px-2.5 text-xs font-bold text-slate-800">${item.quantity}</span>
              <button type="button" class="cart-qty-btn p-1 px-2 text-slate-600 hover:bg-slate-100" data-item-id="${item.id}" data-action="increase">
                <i data-lucide="plus" class="w-3 h-3"></i>
              </button>
            </div>

            <div class="text-right">
              <div class="text-xs font-extrabold text-slate-900">${formatCurrency(item.totalPrice)}</div>
            </div>
          </div>
        </div>
      </div>
    `).join('');

    // Attach quantity and remove buttons
    listEl.querySelectorAll('.cart-qty-btn').forEach(btn => {
      btn.addEventListener('click', async () => {
        const itemId = parseInt(btn.dataset.itemId, 10);
        const action = btn.dataset.action;
        const currentItem = items.find(i => i.id === itemId);
        if (currentItem) {
          const newQty = action === 'increase' ? currentItem.quantity + 1 : currentItem.quantity - 1;
          await cartStore.updateQuantity(itemId, newQty);
        }
      });
    });

    listEl.querySelectorAll('.cart-item-remove-btn').forEach(btn => {
      btn.addEventListener('click', async () => {
        const itemId = parseInt(btn.dataset.itemId, 10);
        await cartStore.removeItem(itemId);
      });
    });

    // Summary numbers
    const subtotal = state.cart?.subtotalAmount || 0;
    const discount = state.cart?.discountAmount || 0;
    const delivery = state.cart?.deliveryFee || 0;
    const total = state.cart?.totalAmount || 0;

    container.querySelector('#cart-subtotal').textContent = formatCurrency(subtotal);
    container.querySelector('#cart-discount').textContent = `-${formatCurrency(discount)}`;
    container.querySelector('#cart-delivery-fee').textContent = delivery > 0 ? formatCurrency(delivery) : 'Calculated next';
    container.querySelector('#cart-total').textContent = formatCurrency(total);

    // Coupon UI
    const appliedCoupon = state.cart?.couponCode;
    const couponAppliedBadge = container.querySelector('#cart-coupon-applied-badge');
    const couponForm = container.querySelector('#cart-coupon-form');
    if (appliedCoupon) {
      couponAppliedBadge?.classList.remove('hidden');
      couponForm?.classList.add('hidden');
      container.querySelector('#applied-coupon-name').textContent = appliedCoupon;
    } else {
      couponAppliedBadge?.classList.add('hidden');
      couponForm?.classList.remove('hidden');
    }
  }

  renderIcons(container);
}
