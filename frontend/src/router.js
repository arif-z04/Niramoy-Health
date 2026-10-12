import { renderHomePage } from './pages/HomePage.js';
import { renderCatalogPage } from './pages/CatalogPage.js';
import { renderProductDetailsPage } from './pages/ProductDetailsPage.js';
import { renderPrescriptionUploadPage } from './pages/PrescriptionUploadPage.js';
import { renderPrescriptionsListPage } from './pages/PrescriptionsListPage.js';
import { renderLoginPage } from './pages/LoginPage.js';
import { renderRegisterPage } from './pages/RegisterPage.js';
import { renderAccountPage } from './pages/AccountPage.js';
import { renderOrdersListPage } from './pages/OrdersListPage.js';
import { renderOrderDetailsPage } from './pages/OrderDetailsPage.js';
import { renderCheckoutPage } from './pages/CheckoutPage.js';
import { renderOrderConfirmationPage } from './pages/OrderConfirmationPage.js';
import { renderSupportPage } from './pages/SupportPage.js';
import { renderAdminDashboardPage } from './pages/AdminDashboardPage.js';
import { renderIcons } from './utils/icons.js';
import { cartStore } from './state/cartStore.js';

export class Router {
  constructor(appElement) {
    this.appElement = appElement;
    window.addEventListener('hashchange', () => this.handleRoute());
  }

  async handleRoute() {
    // Ensure slide-over drawers are closed on navigation
    cartStore.closeDrawer();

    const rawHash = window.location.hash.slice(1) || '/';
    const [pathPart, queryPart] = rawHash.split('?');
    const path = pathPart.replace(/\/$/, '') || '/';
    const params = new URLSearchParams(queryPart || '');
    const queryParams = Object.fromEntries(params.entries());

    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'instant' });

    // Clear previous page
    this.appElement.innerHTML = '';

    try {
      let pageElement = null;

      if (path === '/') {
        pageElement = await renderHomePage();
      } else if (path === '/products') {
        pageElement = await renderCatalogPage(queryParams);
      } else if (path.startsWith('/products/')) {
        const slug = path.replace('/products/', '');
        pageElement = await renderProductDetailsPage(slug);
      } else if (path === '/prescriptions/upload') {
        pageElement = renderPrescriptionUploadPage();
      } else if (path === '/prescriptions') {
        pageElement = await renderPrescriptionsListPage();
      } else if (path === '/login') {
        pageElement = renderLoginPage(queryParams.redirect);
      } else if (path === '/register') {
        pageElement = renderRegisterPage();
      } else if (path === '/account') {
        pageElement = await renderAccountPage();
      } else if (path === '/account/orders') {
        pageElement = await renderOrdersListPage();
      } else if (path.startsWith('/account/orders/')) {
        const orderId = path.replace('/account/orders/', '');
        pageElement = await renderOrderDetailsPage(orderId);
      } else if (path === '/checkout') {
        pageElement = await renderCheckoutPage();
      } else if (path.startsWith('/order-confirmed/')) {
        const orderId = path.replace('/order-confirmed/', '');
        pageElement = await renderOrderConfirmationPage(orderId);
      } else if (path === '/support') {
        pageElement = await renderSupportPage();
      } else if (path === '/admin') {
        pageElement = await renderAdminDashboardPage();
      } else if (path === '/terms' || path === '/privacy-policy' || path === '/refund-policy' || path === '/disclaimer') {
        pageElement = this.renderPolicyPage(path);
      } else {
        pageElement = this.render404();
      }

      if (pageElement) {
        this.appElement.appendChild(pageElement);
        renderIcons(this.appElement);
      }
    } catch (err) {
      console.error('Route navigation error:', err);
      this.appElement.innerHTML = `
        <div class="max-w-md mx-auto my-12 p-8 bg-white border border-rose-200 rounded-3xl text-center shadow-card space-y-4">
          <h2 class="text-lg font-bold text-slate-900">Unable to load page</h2>
          <p class="text-xs text-slate-500">${err.message || 'An unexpected navigation error occurred.'}</p>
          <a href="#/" class="btn-primary text-xs py-2 px-4 inline-flex">Return Home</a>
        </div>
      `;
      renderIcons(this.appElement);
    }
  }

  renderPolicyPage(path) {
    const el = document.createElement('div');
    el.className = 'max-w-3xl mx-auto px-4 sm:px-6 py-12 bg-white rounded-3xl border border-slate-200/80 p-8 shadow-card space-y-6 text-xs text-slate-700 leading-relaxed';
    
    let title = 'Privacy Policy';
    if (path === '/terms') title = 'Terms and Conditions';
    else if (path === '/refund-policy') title = 'Refund & Return Policy';
    else if (path === '/disclaimer') title = 'Medical & Clinical Disclaimer';

    el.innerHTML = `
      <h1 class="text-2xl font-black text-slate-900 border-b border-slate-100 pb-4">${title}</h1>
      <p>Niramoy Health (নিরাময় হেলথ) operates in compliance with the Directorate General of Drug Administration (DGDA) and applicable laws of the People's Republic of Bangladesh.</p>
      <h3 class="font-bold text-slate-900 text-sm mt-4">1. Authentic Supply Chain</h3>
      <p>All prescription and over-the-counter pharmaceuticals offered on this portal are procured directly from licensed national manufacturers and kept under strict temperature-controlled storage conditions.</p>
      <h3 class="font-bold text-slate-900 text-sm mt-4">2. Prescription Verification</h3>
      <p>All prescription orders require verification by our BMDC licensed clinical pharmacists prior to packing and courier dispatch.</p>
      <h3 class="font-bold text-slate-900 text-sm mt-4">3. Returns & Refunds</h3>
      <p>Items damaged in transit or delivered in error can be returned within 48 hours of delivery. Refrigerated cold-chain products (e.g. insulin) cannot be returned once delivered for patient safety.</p>
      <div class="pt-4 border-t border-slate-100">
        <a href="#/" class="btn-secondary text-xs py-2 px-4 inline-flex">&larr; Return to Store</a>
      </div>
    `;
    return el;
  }

  render404() {
    const el = document.createElement('div');
    el.className = 'max-w-md mx-auto my-16 p-8 bg-white border border-slate-200/80 rounded-3xl text-center shadow-card space-y-4';
    el.innerHTML = `
      <div class="w-16 h-16 rounded-full bg-slate-100 text-slate-400 flex items-center justify-center mx-auto">
        <i data-lucide="compass" class="w-8 h-8"></i>
      </div>
      <h2 class="text-xl font-bold text-slate-900">Page Not Found</h2>
      <p class="text-xs text-slate-500">The page you requested could not be found.</p>
      <a href="#/" class="btn-primary text-xs py-2 px-5 inline-flex font-bold">Return to Homepage</a>
    `;
    return el;
  }
}
