import { authStore } from '../state/authStore.js';
import { cartStore } from '../state/cartStore.js';
import { renderIcons } from '../utils/icons.js';

export function createBottomNav() {
  const nav = document.createElement('nav');
  nav.id = 'mobile-bottom-nav';
  nav.className = 'md:hidden fixed bottom-0 left-0 right-0 z-40 bg-white/95 backdrop-blur-md border-t border-slate-200/90 shadow-[0_-4px_16px_rgba(0,0,0,0.06)] px-2 py-1 flex items-center justify-around';
  nav.setAttribute('aria-label', 'Mobile bottom navigation');

  nav.innerHTML = `
    <!-- 1. Home -->
    <a href="#/" class="bottom-nav-link flex flex-col items-center justify-center p-1.5 min-w-[56px] min-h-[44px] text-slate-600 hover:text-brand-600 transition" data-route="home">
      <i data-lucide="home" class="w-5 h-5"></i>
      <span class="text-[10px] font-semibold mt-0.5">Home</span>
    </a>

    <!-- 2. Catalog / Medicines -->
    <a href="#/products" class="bottom-nav-link flex flex-col items-center justify-center p-1.5 min-w-[56px] min-h-[44px] text-slate-600 hover:text-brand-600 transition" data-route="products">
      <i data-lucide="layout-grid" class="w-5 h-5"></i>
      <span class="text-[10px] font-semibold mt-0.5">Medicines</span>
    </a>

    <!-- 3. Upload Prescription (Prominent Center Action) -->
    <a href="#/prescriptions/upload" class="bottom-nav-link flex flex-col items-center justify-center p-1 min-w-[56px] min-h-[44px] text-brand-700 hover:text-brand-800 transition -mt-3 group" data-route="prescriptions">
      <div class="w-11 h-11 rounded-full bg-brand-600 text-white flex items-center justify-center shadow-lg shadow-brand-600/30 group-hover:scale-105 transition">
        <i data-lucide="file-up" class="w-5 h-5"></i>
      </div>
      <span class="text-[10px] font-bold text-brand-700 mt-0.5">Upload Rx</span>
    </a>

    <!-- 4. Cart Drawer Trigger -->
    <button type="button" id="bottom-nav-cart-btn" class="bottom-nav-link relative flex flex-col items-center justify-center p-1.5 min-w-[56px] min-h-[44px] text-slate-600 hover:text-brand-600 transition" data-route="cart">
      <div class="relative">
        <i data-lucide="shopping-bag" class="w-5 h-5"></i>
        <span id="bottom-nav-cart-count" class="hidden absolute -top-1 -right-2 w-4 h-4 rounded-full bg-accent-500 text-slate-950 font-bold text-[9px] flex items-center justify-center">0</span>
      </div>
      <span class="text-[10px] font-semibold mt-0.5">Cart</span>
    </button>

    <!-- 5. Account / Sign In -->
    <a href="#/account" id="bottom-nav-account-link" class="bottom-nav-link flex flex-col items-center justify-center p-1.5 min-w-[56px] min-h-[44px] text-slate-600 hover:text-brand-600 transition" data-route="account">
      <i data-lucide="user" class="w-5 h-5"></i>
      <span class="text-[10px] font-semibold mt-0.5" id="bottom-nav-account-label">Account</span>
    </a>
  `;

  // Attach events
  nav.querySelector('#bottom-nav-cart-btn')?.addEventListener('click', () => {
    cartStore.openDrawer();
  });

  // Highlight active link based on current hash
  const updateActiveState = () => {
    const hash = window.location.hash || '#/';
    nav.querySelectorAll('.bottom-nav-link').forEach(link => {
      const href = link.getAttribute('href');
      if (href && (hash === href || (href !== '#/' && hash.startsWith(href)))) {
        link.classList.add('text-brand-600');
        link.classList.remove('text-slate-600');
      } else if (href) {
        link.classList.remove('text-brand-600');
        link.classList.add('text-slate-600');
      }
    });
  };

  window.addEventListener('hashchange', updateActiveState);
  updateActiveState();

  // Subscribe to Cart updates for bottom nav badge
  cartStore.subscribe(state => {
    const badge = nav.querySelector('#bottom-nav-cart-count');
    if (badge) {
      badge.textContent = state.itemCount;
      if (state.itemCount > 0) {
        badge.classList.remove('hidden');
      } else {
        badge.classList.add('hidden');
      }
    }
  });

  // Subscribe to Auth updates
  authStore.subscribe(state => {
    const accountLink = nav.querySelector('#bottom-nav-account-link');
    const accountLabel = nav.querySelector('#bottom-nav-account-label');
    if (accountLink && accountLabel) {
      if (state.isAuthenticated) {
        accountLink.setAttribute('href', '#/account');
        accountLabel.textContent = 'Account';
      } else {
        accountLink.setAttribute('href', '#/login');
        accountLabel.textContent = 'Sign In';
      }
    }
  });

  renderIcons(nav);
  return nav;
}
