import { authStore } from '../state/authStore.js';
import { cartStore } from '../state/cartStore.js';
import { ordersApi } from '../api/orders.js';
import { paymentsApi } from '../api/payments.js';
import { lookupsApi, addressesApi } from '../api/lookups.js';
import { prescriptionsApi } from '../api/prescriptions.js';
import { toast } from '../state/toastStore.js';
import { escapeHtml, formatCurrency } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export async function renderCheckoutPage() {
  const container = document.createElement('div');
  container.className = 'max-w-5xl mx-auto px-3 sm:px-6 py-4 sm:py-8 space-y-6 sm:space-y-8 w-full max-w-full';

  const authState = authStore.getState();
  if (!authState.isAuthenticated) {
    container.innerHTML = `
      <div class="bg-white rounded-3xl border border-slate-200/80 p-8 sm:p-12 text-center shadow-card space-y-5">
        <div class="w-16 h-16 rounded-2xl bg-brand-50 text-brand-600 flex items-center justify-center mx-auto">
          <i data-lucide="lock" class="w-8 h-8"></i>
        </div>
        <div>
          <h2 class="text-xl font-bold text-slate-900">Sign in to Checkout</h2>
          <p class="text-xs sm:text-sm text-slate-500 max-w-md mx-auto mt-1 leading-relaxed">
            Please log in or create an account to complete your healthcare order, apply coupons, and ensure secure prescription delivery.
          </p>
        </div>
        <div class="flex items-center justify-center gap-3 pt-2">
          <a href="#/login?redirect=checkout" class="btn-primary text-xs py-2.5 px-6 font-bold">
            Sign In to Checkout &rarr;
          </a>
          <a href="#/register" class="btn-secondary text-xs py-2.5 px-5">
            Create Account
          </a>
        </div>
      </div>
    `;
    renderIcons(container);
    return container;
  }

  // Ensure cart is fresh
  await cartStore.fetchCart();
  const cartState = cartStore.getState();
  const items = cartState.cart?.items || [];

  if (items.length === 0) {
    container.innerHTML = `
      <div class="bg-white rounded-3xl border border-slate-200/80 p-12 text-center shadow-sm space-y-4">
        <div class="w-16 h-16 rounded-full bg-slate-100 text-slate-400 flex items-center justify-center mx-auto">
          <i data-lucide="shopping-cart" class="w-8 h-8"></i>
        </div>
        <h2 class="text-lg font-bold text-slate-900">Your Cart is Empty</h2>
        <p class="text-xs text-slate-500">Please add medicines or products to your basket before proceeding to checkout.</p>
        <a href="#/products" class="btn-primary text-xs py-2 px-5 inline-flex font-bold">Browse Products &rarr;</a>
      </div>
    `;
    renderIcons(container);
    return container;
  }

  container.innerHTML = `
    <!-- Header -->
    <div class="border-b border-slate-200/80 pb-4">
      <div class="flex items-center gap-2 text-xs text-brand-700 font-bold uppercase tracking-wider mb-1">
        <i data-lucide="shield-check" class="w-4 h-4"></i> Secure SSL Checkout
      </div>
      <h1 class="text-2xl sm:text-3xl font-black text-slate-900">Complete Your Order</h1>
    </div>

    <!-- Main Checkout Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
      
      <!-- Checkout Form Steps (8 Cols) -->
      <form id="checkout-form" class="lg:col-span-8 space-y-8">
        
        <!-- Step 1: Delivery Address -->
        <section class="bg-white rounded-3xl border border-slate-200/80 p-4 sm:p-7 shadow-sm space-y-4">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <h3 class="text-sm font-bold text-slate-900 flex items-center gap-2">
              <span class="w-6 h-6 rounded-full bg-brand-600 text-white text-xs flex items-center justify-center font-bold">1</span>
              Delivery Address
            </h3>
            <button type="button" id="checkout-add-addr-btn" class="text-xs font-semibold text-brand-600 hover:text-brand-700 min-h-[36px] px-2 flex items-center">
              + New Address
            </button>
          </div>

          <div id="checkout-addresses-list" class="space-y-3">
            <p class="text-xs text-slate-400 py-2">Loading saved addresses...</p>
          </div>

          <!-- Inline New Address Form -->
          <div id="inline-address-form" class="hidden border-t border-slate-100 pt-4 space-y-3">
            <h4 class="text-xs font-bold text-slate-800">Add New Delivery Location</h4>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
              <input type="text" id="chk-addr-name" placeholder="Recipient Name *" class="form-input"/>
              <input type="tel" id="chk-addr-phone" placeholder="Recipient Phone (017XXXXXXXX) *" class="form-input"/>
              <select id="chk-addr-division" class="form-input"><option value="">Division *</option></select>
              <select id="chk-addr-district" class="form-input"><option value="">District *</option></select>
              <select id="chk-addr-upazila" class="form-input"><option value="">Upazila *</option></select>
              <input type="text" id="chk-addr-postal" placeholder="Postal Code" class="form-input"/>
              <input type="text" id="chk-addr-details" placeholder="House, Road, Area Details *" class="form-input sm:col-span-2"/>
            </div>
            <button type="button" id="save-inline-addr-btn" class="btn-primary text-xs py-2 px-4 min-h-[40px]">
              Save and Select Address
            </button>
          </div>
        </section>

        <!-- Step 2: Delivery Method -->
        <section class="bg-white rounded-3xl border border-slate-200/80 p-4 sm:p-7 shadow-sm space-y-4">
          <h3 class="text-sm font-bold text-slate-900 flex items-center gap-2 border-b border-slate-100 pb-3">
            <span class="w-6 h-6 rounded-full bg-brand-600 text-white text-xs flex items-center justify-center font-bold">2</span>
            Delivery Method
          </h3>

          <div id="checkout-delivery-methods-list" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <p class="text-xs text-slate-400 py-2">Loading delivery options...</p>
          </div>
        </section>

        <!-- Step 3: Prescription Validation (if any cart item is Rx) -->
        <section id="checkout-prescription-section" class="bg-white rounded-3xl border border-slate-200/80 p-4 sm:p-7 shadow-sm space-y-4">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <h3 class="text-sm font-bold text-slate-900 flex items-center gap-2">
              <span class="w-6 h-6 rounded-full bg-brand-600 text-white text-xs flex items-center justify-center font-bold">3</span>
              Prescription Verification
            </h3>
            <span id="rx-required-badge" class="badge-prescription">Required for Cart</span>
          </div>

          <div id="checkout-prescriptions-content" class="space-y-3">
            <p class="text-xs text-slate-600 leading-relaxed">
              Your cart contains prescription items. Select an approved prescription from your records below:
            </p>
            <div id="checkout-user-prescriptions" class="space-y-2">
              <p class="text-xs text-slate-400">Loading approved prescriptions...</p>
            </div>
          </div>
        </section>

        <!-- Step 4: Payment Method -->
        <section class="bg-white rounded-3xl border border-slate-200/80 p-4 sm:p-7 shadow-sm space-y-4">
          <h3 class="text-sm font-bold text-slate-900 flex items-center gap-2 border-b border-slate-100 pb-3">
            <span class="w-6 h-6 rounded-full bg-brand-600 text-white text-xs flex items-center justify-center font-bold">4</span>
            Payment Method
          </h3>

          <div id="checkout-payment-methods-list" class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <p class="text-xs text-slate-400 py-2">Loading payment methods...</p>
          </div>
        </section>

        <!-- Special Delivery Notes -->
        <section class="bg-white rounded-3xl border border-slate-200/80 p-4 sm:p-5 shadow-sm space-y-2">
          <label for="checkout-notes" class="form-label font-semibold">Special Delivery Instructions</label>
          <input type="text" id="checkout-notes" placeholder="e.g. Call before delivery, leave with guard" class="form-input"/>
        </section>

      </form>

      <!-- Order Summary Card (4 Cols Sticky) -->
      <aside class="lg:col-span-4 bg-white rounded-3xl border border-slate-200/80 p-4 sm:p-6 shadow-card space-y-5 sticky top-24">
        <h3 class="text-sm font-bold text-slate-900 border-b border-slate-100 pb-3">Order Summary</h3>

        <!-- Line items -->
        <div class="max-h-56 overflow-y-auto divide-y divide-slate-100 text-xs">
          ${items.map(it => `
            <div class="py-2.5 flex items-center justify-between gap-2">
              <div class="min-w-0">
                <div class="font-bold text-slate-900 truncate">${escapeHtml(it.productName)}</div>
                <div class="text-[11px] text-slate-400">${it.quantity} × ${formatCurrency(it.unitPrice)}</div>
              </div>
              <span class="font-bold text-slate-900 flex-shrink-0">${formatCurrency(it.totalPrice)}</span>
            </div>
          `).join('')}
        </div>

        <!-- Totals breakdown -->
        <div class="space-y-2 pt-3 border-t border-slate-100 text-xs text-slate-600">
          <div class="flex justify-between">
            <span>Subtotal</span>
            <span class="font-bold text-slate-900" id="chk-subtotal">${formatCurrency(cartState.cart.subtotalAmount)}</span>
          </div>
          ${cartState.cart.discountAmount > 0 ? `
            <div class="flex justify-between text-emerald-600">
              <span>Discount</span>
              <span class="font-bold">-${formatCurrency(cartState.cart.discountAmount)}</span>
            </div>
          ` : ''}
          <div class="flex justify-between">
            <span>Delivery Fee</span>
            <span class="font-bold text-slate-900" id="chk-delivery-fee">${formatCurrency(cartState.cart.deliveryFee)}</span>
          </div>
          <div class="border-t border-slate-200 pt-3 flex justify-between text-sm font-extrabold text-slate-900">
            <span>Total Payable</span>
            <span class="text-lg text-brand-700 font-black" id="chk-total">${formatCurrency(cartState.cart.totalAmount)}</span>
          </div>
        </div>

        <!-- Place Order Button -->
        <button 
          type="button" 
          id="place-order-submit-btn" 
          class="btn-primary w-full py-3.5 font-bold text-sm shadow-xl shadow-brand-600/25 uppercase tracking-wider"
        >
          Confirm &amp; Place Order &rarr;
        </button>

        <p class="text-[10px] text-slate-400 text-center leading-normal">
          By clicking Place Order, you confirm you agree to our Terms of Use and return policies.
        </p>
      </aside>

    </div>
  `;

  renderIcons(container);
  setupCheckoutWorkflow(container, cartState);

  return container;
}

async function setupCheckoutWorkflow(container, cartState) {
  let selectedAddressId = null;
  let selectedDeliveryMethodId = null;
  let selectedPaymentMethodId = null;
  let selectedPrescriptionId = null;

  // 1. Load Addresses
  const addrListEl = container.querySelector('#checkout-addresses-list');
  try {
    const addresses = await addressesApi.getAddresses();
    if (!addresses || addresses.length === 0) {
      addrListEl.innerHTML = `
        <div class="p-4 bg-amber-50 text-amber-800 rounded-xl text-xs">
          No saved addresses found. Please add a delivery address below.
        </div>
      `;
      container.querySelector('#inline-address-form').classList.remove('hidden');
      setupAddressDropdowns(container);
    } else {
      const defaultAddr = addresses.find(a => a.isDefault || a.isDefaultShipping) || addresses[0];
      selectedAddressId = defaultAddr.id;

      addrListEl.innerHTML = addresses.map(addr => `
        <label class="p-3.5 rounded-2xl border ${addr.id === selectedAddressId ? 'border-brand-500 bg-brand-50/20' : 'border-slate-200'} flex items-start gap-3 cursor-pointer hover:border-brand-400 transition block">
          <input type="radio" name="checkout-address" value="${addr.id}" ${addr.id === selectedAddressId ? 'checked' : ''} class="mt-1 text-brand-600 focus:ring-brand-500"/>
          <div class="text-xs min-w-0">
            <div class="font-bold text-slate-900">${escapeHtml(addr.recipientName)} (${escapeHtml(addr.recipientPhone)})</div>
            <div class="text-slate-600 mt-0.5">${escapeHtml(addr.areaDetails)}, ${escapeHtml(addr.upazila || '')}, ${escapeHtml(addr.districtName || addr.district || '')}</div>
          </div>
        </label>
      `).join('');

      addrListEl.querySelectorAll('input[name="checkout-address"]').forEach(r => {
        r.addEventListener('change', () => {
          selectedAddressId = parseInt(r.value, 10);
        });
      });
    }
  } catch (e) {
    addrListEl.innerHTML = `<p class="text-xs text-rose-600">Failed to load addresses</p>`;
  }

  // Toggle inline address
  container.querySelector('#checkout-add-addr-btn')?.addEventListener('click', () => {
    const form = container.querySelector('#inline-address-form');
    form.classList.toggle('hidden');
    if (!form.classList.contains('hidden')) {
      setupAddressDropdowns(container);
    }
  });

  // Save inline address
  container.querySelector('#save-inline-addr-btn')?.addEventListener('click', async () => {
    const recipientName = container.querySelector('#chk-addr-name').value.trim();
    const recipientPhone = container.querySelector('#chk-addr-phone').value.trim();
    const division = container.querySelector('#chk-addr-division').value;
    const district = container.querySelector('#chk-addr-district').value;
    const upazila = container.querySelector('#chk-addr-upazila').value;
    const postalCode = container.querySelector('#chk-addr-postal').value.trim();
    const areaDetails = container.querySelector('#chk-addr-details').value.trim();

    if (!recipientName || !recipientPhone || !division || !district || !upazila || !areaDetails) {
      toast.error('Please fill in all required address fields.');
      return;
    }

    try {
      const newAddr = await addressesApi.createAddress({
        recipientName,
        recipientPhone,
        division,
        district,
        upazila,
        postalCode: postalCode || undefined,
        areaDetails,
        isDefaultShipping: true,
      });

      toast.success('Address added and selected!');
      container.querySelector('#inline-address-form').classList.add('hidden');
      selectedAddressId = newAddr.id;

      // Re-render addresses
      const addresses = await addressesApi.getAddresses();
      addrListEl.innerHTML = addresses.map(addr => `
        <label class="p-3.5 rounded-2xl border ${addr.id === newAddr.id ? 'border-brand-500 bg-brand-50/20' : 'border-slate-200'} flex items-start gap-3 cursor-pointer hover:border-brand-400 transition block">
          <input type="radio" name="checkout-address" value="${addr.id}" ${addr.id === newAddr.id ? 'checked' : ''} class="mt-1 text-brand-600 focus:ring-brand-500"/>
          <div class="text-xs min-w-0">
            <div class="font-bold text-slate-900">${escapeHtml(addr.recipientName)} (${escapeHtml(addr.recipientPhone)})</div>
            <div class="text-slate-600 mt-0.5">${escapeHtml(addr.areaDetails)}, ${escapeHtml(addr.upazila)}, ${escapeHtml(addr.district)}</div>
          </div>
        </label>
      `).join('');
    } catch (err) {
      toast.error(err.message || 'Failed to save address');
    }
  });

  // 2. Load Delivery Methods
  const delivListEl = container.querySelector('#checkout-delivery-methods-list');
  try {
    const deliveryMethods = await lookupsApi.getDeliveryMethods();
    if (deliveryMethods && deliveryMethods.length > 0) {
      selectedDeliveryMethodId = deliveryMethods[0].id;
      delivListEl.innerHTML = deliveryMethods.map((dm, i) => `
        <label class="p-3.5 rounded-2xl border ${i === 0 ? 'border-brand-500 bg-brand-50/20' : 'border-slate-200'} flex items-start gap-3 cursor-pointer hover:border-brand-400 transition block">
          <input type="radio" name="checkout-delivery" value="${dm.id}" data-fee="${dm.baseFee}" ${i === 0 ? 'checked' : ''} class="mt-1 text-brand-600 focus:ring-brand-500"/>
          <div class="text-xs min-w-0 flex-1">
            <div class="flex items-center justify-between">
              <span class="font-bold text-slate-900">${escapeHtml(dm.name)}</span>
              <span class="font-bold text-brand-700">${formatCurrency(dm.baseFee)}</span>
            </div>
            <div class="text-slate-500 text-[11px] mt-0.5">${dm.estimatedDaysMin}-${dm.estimatedDaysMax} business days</div>
          </div>
        </label>
      `).join('');

      delivListEl.querySelectorAll('input[name="checkout-delivery"]').forEach(r => {
        r.addEventListener('change', () => {
          selectedDeliveryMethodId = parseInt(r.value, 10);
          const fee = parseFloat(r.dataset.fee || '0');
          container.querySelector('#chk-delivery-fee').textContent = formatCurrency(fee);
          const subtotal = cartState.cart.subtotalAmount || 0;
          const discount = cartState.cart.discountAmount || 0;
          container.querySelector('#chk-total').textContent = formatCurrency((subtotal - discount) + fee);
        });
      });
    }
  } catch (e) {
    delivListEl.innerHTML = `<p class="text-xs text-rose-600">Failed to load delivery methods</p>`;
  }

  // 3. Load Prescriptions (if required)
  const rxSection = container.querySelector('#checkout-prescription-section');
  const rxListEl = container.querySelector('#checkout-user-prescriptions');
  if (cartState.hasPrescriptionItems) {
    try {
      const rxData = await prescriptionsApi.getMyPrescriptions();
      const approvedRx = (rxData?.items || []).filter(r => r.status === 'approved');

      if (approvedRx.length === 0) {
        rxListEl.innerHTML = `
          <div class="p-4 bg-rose-50 border border-rose-200 rounded-xl text-xs text-rose-800 space-y-2">
            <p class="font-bold">No approved prescriptions found!</p>
            <p class="text-[11px]">Your cart contains prescription-required medications. Under pharmacy guidelines, an approved prescription is mandatory before completing order checkout.</p>
            <a href="#/prescriptions/upload" class="btn-primary text-xs py-1.5 px-3 inline-flex font-bold">
              Upload Prescription Now &rarr;
            </a>
          </div>
        `;
      } else {
        selectedPrescriptionId = approvedRx[0].id;
        rxListEl.innerHTML = approvedRx.map((rx, i) => `
          <label class="p-3 rounded-2xl border ${i === 0 ? 'border-brand-500 bg-brand-50/20' : 'border-slate-200'} flex items-center justify-between gap-3 cursor-pointer text-xs">
            <div class="flex items-center gap-2">
              <input type="radio" name="checkout-rx" value="${rx.id}" ${i === 0 ? 'checked' : ''} class="text-brand-600 focus:ring-brand-500"/>
              <div>
                <span class="font-bold text-slate-900">Rx #${rx.id}</span> • ${escapeHtml(rx.patientName)}
                <span class="text-[10px] text-slate-400 block">${rx.doctorName || 'Doctor Prescription'}</span>
              </div>
            </div>
            <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-emerald-100 text-emerald-800">APPROVED</span>
          </label>
        `).join('');

        rxListEl.querySelectorAll('input[name="checkout-rx"]').forEach(r => {
          r.addEventListener('change', () => {
            selectedPrescriptionId = parseInt(r.value, 10);
          });
        });
      }
    } catch (e) {
      rxListEl.innerHTML = `<p class="text-xs text-rose-600">Failed to load prescriptions</p>`;
    }
  } else {
    rxSection.classList.add('hidden');
  }

  // 4. Load Payment Methods
  const payListEl = container.querySelector('#checkout-payment-methods-list');
  try {
    const paymentMethods = await lookupsApi.getPaymentMethods();
    if (paymentMethods && paymentMethods.length > 0) {
      selectedPaymentMethodId = paymentMethods[0].id;
      payListEl.innerHTML = paymentMethods.map((pm, i) => {
        let badgeColor = 'bg-slate-100 text-slate-700';
        if (pm.code === 'bkash') badgeColor = 'bg-pink-100 text-pink-700';
        else if (pm.code === 'nagad') badgeColor = 'bg-orange-100 text-orange-700';
        else if (pm.code === 'cod') badgeColor = 'bg-emerald-100 text-emerald-700';

        return `
          <label class="p-3.5 rounded-2xl border ${i === 0 ? 'border-brand-500 bg-brand-50/20' : 'border-slate-200'} flex items-start gap-3 cursor-pointer hover:border-brand-400 transition block">
            <input type="radio" name="checkout-payment" value="${pm.id}" data-code="${pm.code}" ${i === 0 ? 'checked' : ''} class="mt-1 text-brand-600 focus:ring-brand-500"/>
            <div class="text-xs min-w-0">
              <span class="font-bold text-slate-900 block">${escapeHtml(pm.name)}</span>
              <span class="text-[10px] text-slate-500 mt-0.5 block">${escapeHtml(pm.nameBn || '')}</span>
            </div>
          </label>
        `;
      }).join('');

      payListEl.querySelectorAll('input[name="checkout-payment"]').forEach(r => {
        r.addEventListener('change', () => {
          selectedPaymentMethodId = parseInt(r.value, 10);
        });
      });
    }
  } catch (e) {
    payListEl.innerHTML = `<p class="text-xs text-rose-600">Failed to load payment methods</p>`;
  }

  // Submit Order
  const submitBtn = container.querySelector('#place-order-submit-btn');
  submitBtn?.addEventListener('click', async () => {
    const checkedAddr = container.querySelector('input[name="checkout-address"]:checked');
    const checkedDeliv = container.querySelector('input[name="checkout-delivery"]:checked');
    const checkedPay = container.querySelector('input[name="checkout-payment"]:checked');
    const checkedRx = container.querySelector('input[name="checkout-rx"]:checked');

    const addressId = checkedAddr ? parseInt(checkedAddr.value, 10) : selectedAddressId;
    const deliveryMethodId = checkedDeliv ? parseInt(checkedDeliv.value, 10) : selectedDeliveryMethodId;
    const paymentMethodId = checkedPay ? parseInt(checkedPay.value, 10) : selectedPaymentMethodId;
    const prescriptionId = checkedRx ? parseInt(checkedRx.value, 10) : selectedPrescriptionId;

    if (!addressId) {
      toast.error('Please select or add a delivery address.');
      return;
    }
    if (!deliveryMethodId) {
      toast.error('Please select a delivery method.');
      return;
    }
    if (!paymentMethodId) {
      toast.error('Please select a payment method.');
      return;
    }
    if (cartState.hasPrescriptionItems && !prescriptionId) {
      toast.error('An approved prescription is required for medicines in your cart.');
      return;
    }

    const customerNotes = container.querySelector('#checkout-notes')?.value.trim();
    const idempotencyKey = crypto.randomUUID();

    submitBtn.disabled = true;
    const origHtml = submitBtn.innerHTML;
    submitBtn.innerHTML = `<i data-lucide="loader" class="w-4 h-4 animate-spin"></i> Processing Order...`;
    renderIcons(submitBtn);

    try {
      const order = await ordersApi.checkout({
        deliveryAddressId: addressId,
        addressId: addressId,
        deliveryMethodId: deliveryMethodId,
        paymentMethodId: paymentMethodId,
        prescriptionId: prescriptionId,
        customerNotes: customerNotes || undefined,
        idempotencyKey,
      });

      // Clear local cart store
      await cartStore.fetchCart();

      toast.success('Order placed successfully!');
      window.location.hash = `#/order-confirmed/${order.id}`;
    } catch (err) {
      toast.error(err.message || 'Checkout failed. Please check stock and details.');
      submitBtn.disabled = false;
      submitBtn.innerHTML = origHtml;
      renderIcons(submitBtn);
    }
  });
}

async function setupAddressDropdowns(container) {
  const divSelect = container.querySelector('#chk-addr-division');
  const distSelect = container.querySelector('#chk-addr-district');
  const upzSelect = container.querySelector('#chk-addr-upazila');

  try {
    const divisions = await lookupsApi.getDivisions();
    divSelect.innerHTML = '<option value="">Select Division</option>';
    divisions.forEach(d => {
      const opt = document.createElement('option');
      opt.value = d.name;
      opt.dataset.id = d.id;
      opt.textContent = `${d.name} (${d.nameBn || ''})`;
      divSelect.appendChild(opt);
    });

    divSelect.onchange = async () => {
      const opt = divSelect.selectedOptions[0];
      distSelect.innerHTML = '<option value="">Select District</option>';
      upzSelect.innerHTML = '<option value="">Select Upazila</option>';
      if (opt?.dataset.id) {
        const districts = await lookupsApi.getDistricts(opt.dataset.id);
        districts.forEach(dst => {
          const o = document.createElement('option');
          o.value = dst.name;
          o.dataset.id = dst.id;
          o.textContent = dst.name;
          distSelect.appendChild(o);
        });
      }
    };

    distSelect.onchange = async () => {
      const opt = distSelect.selectedOptions[0];
      upzSelect.innerHTML = '<option value="">Select Upazila</option>';
      if (opt?.dataset.id) {
        const upazilas = await lookupsApi.getUpazilas(opt.dataset.id);
        upazilas.forEach(u => {
          const o = document.createElement('option');
          o.value = u.name;
          o.textContent = u.name;
          upzSelect.appendChild(o);
        });
      }
    };
  } catch (e) {
    console.warn('Address dropdown failed:', e);
  }
}
