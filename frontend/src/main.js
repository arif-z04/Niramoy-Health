import './css/style.css';
import { createHeader } from './components/Header.js';
import { createFooter } from './components/Footer.js';
import { createBottomNav } from './components/BottomNav.js';
import { createCartDrawer } from './components/CartDrawer.js';
import { createToastContainer } from './components/ToastContainer.js';
import { Router } from './router.js';
import { authStore } from './state/authStore.js';
import { cartStore } from './state/cartStore.js';
import { renderIcons } from './utils/icons.js';

async function bootstrap() {
  const root = document.getElementById('app');
  if (!root) return;

  root.className = 'min-h-screen flex flex-col bg-slate-50 text-slate-800 font-sans w-full max-w-full overflow-x-clip';

  // 1. Header
  const header = createHeader();
  root.appendChild(header);

  // 2. Main content container for routes (pb-16 on mobile for bottom navigation)
  const main = document.createElement('main');
  main.id = 'app-main';
  main.className = 'flex-1 pb-16 md:pb-0 w-full max-w-full';
  root.appendChild(main);

  // 3. Footer
  const footer = createFooter();
  root.appendChild(footer);

  // 4. Mobile Bottom Navigation
  const bottomNav = createBottomNav();
  document.body.appendChild(bottomNav);

  // 5. Overlays: Cart Slide-over & Toast Container
  const cartDrawer = createCartDrawer();
  document.body.appendChild(cartDrawer);

  const toastContainer = createToastContainer();
  document.body.appendChild(toastContainer);

  // 6. Initialize Router
  const router = new Router(main);
  await router.handleRoute();

  // 7. Fetch background state: Cart & Auth
  authStore.loadMe().catch(() => {});
  cartStore.fetchCart().catch(() => {});

  renderIcons(document.body);
}

document.addEventListener('DOMContentLoaded', bootstrap);
