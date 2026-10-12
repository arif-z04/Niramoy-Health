import { authStore } from '../state/authStore.js';
import { addressesApi, lookupsApi } from '../api/lookups.js';
import { toast } from '../state/toastStore.js';
import { escapeHtml } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export async function renderAccountPage() {
  const container = document.createElement('div');
  container.className = 'max-w-5xl mx-auto px-4 sm:px-6 py-8 space-y-8';

  const authState = authStore.getState();
  if (!authState.isAuthenticated) {
    container.innerHTML = `
      <div class="bg-white rounded-3xl border border-slate-200/80 p-8 text-center shadow-card space-y-4">
        <h2 class="text-xl font-bold text-slate-900">Sign in to view your profile</h2>
        <a href="#/login?redirect=account" class="btn-primary text-xs py-2 px-5 inline-flex">Sign In</a>
      </div>
    `;
    return container;
  }

  container.innerHTML = `
    <!-- Header -->
    <div class="border-b border-slate-200/80 pb-4">
      <h1 class="text-2xl font-bold text-slate-900">Account &amp; Addresses</h1>
      <p class="text-xs text-slate-500">Manage your personal profile and saved delivery locations</p>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      
      <!-- Profile Card -->
      <div class="lg:col-span-1 bg-white rounded-3xl border border-slate-200/80 p-6 shadow-sm space-y-5 h-fit">
        <div class="flex items-center gap-3 border-b border-slate-100 pb-4">
          <div class="w-12 h-12 rounded-2xl bg-brand-600 text-white font-bold text-lg flex items-center justify-center">
            ${(authState.user?.fullName?.charAt(0) || 'U').toUpperCase()}
          </div>
          <div class="min-w-0">
            <h3 class="text-sm font-bold text-slate-900 truncate">${escapeHtml(authState.user?.fullName)}</h3>
            <p class="text-xs text-slate-500 truncate">${escapeHtml(authState.user?.phoneNumber || '')}</p>
          </div>
        </div>

        <form id="profile-edit-form" class="space-y-4">
          <div>
            <label class="form-label font-semibold">Full Name</label>
            <input type="text" id="prof-name" value="${escapeHtml(authState.user?.fullName || '')}" required class="form-input"/>
          </div>

          <div>
            <label class="form-label font-semibold">Email Address</label>
            <input type="email" id="prof-email" value="${escapeHtml(authState.user?.email || '')}" class="form-input"/>
          </div>

          <div>
            <label class="form-label font-semibold">Gender</label>
            <select id="prof-gender" class="form-input">
              <option value="" ${!authState.user?.gender ? 'selected' : ''}>Not specified</option>
              <option value="male" ${authState.user?.gender === 'male' ? 'selected' : ''}>Male</option>
              <option value="female" ${authState.user?.gender === 'female' ? 'selected' : ''}>Female</option>
              <option value="other" ${authState.user?.gender === 'other' ? 'selected' : ''}>Other</option>
            </select>
          </div>

          <button type="submit" id="save-profile-btn" class="btn-primary w-full text-xs py-2.5">
            Save Profile Changes
          </button>
        </form>

        <div class="border-t border-slate-100 pt-4">
          <button type="button" id="logout-btn" class="w-full btn-secondary text-rose-600 hover:text-rose-700 text-xs py-2">
            <i data-lucide="log-out" class="w-3.5 h-3.5"></i> Sign Out
          </button>
        </div>
      </div>

      <!-- Saved Addresses Column -->
      <div class="lg:col-span-2 space-y-5">
        <div class="bg-white rounded-3xl border border-slate-200/80 p-6 shadow-sm space-y-5">
          <div class="flex items-center justify-between border-b border-slate-100 pb-4">
            <div>
              <h3 class="text-sm font-bold text-slate-900">Saved Delivery Addresses</h3>
              <p class="text-xs text-slate-500">Addresses available during checkout</p>
            </div>
            <button type="button" id="add-address-modal-btn" class="btn-secondary text-xs py-2 px-3 font-semibold">
              <i data-lucide="plus" class="w-3.5 h-3.5"></i> Add Address
            </button>
          </div>

          <!-- Address list -->
          <div id="addresses-list" class="space-y-3">
            <p class="text-xs text-slate-400 text-center py-4">Loading saved addresses...</p>
          </div>
        </div>

        <!-- Add Address Form (Hidden by default, expands when Add clicked) -->
        <div id="add-address-card" class="hidden bg-white rounded-3xl border border-brand-200 p-6 shadow-md space-y-4 animate-slide-down">
          <div class="flex items-center justify-between border-b border-slate-100 pb-3">
            <h4 class="text-xs font-bold text-brand-700 uppercase tracking-wider">New Delivery Address</h4>
            <button type="button" id="cancel-add-address-btn" class="text-slate-400 hover:text-slate-600 p-1">
              <i data-lucide="x" class="w-4 h-4"></i>
            </button>
          </div>

          <form id="new-address-form" class="space-y-4">
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="form-label">Recipient Name *</label>
                <input type="text" id="addr-name" required placeholder="Mohammad Rahim" class="form-input"/>
              </div>
              <div>
                <label class="form-label">Recipient Mobile Number *</label>
                <input type="tel" id="addr-phone" required placeholder="017XXXXXXXX" class="form-input"/>
              </div>

              <div>
                <label class="form-label">Division *</label>
                <select id="addr-division" required class="form-input">
                  <option value="">Select Division</option>
                </select>
              </div>

              <div>
                <label class="form-label">District *</label>
                <select id="addr-district" required class="form-input">
                  <option value="">Select District</option>
                </select>
              </div>

              <div>
                <label class="form-label">Upazila / Area *</label>
                <select id="addr-upazila" required class="form-input">
                  <option value="">Select Upazila</option>
                </select>
              </div>

              <div>
                <label class="form-label">Postal Code</label>
                <input type="text" id="addr-postal" placeholder="1212" class="form-input"/>
              </div>

              <div class="sm:col-span-2">
                <label class="form-label">Street / House / Detailed Address *</label>
                <input type="text" id="addr-details" required placeholder="House 12, Road 4, Block D, Banani" class="form-input"/>
              </div>

              <div class="sm:col-span-2">
                <label class="form-label">Delivery Instructions (Optional)</label>
                <input type="text" id="addr-instructions" placeholder="e.g. Ring the bell twice, 4th floor" class="form-input"/>
              </div>
            </div>

            <div class="flex items-center gap-2 pt-1 text-xs">
              <input type="checkbox" id="addr-default" class="rounded text-brand-600 focus:ring-brand-500"/>
              <label for="addr-default">Set as default shipping address</label>
            </div>

            <div class="flex justify-end gap-2 pt-2">
              <button type="button" id="discard-addr-btn" class="btn-secondary text-xs py-2 px-4">Cancel</button>
              <button type="submit" id="save-addr-btn" class="btn-primary text-xs py-2 px-5 font-bold">Save Address</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `;

  renderIcons(container);
  setupAccountEvents(container);

  return container;
}

async function setupAccountEvents(container) {
  // Profile update
  const profileForm = container.querySelector('#profile-edit-form');
  profileForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const fullName = container.querySelector('#prof-name').value.trim();
    const email = container.querySelector('#prof-email').value.trim();
    const gender = container.querySelector('#prof-gender').value;

    try {
      await authStore.updateProfile({
        fullName,
        email: email || undefined,
        gender: gender || undefined,
      });
      toast.success('Profile updated successfully!');
    } catch (err) {
      toast.error(err.message || 'Failed to update profile');
    }
  });

  // Logout
  container.querySelector('#logout-btn')?.addEventListener('click', async () => {
    await authStore.logout();
    window.location.hash = '#/';
  });

  // Address add form toggle
  const addAddrCard = container.querySelector('#add-address-card');
  const addModalBtn = container.querySelector('#add-address-modal-btn');
  const cancelBtn = container.querySelector('#cancel-add-address-btn');
  const discardBtn = container.querySelector('#discard-addr-btn');

  const toggleAdd = (show) => {
    if (show) {
      addAddrCard.classList.remove('hidden');
      loadAddressDropdowns(container);
    } else {
      addAddrCard.classList.add('hidden');
    }
  };

  addModalBtn?.addEventListener('click', () => toggleAdd(true));
  cancelBtn?.addEventListener('click', () => toggleAdd(false));
  discardBtn?.addEventListener('click', () => toggleAdd(false));

  // Load addresses
  await loadAddressesList(container);

  // Address form submit
  const addrForm = container.querySelector('#new-address-form');
  addrForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const recipientName = container.querySelector('#addr-name').value.trim();
    const recipientPhone = container.querySelector('#addr-phone').value.trim();
    const division = container.querySelector('#addr-division').value;
    const district = container.querySelector('#addr-district').value;
    const upazila = container.querySelector('#addr-upazila').value;
    const postalCode = container.querySelector('#addr-postal').value.trim();
    const areaDetails = container.querySelector('#addr-details').value.trim();
    const deliveryInstructions = container.querySelector('#addr-instructions').value.trim();
    const isDefaultShipping = container.querySelector('#addr-default').checked;

    try {
      await addressesApi.createAddress({
        recipientName,
        recipientPhone,
        division,
        district,
        upazila,
        postalCode: postalCode || undefined,
        areaDetails,
        deliveryInstructions: deliveryInstructions || undefined,
        isDefaultShipping,
      });

      toast.success('Address saved successfully!');
      addrForm.reset();
      toggleAdd(false);
      await loadAddressesList(container);
    } catch (err) {
      toast.error(err.message || 'Failed to save address');
    }
  });
}

async function loadAddressesList(container) {
  const listEl = container.querySelector('#addresses-list');
  try {
    const addresses = await addressesApi.getAddresses();
    if (!addresses || addresses.length === 0) {
      listEl.innerHTML = `
        <div class="text-center py-6 text-slate-400 text-xs">
          No addresses saved yet. Click "Add Address" to register a delivery location.
        </div>
      `;
    } else {
      listEl.innerHTML = addresses.map(addr => `
        <div class="p-4 rounded-2xl border ${addr.isDefaultShipping ? 'border-brand-300 bg-brand-50/20' : 'border-slate-200 bg-white'} flex items-start justify-between gap-4">
          <div class="space-y-1">
            <div class="flex items-center gap-2">
              <strong class="text-xs font-bold text-slate-900">${escapeHtml(addr.recipientName)}</strong>
              <span class="text-xs text-slate-500 font-mono">(${escapeHtml(addr.recipientPhone)})</span>
              ${addr.isDefaultShipping ? `
                <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-brand-100 text-brand-800">DEFAULT</span>
              ` : ''}
            </div>
            <p class="text-xs text-slate-700">${escapeHtml(addr.areaDetails)}, ${escapeHtml(addr.upazila)}, ${escapeHtml(addr.district)}, ${escapeHtml(addr.division)}</p>
            ${addr.deliveryInstructions ? `
              <p class="text-[11px] text-slate-500 italic">Note: ${escapeHtml(addr.deliveryInstructions)}</p>
            ` : ''}
          </div>

          <div class="flex items-center gap-2 flex-shrink-0">
            ${!addr.isDefaultShipping ? `
              <button type="button" class="set-default-addr-btn text-xs text-brand-600 hover:text-brand-700 font-semibold" data-id="${addr.id}">
                Set Default
              </button>
            ` : ''}
            <button type="button" class="delete-addr-btn text-slate-400 hover:text-rose-600 p-1" data-id="${addr.id}" title="Delete address">
              <i data-lucide="trash-2" class="w-4 h-4"></i>
            </button>
          </div>
        </div>
      `).join('');

      // Setup default & delete buttons
      listEl.querySelectorAll('.set-default-addr-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
          await addressesApi.setDefaultAddress(btn.dataset.id);
          toast.success('Default address updated!');
          loadAddressesList(container);
        });
      });

      listEl.querySelectorAll('.delete-addr-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
          if (confirm('Delete this saved address?')) {
            await addressesApi.deleteAddress(btn.dataset.id);
            toast.info('Address removed');
            loadAddressesList(container);
          }
        });
      });
    }

    renderIcons(listEl);
  } catch (err) {
    listEl.innerHTML = `<p class="text-xs text-rose-600 p-2">Error loading addresses: ${err.message}</p>`;
  }
}

async function loadAddressDropdowns(container) {
  const divSelect = container.querySelector('#addr-division');
  const distSelect = container.querySelector('#addr-district');
  const upzSelect = container.querySelector('#addr-upazila');

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
      const selectedOpt = divSelect.selectedOptions[0];
      const divId = selectedOpt?.dataset.id;
      distSelect.innerHTML = '<option value="">Select District</option>';
      upzSelect.innerHTML = '<option value="">Select Upazila</option>';
      if (divId) {
        const districts = await lookupsApi.getDistricts(divId);
        districts.forEach(dst => {
          const opt = document.createElement('option');
          opt.value = dst.name;
          opt.dataset.id = dst.id;
          opt.textContent = `${dst.name} (${dst.nameBn || ''})`;
          distSelect.appendChild(opt);
        });
      }
    };

    distSelect.onchange = async () => {
      const selectedOpt = distSelect.selectedOptions[0];
      const distId = selectedOpt?.dataset.id;
      upzSelect.innerHTML = '<option value="">Select Upazila</option>';
      if (distId) {
        const upazilas = await lookupsApi.getUpazilas(distId);
        upazilas.forEach(u => {
          const opt = document.createElement('option');
          opt.value = u.name;
          opt.textContent = `${u.name} (${u.nameBn || ''})`;
          upzSelect.appendChild(opt);
        });
      }
    };
  } catch (e) {
    console.warn('Address cascade lookup failed:', e);
  }
}
