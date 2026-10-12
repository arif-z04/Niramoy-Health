import { authStore } from '../state/authStore.js';
import { cartStore } from '../state/cartStore.js';
import { productsApi } from '../api/products.js';
import { debounce, escapeHtml, formatCurrency } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export function createHeader() {
  const header = document.createElement('header');
  header.className = 'sticky top-0 z-40 bg-white border-b border-slate-200/80 shadow-sm w-full max-w-full';

  header.innerHTML = `
    <!-- Top Micro Bar: Express delivery banner & emergency hotline -->
    <div class="bg-brand-900 text-brand-50 text-xs py-1.5 px-4 hidden md:block">
      <div class="max-w-7xl mx-auto flex justify-between items-center">
        <div class="flex items-center gap-4">
          <span class="flex items-center gap-1 font-medium">
            <i data-lucide="shield-check" class="w-3.5 h-3.5 text-brand-300"></i>
            100% Genuine Medicine & BMDC Verified Pharmacists
          </span>
          <span class="text-brand-300">|</span>
          <span class="flex items-center gap-1 text-brand-200">
            <i data-lucide="truck" class="w-3.5 h-3.5"></i>
            Fast Delivery across 64 Districts in Bangladesh
          </span>
        </div>
        <div class="flex items-center gap-4">
          <a href="#/support" class="hover:text-white transition flex items-center gap-1">
            <i data-lucide="help-circle" class="w-3.5 h-3.5"></i> Support & Hotline: 16247
          </a>
          <span id="top-admin-badge" class="hidden px-2 py-0.5 rounded bg-amber-500 text-slate-950 font-bold text-[10px]">ADMIN</span>
        </div>
      </div>
    </div>

    <!-- Main Header -->
    <div class="max-w-7xl mx-auto px-3 sm:px-6 py-2.5 sm:py-3">
      <div class="flex items-center justify-between gap-2 sm:gap-4 md:gap-8">
        
        <!-- Logo -->
        <a href="#/" id="header-brand-logo" class="flex items-center gap-2 flex-shrink-0 group">
          <div class="w-9 h-9 sm:w-10 sm:h-10 rounded-xl bg-gradient-to-tr from-brand-700 to-brand-500 flex items-center justify-center text-white shadow-md shadow-brand-500/20 group-hover:scale-105 transition">
            <i data-lucide="cross" class="w-5 h-5 sm:w-6 sm:h-6 stroke-[2.5]"></i>
          </div>
          <div>
            <div class="flex items-baseline gap-1">
              <span class="text-lg sm:text-xl font-extrabold tracking-tight text-slate-900">Niramoy</span>
              <span class="text-lg sm:text-xl font-bold tracking-tight text-brand-600">Health</span>
            </div>
            <p class="hidden xs:block text-[9px] sm:text-[10px] font-medium text-slate-500 -mt-1 tracking-wide font-sans">নিরাময় হেলথ • ডিজিটাল ফার্মেসি</p>
          </div>
        </a>

        <!-- Location Selector (Bangladeshi Hubs - Desktop only) -->
        <div class="hidden lg:flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-slate-50 border border-slate-200/80 text-xs text-slate-700 cursor-pointer hover:border-brand-500 transition" id="location-selector-btn">
          <i data-lucide="map-pin" class="w-4 h-4 text-brand-600"></i>
          <div>
            <div class="text-[10px] text-slate-400 leading-none">Deliver to</div>
            <div class="font-semibold text-slate-800 leading-tight">Dhaka City</div>
          </div>
          <i data-lucide="chevron-down" class="w-3.5 h-3.5 text-slate-400 ml-1"></i>
        </div>

        <!-- Desktop Search Bar with Live Suggestions Dropdown -->
        <div class="hidden md:block flex-1 max-w-2xl relative">
          <form id="header-search-form" class="relative">
            <input 
              type="text" 
              id="header-search-input"
              autocomplete="off"
              placeholder="Search medicine, generic, health products (e.g. Napa, Seclo, Dextrose)..." 
              class="w-full pl-10 pr-10 py-2.5 text-base sm:text-sm rounded-xl border border-slate-200 bg-slate-50/70 focus:bg-white focus:border-brand-500 focus:ring-4 focus:ring-brand-500/10 focus:outline-none transition shadow-inner"
            />
            <i data-lucide="search" class="w-4 h-4 text-slate-400 absolute left-3.5 top-3.5 pointer-events-none"></i>
            <button type="button" id="header-search-clear" class="hidden absolute right-3.5 top-3.5 text-slate-400 hover:text-slate-600">
              <i data-lucide="x" class="w-4 h-4"></i>
            </button>
          </form>

          <!-- Autocomplete Dropdown (Desktop) -->
          <div id="search-suggestions-dropdown" class="hidden absolute left-0 right-0 top-full mt-1.5 bg-white rounded-xl border border-slate-200 shadow-xl overflow-hidden z-50 animate-slide-down max-h-[420px] overflow-y-auto">
            <div id="search-suggestions-list" class="divide-y divide-slate-100"></div>
          </div>
        </div>

        <!-- Actions: Prescription, Account, Cart, Mobile Menu -->
        <div class="flex items-center gap-1.5 sm:gap-2.5 flex-shrink-0">
          
          <!-- Upload Prescription CTA (Desktop/Tablet) -->
          <a href="#/prescriptions/upload" class="hidden sm:inline-flex items-center gap-1.5 px-3 py-2 rounded-xl bg-brand-50 hover:bg-brand-100 text-brand-700 font-semibold text-xs border border-brand-200 transition shadow-sm">
            <i data-lucide="file-up" class="w-4 h-4 text-brand-600"></i>
            <span class="hidden md:inline">Upload Prescription</span>
            <span class="md:hidden">Upload Rx</span>
          </a>

          <!-- User Account / Login Button -->
          <div class="relative" id="user-menu-container">
            <button id="user-menu-btn" class="flex items-center gap-1.5 p-1.5 sm:p-2 rounded-xl text-slate-700 hover:bg-slate-100 transition focus:outline-none min-h-[40px] min-w-[40px] justify-center" aria-label="User account">
              <div class="w-7 h-7 sm:w-8 sm:h-8 rounded-full bg-slate-100 border border-slate-200 flex items-center justify-center text-slate-600 font-bold text-xs" id="user-avatar-text">
                <i data-lucide="user" class="w-4 h-4"></i>
              </div>
              <span class="hidden md:inline text-xs font-semibold max-w-[100px] truncate" id="user-display-name">Sign In</span>
              <i data-lucide="chevron-down" class="w-3.5 h-3.5 text-slate-400 hidden md:block"></i>
            </button>

            <!-- Dropdown Menu -->
            <div id="user-dropdown" class="hidden absolute right-0 top-full mt-1.5 w-56 bg-white rounded-xl border border-slate-200 shadow-xl py-1.5 z-50 animate-slide-down">
              <div id="user-dropdown-auth-items"></div>
            </div>
          </div>

          <!-- Shopping Cart Trigger Button -->
          <button id="header-cart-btn" class="relative flex items-center gap-1.5 px-2.5 sm:px-3 py-2 rounded-xl bg-brand-600 hover:bg-brand-700 active:bg-brand-800 text-white font-medium text-xs shadow-md shadow-brand-600/20 transition min-h-[40px]" aria-label="View shopping cart">
            <i data-lucide="shopping-bag" class="w-4 h-4"></i>
            <span class="hidden md:inline font-semibold">Cart</span>
            <span id="header-cart-count" class="w-5 h-5 rounded-full bg-accent-500 text-slate-950 font-bold text-[11px] flex items-center justify-center">0</span>
          </button>

          <!-- Mobile Menu Hamburger Button -->
          <button id="mobile-menu-btn" class="md:hidden p-2 rounded-xl text-slate-700 hover:bg-slate-100 min-h-[40px] min-w-[40px] flex items-center justify-center" aria-label="Open mobile navigation menu">
            <i data-lucide="menu" class="w-5 h-5"></i>
          </button>
        </div>
      </div>

      <!-- Mobile Search Bar (Full Width on Phone < md) -->
      <div class="md:hidden mt-2 pb-0.5 relative">
        <form id="mobile-search-form" class="relative">
          <input 
            type="text" 
            id="mobile-search-input"
            autocomplete="off"
            placeholder="Search medicine or healthcare essentials..." 
            class="w-full pl-9 pr-9 py-2 text-base rounded-xl border border-slate-200 bg-slate-50 focus:bg-white focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none transition shadow-sm"
          />
          <i data-lucide="search" class="w-4 h-4 text-slate-400 absolute left-3 top-2.5 pointer-events-none"></i>
          <button type="button" id="mobile-search-clear" class="hidden absolute right-3 top-2.5 text-slate-400 hover:text-slate-600">
            <i data-lucide="x" class="w-4 h-4"></i>
          </button>
        </form>

        <!-- Autocomplete Dropdown (Mobile) -->
        <div id="mobile-search-suggestions-dropdown" class="hidden absolute left-0 right-0 top-full mt-1.5 bg-white rounded-xl border border-slate-200 shadow-xl overflow-hidden z-50 animate-slide-down max-h-[360px] overflow-y-auto">
          <div id="mobile-search-suggestions-list" class="divide-y divide-slate-100"></div>
        </div>
      </div>
    </div>

    <!-- Category Navigation Ribbon (Desktop & Tablet) -->
    <nav class="border-t border-slate-100 bg-slate-50/50 hidden md:block w-full max-w-full overflow-hidden">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 w-full min-w-0">
        <ul class="flex items-center gap-6 py-2 text-xs font-medium text-slate-700 overflow-x-auto no-scrollbar whitespace-nowrap w-full">
          <li>
            <a href="#/products" class="flex items-center gap-1.5 hover:text-brand-600 transition font-semibold text-brand-700">
              <i data-lucide="layout-grid" class="w-3.5 h-3.5"></i> All Products
            </a>
          </li>
          <li>
            <a href="#/products?category=prescription-medicine" class="hover:text-brand-600 transition flex items-center gap-1">
              <i data-lucide="pill" class="w-3.5 h-3.5 text-rose-500"></i> Prescription Medicine
            </a>
          </li>
          <li>
            <a href="#/products?category=otc-medicine" class="hover:text-brand-600 transition flex items-center gap-1">
              <i data-lucide="tablets" class="w-3.5 h-3.5 text-emerald-500"></i> OTC Medicine
            </a>
          </li>
          <li>
            <a href="#/products?category=vitamins-supplements" class="hover:text-brand-600 transition">Vitamins & Supplements</a>
          </li>
          <li>
            <a href="#/products?category=healthcare-devices" class="hover:text-brand-600 transition">Healthcare Devices</a>
          </li>
          <li>
            <a href="#/products?category=baby-mom-care" class="hover:text-brand-600 transition">Baby & Mom Care</a>
          </li>
          <li>
            <a href="#/products?category=beauty-skincare" class="hover:text-brand-600 transition">Beauty & Skincare</a>
          </li>
          <li>
            <a href="#/products?category=personal-care" class="hover:text-brand-600 transition">Personal Care</a>
          </li>
          <li class="ml-auto">
            <a href="#/products?sort=discount" class="flex items-center gap-1 text-accent-700 hover:text-accent-800 font-bold">
              <i data-lucide="flame" class="w-3.5 h-3.5 text-accent-600"></i> Special Offers
            </a>
          </li>
        </ul>
      </div>
    </nav>

    <!-- Off-Canvas Mobile Drawer & Backdrop -->
    <div id="mobile-drawer-backdrop" class="fixed inset-0 bg-slate-950/50 backdrop-blur-sm z-50 opacity-0 pointer-events-none transition-opacity duration-300 md:hidden"></div>
    
    <div id="mobile-drawer-panel" class="fixed inset-y-0 left-0 w-80 max-w-[85vw] bg-white z-50 shadow-2xl flex flex-col transform -translate-x-full transition-transform duration-300 ease-in-out md:hidden overflow-hidden">
      <!-- Drawer Header -->
      <div class="px-5 py-4 border-b border-slate-200/80 flex items-center justify-between bg-slate-50/70">
        <a href="#/" class="flex items-center gap-2 drawer-close-trigger">
          <div class="w-8 h-8 rounded-xl bg-gradient-to-tr from-brand-700 to-brand-500 flex items-center justify-center text-white shadow-sm">
            <i data-lucide="cross" class="w-4 h-4 stroke-[2.5]"></i>
          </div>
          <div>
            <span class="text-base font-extrabold text-slate-900">Niramoy</span>
            <span class="text-base font-bold text-brand-600">Health</span>
          </div>
        </a>
        <button type="button" id="mobile-drawer-close-btn" class="p-2 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition tap-target-44 flex items-center justify-center" aria-label="Close menu">
          <i data-lucide="x" class="w-5 h-5"></i>
        </button>
      </div>

      <!-- Drawer Content (Scrollable) -->
      <div class="flex-1 overflow-y-auto p-4 space-y-5">
        
        <!-- User Status Card in Drawer -->
        <div id="drawer-user-section" class="p-3.5 rounded-2xl bg-brand-50/50 border border-brand-100 text-xs">
          <!-- Populated dynamically by updateHeaderAuthUI -->
        </div>

        <!-- Primary Quick Links -->
        <div class="space-y-1">
          <div class="text-[10px] font-bold uppercase tracking-wider text-slate-400 px-3 mb-1">Quick Access</div>
          <a href="#/prescriptions/upload" class="drawer-close-trigger flex items-center gap-3 px-3 py-2.5 rounded-xl bg-brand-50 text-brand-700 font-bold text-xs hover:bg-brand-100 transition">
            <i data-lucide="file-up" class="w-4 h-4 text-brand-600"></i>
            <span>Upload Prescription</span>
          </a>
          <a href="#/products" class="drawer-close-trigger flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-slate-50 text-slate-700 font-semibold text-xs transition">
            <i data-lucide="layout-grid" class="w-4 h-4 text-slate-500"></i>
            <span>Browse All Medicines</span>
          </a>
          <a href="#/account/orders" class="drawer-close-trigger flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-slate-50 text-slate-700 font-semibold text-xs transition">
            <i data-lucide="package" class="w-4 h-4 text-slate-500"></i>
            <span>My Orders &amp; Receipts</span>
          </a>
          <a href="#/prescriptions" class="drawer-close-trigger flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-slate-50 text-slate-700 font-semibold text-xs transition">
            <i data-lucide="clipboard-list" class="w-4 h-4 text-slate-500"></i>
            <span>My Prescriptions History</span>
          </a>
          <a href="#/products?sort=discount" class="drawer-close-trigger flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-amber-50 text-amber-700 font-semibold text-xs transition">
            <i data-lucide="flame" class="w-4 h-4 text-amber-600"></i>
            <span>Special Health Offers</span>
          </a>
        </div>

        <!-- Healthcare Categories List -->
        <div class="space-y-1 pt-2 border-t border-slate-100">
          <div class="text-[10px] font-bold uppercase tracking-wider text-slate-400 px-3 mb-1">Medicine Categories</div>
          <a href="#/products?category=prescription-medicine" class="drawer-close-trigger flex items-center justify-between px-3 py-2 rounded-lg text-xs text-slate-600 hover:text-brand-600 hover:bg-slate-50 transition">
            <span class="flex items-center gap-2">
              <i data-lucide="pill" class="w-3.5 h-3.5 text-rose-500"></i> Prescription Medicine
            </span>
            <i data-lucide="chevron-right" class="w-3 h-3 text-slate-300"></i>
          </a>
          <a href="#/products?category=otc-medicine" class="drawer-close-trigger flex items-center justify-between px-3 py-2 rounded-lg text-xs text-slate-600 hover:text-brand-600 hover:bg-slate-50 transition">
            <span class="flex items-center gap-2">
              <i data-lucide="tablets" class="w-3.5 h-3.5 text-emerald-500"></i> OTC Medicine
            </span>
            <i data-lucide="chevron-right" class="w-3 h-3 text-slate-300"></i>
          </a>
          <a href="#/products?category=vitamins-supplements" class="drawer-close-trigger flex items-center justify-between px-3 py-2 rounded-lg text-xs text-slate-600 hover:text-brand-600 hover:bg-slate-50 transition">
            <span class="flex items-center gap-2">
              <i data-lucide="shield-plus" class="w-3.5 h-3.5 text-blue-500"></i> Vitamins &amp; Supplements
            </span>
            <i data-lucide="chevron-right" class="w-3 h-3 text-slate-300"></i>
          </a>
          <a href="#/products?category=healthcare-devices" class="drawer-close-trigger flex items-center justify-between px-3 py-2 rounded-lg text-xs text-slate-600 hover:text-brand-600 hover:bg-slate-50 transition">
            <span class="flex items-center gap-2">
              <i data-lucide="activity" class="w-3.5 h-3.5 text-teal-500"></i> Health Devices
            </span>
            <i data-lucide="chevron-right" class="w-3 h-3 text-slate-300"></i>
          </a>
          <a href="#/products?category=baby-mom-care" class="drawer-close-trigger flex items-center justify-between px-3 py-2 rounded-lg text-xs text-slate-600 hover:text-brand-600 hover:bg-slate-50 transition">
            <span class="flex items-center gap-2">
              <i data-lucide="baby" class="w-3.5 h-3.5 text-pink-500"></i> Baby &amp; Mom Care
            </span>
            <i data-lucide="chevron-right" class="w-3 h-3 text-slate-300"></i>
          </a>
        </div>

        <!-- Staff Portal shortcut (if staff) -->
        <div id="drawer-admin-section" class="hidden pt-2 border-t border-slate-100">
          <a href="#/admin" class="drawer-close-trigger flex items-center gap-3 px-3 py-2.5 rounded-xl bg-amber-50 text-amber-900 font-bold text-xs hover:bg-amber-100 transition">
            <i data-lucide="layout-dashboard" class="w-4 h-4 text-amber-600"></i>
            <span>Admin Staff Dashboard</span>
          </a>
        </div>

      </div>

      <!-- Drawer Footer: Hotline & DGDA info -->
      <div class="p-4 border-t border-slate-100 bg-slate-50 text-xs text-slate-500 space-y-2">
        <a href="#/support" class="drawer-close-trigger flex items-center gap-2 text-slate-700 font-semibold hover:text-brand-600">
          <i data-lucide="phone-call" class="w-4 h-4 text-brand-600"></i>
          <span>Hotline: 16247 (9 AM – 11 PM)</span>
        </a>
        <div class="text-[11px] text-slate-400">
          DGDA Licensed • BMDC Registered Pharmacists
        </div>
      </div>
    </div>
  `;

  // Attach interactive events
  setupHeaderEvents(header);

  // Subscribe to auth state updates
  authStore.subscribe(state => updateHeaderAuthUI(header, state));

  // Subscribe to cart state updates
  cartStore.subscribe(state => updateHeaderCartUI(header, state));

  return header;
}

function updateHeaderAuthUI(header, authState) {
  const nameEl = header.querySelector('#user-display-name');
  const avatarEl = header.querySelector('#user-avatar-text');
  const dropdownItemsEl = header.querySelector('#user-dropdown-auth-items');
  const topAdminBadge = header.querySelector('#top-admin-badge');
  const drawerUserSection = header.querySelector('#drawer-user-section');
  const drawerAdminSection = header.querySelector('#drawer-admin-section');

  if (authState.isAdmin || authState.isPharmacist) {
    topAdminBadge?.classList.remove('hidden');
    drawerAdminSection?.classList.remove('hidden');
  } else {
    topAdminBadge?.classList.add('hidden');
    drawerAdminSection?.classList.add('hidden');
  }

  if (authState.isAuthenticated && authState.user) {
    const firstName = authState.user.fullName?.split(' ')[0] || 'Account';
    if (nameEl) nameEl.textContent = firstName;
    if (avatarEl) avatarEl.textContent = firstName.charAt(0).toUpperCase();

    let itemsHtml = `
      <div class="px-4 py-2 border-b border-slate-100">
        <div class="font-semibold text-xs text-slate-900 truncate">${escapeHtml(authState.user.fullName)}</div>
        <div class="text-[11px] text-slate-500 truncate">${escapeHtml(authState.user.phoneNumber || authState.user.email)}</div>
      </div>
      <a href="#/account" class="flex items-center gap-2 px-4 py-2 text-xs text-slate-700 hover:bg-slate-50 transition">
        <i data-lucide="user" class="w-4 h-4 text-slate-400"></i> My Profile & Addresses
      </a>
      <a href="#/account/orders" class="flex items-center gap-2 px-4 py-2 text-xs text-slate-700 hover:bg-slate-50 transition">
        <i data-lucide="package" class="w-4 h-4 text-slate-400"></i> My Orders
      </a>
      <a href="#/prescriptions" class="flex items-center gap-2 px-4 py-2 text-xs text-slate-700 hover:bg-slate-50 transition">
        <i data-lucide="file-text" class="w-4 h-4 text-slate-400"></i> My Prescriptions
      </a>
      <a href="#/support" class="flex items-center gap-2 px-4 py-2 text-xs text-slate-700 hover:bg-slate-50 transition">
        <i data-lucide="life-buoy" class="w-4 h-4 text-slate-400"></i> Support Tickets
      </a>
    `;

    if (authState.isAdmin || authState.isPharmacist) {
      itemsHtml += `
        <div class="border-t border-slate-100 my-1"></div>
        <a href="#/admin" class="flex items-center gap-2 px-4 py-2 text-xs font-semibold text-brand-700 bg-brand-50/50 hover:bg-brand-50 transition">
          <i data-lucide="layout-dashboard" class="w-4 h-4 text-brand-600"></i> Admin Dashboard
        </a>
      `;
    }

    itemsHtml += `
      <div class="border-t border-slate-100 my-1"></div>
      <button id="header-logout-btn" class="w-full flex items-center gap-2 px-4 py-2 text-xs text-rose-600 hover:bg-rose-50 transition text-left">
        <i data-lucide="log-out" class="w-4 h-4 text-rose-500"></i> Sign Out
      </button>
    `;

    dropdownItemsEl.innerHTML = itemsHtml;

    dropdownItemsEl.querySelector('#header-logout-btn')?.addEventListener('click', async () => {
      await authStore.logout();
      window.location.hash = '#/';
    });

    if (drawerUserSection) {
      drawerUserSection.innerHTML = `
        <div class="flex items-center justify-between">
          <div class="min-w-0">
            <div class="font-bold text-slate-900 truncate">${escapeHtml(authState.user.fullName)}</div>
            <div class="text-[11px] text-slate-500 truncate">${escapeHtml(authState.user.phoneNumber || authState.user.email)}</div>
          </div>
          <a href="#/account" class="drawer-close-trigger px-2.5 py-1 rounded-lg bg-white border border-brand-200 text-brand-700 font-bold text-[11px] flex-shrink-0">
            Profile
          </a>
        </div>
      `;
    }
  } else {
    if (nameEl) nameEl.textContent = 'Sign In';
    if (avatarEl) avatarEl.innerHTML = '<i data-lucide="user" class="w-4 h-4"></i>';

    dropdownItemsEl.innerHTML = `
      <div class="p-3 text-center">
        <p class="text-xs text-slate-600 mb-2">Access orders, prescriptions & saved addresses</p>
        <a href="#/login" class="btn-primary w-full text-xs py-2 mb-1.5 block">Sign In</a>
        <a href="#/register" class="btn-secondary w-full text-xs py-2 block">Create Account</a>
      </div>
    `;

    if (drawerUserSection) {
      drawerUserSection.innerHTML = `
        <div class="text-center space-y-2">
          <p class="text-slate-600">Welcome to Niramoy Health</p>
          <div class="flex gap-2">
            <a href="#/login" class="drawer-close-trigger btn-primary flex-1 py-1.5 text-xs text-center">Sign In</a>
            <a href="#/register" class="drawer-close-trigger btn-secondary flex-1 py-1.5 text-xs text-center">Register</a>
          </div>
        </div>
      `;
    }
  }

  renderIcons(header);
}

function updateHeaderCartUI(header, cartState) {
  const countEl = header.querySelector('#header-cart-count');
  if (countEl) {
    countEl.textContent = cartState.itemCount;
    if (cartState.itemCount > 0) {
      countEl.classList.remove('hidden');
    }
  }
}

function setupHeaderEvents(header) {
  // Toggle Cart Drawer
  header.querySelector('#header-cart-btn')?.addEventListener('click', () => {
    cartStore.openDrawer();
  });

  // Toggle User Menu Dropdown
  const userMenuBtn = header.querySelector('#user-menu-btn');
  const userDropdown = header.querySelector('#user-dropdown');
  userMenuBtn?.addEventListener('click', (e) => {
    e.stopPropagation();
    userDropdown?.classList.toggle('hidden');
  });

  document.addEventListener('click', (e) => {
    if (!header.querySelector('#user-menu-container')?.contains(e.target)) {
      userDropdown?.classList.add('hidden');
    }
    if (!header.querySelector('#header-search-form')?.contains(e.target)) {
      header.querySelector('#search-suggestions-dropdown')?.classList.add('hidden');
    }
    if (!header.querySelector('#mobile-search-form')?.contains(e.target)) {
      header.querySelector('#mobile-search-suggestions-dropdown')?.classList.add('hidden');
    }
  });

  // Off-Canvas Mobile Drawer Controls
  const drawerBackdrop = header.querySelector('#mobile-drawer-backdrop');
  const drawerPanel = header.querySelector('#mobile-drawer-panel');
  const mobileMenuBtn = header.querySelector('#mobile-menu-btn');
  const mobileDrawerCloseBtn = header.querySelector('#mobile-drawer-close-btn');

  const openMobileDrawer = () => {
    drawerBackdrop?.classList.remove('opacity-0', 'pointer-events-none');
    drawerBackdrop?.classList.add('opacity-100');
    drawerPanel?.classList.remove('-translate-x-full');
    document.body.classList.add('overflow-hidden');
  };

  const closeMobileDrawer = () => {
    drawerBackdrop?.classList.add('opacity-0', 'pointer-events-none');
    drawerBackdrop?.classList.remove('opacity-100');
    drawerPanel?.classList.add('-translate-x-full');
    document.body.classList.remove('overflow-hidden');
  };

  mobileMenuBtn?.addEventListener('click', openMobileDrawer);
  drawerBackdrop?.addEventListener('click', closeMobileDrawer);
  mobileDrawerCloseBtn?.addEventListener('click', closeMobileDrawer);

  // Close drawer when any internal navigation link is clicked
  header.querySelectorAll('.drawer-close-trigger').forEach(el => {
    el.addEventListener('click', closeMobileDrawer);
  });

  // Close drawer on Escape key
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      closeMobileDrawer();
    }
  });

  // Wire Search Handlers (both Desktop and Mobile search inputs)
  setupSearchInputPair(
    header.querySelector('#header-search-input'),
    header.querySelector('#header-search-clear'),
    header.querySelector('#search-suggestions-dropdown'),
    header.querySelector('#search-suggestions-list'),
    header.querySelector('#header-search-form')
  );

  setupSearchInputPair(
    header.querySelector('#mobile-search-input'),
    header.querySelector('#mobile-search-clear'),
    header.querySelector('#mobile-search-suggestions-dropdown'),
    header.querySelector('#mobile-search-suggestions-list'),
    header.querySelector('#mobile-search-form')
  );
}

function setupSearchInputPair(searchInput, searchClear, searchDropdown, suggestionsList, searchForm) {
  if (!searchInput || !searchForm) return;

  searchForm.addEventListener('submit', (e) => {
    e.preventDefault();
    const query = searchInput.value.trim();
    if (query) {
      searchDropdown?.classList.add('hidden');
      window.location.hash = `#/products?query=${encodeURIComponent(query)}`;
    }
  });

  searchClear?.addEventListener('click', () => {
    searchInput.value = '';
    searchClear.classList.add('hidden');
    searchDropdown?.classList.add('hidden');
    searchInput.focus();
  });

  const performSearch = debounce(async (query) => {
    if (!query || query.length < 2) {
      searchDropdown?.classList.add('hidden');
      return;
    }

    try {
      const result = await productsApi.getProducts({ query, pageSize: 6 });
      const products = result.items || [];

      if (products.length === 0) {
        suggestionsList.innerHTML = `
          <div class="p-4 text-center text-xs text-slate-500">
            No products found matching "${escapeHtml(query)}"
          </div>
        `;
      } else {
        suggestionsList.innerHTML = products.map(p => `
          <a href="#/products/${escapeHtml(p.slug)}" class="flex items-center gap-3 p-3 hover:bg-slate-50 transition group">
            <div class="w-10 h-10 rounded-lg bg-slate-100 flex items-center justify-center p-1 border border-slate-200/60 overflow-hidden flex-shrink-0">
              <img src="${escapeHtml(p.imageUrl || '/assets/medicine-placeholder.png')}" alt="${escapeHtml(p.name)}" class="w-full h-full object-contain" onerror="this.src='data:image/svg+xml;utf8,<svg xmlns=\\'http://www.w3.org/2000/svg\\' width=\\'32\\' height=\\'32\\' viewBox=\\'0 0 24 24\\' fill=\\'none\\' stroke=\\'%2394a3b8\\' stroke-width=\\'2\\'><path d=\\'m10.5 20.5 10-10a4.95 4.95 0 1 0-7-7l-10 10a4.95 4.95 0 1 0 7 7Z\\'/></svg>'"/>
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-xs font-semibold text-slate-800 group-hover:text-brand-600 transition truncate">${escapeHtml(p.name)}</div>
              <div class="text-[11px] text-slate-500 truncate">${escapeHtml(p.strength || '')} • ${escapeHtml(p.dosageForm || 'Medicine')}</div>
            </div>
            <div class="text-right flex-shrink-0">
              <div class="text-xs font-bold text-slate-900">${formatCurrency(p.discountPrice || p.basePrice)}</div>
              ${p.discountPrice ? `<div class="text-[10px] text-slate-400 line-through">${formatCurrency(p.basePrice)}</div>` : ''}
            </div>
          </a>
        `).join('') + `
          <div class="p-2.5 bg-slate-50 text-center">
            <a href="#/products?query=${encodeURIComponent(query)}" class="text-xs font-semibold text-brand-600 hover:text-brand-700">
              View all results for "${escapeHtml(query)}" &rarr;
            </a>
          </div>
        `;
      }

      searchDropdown?.classList.remove('hidden');
    } catch (err) {
      console.warn('Autocomplete fetch failed:', err);
    }
  }, 250);

  searchInput.addEventListener('input', (e) => {
    const val = e.target.value.trim();
    if (val) {
      searchClear?.classList.remove('hidden');
      performSearch(val);
    } else {
      searchClear?.classList.add('hidden');
      searchDropdown?.classList.add('hidden');
    }
  });
}
