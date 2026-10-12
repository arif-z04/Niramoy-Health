import { productsApi } from '../api/products.js';
import { categoriesApi } from '../api/categories.js';
import { renderProductCard } from '../components/ProductCard.js';
import { escapeHtml } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export async function renderHomePage() {
  const container = document.createElement('div');
  container.className = 'space-y-12 pb-16';

  container.innerHTML = `
    <!-- Hero Section -->
    <section class="bg-gradient-to-b from-brand-50/80 via-white to-slate-50 border-b border-slate-200/60 pt-8 pb-12 px-4 sm:px-6">
      <div class="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-12 gap-8 items-center">
        
        <!-- Left: Headline & Actions -->
        <div class="lg:col-span-7 space-y-6">
          <div class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-brand-100 text-brand-800 text-xs font-semibold">
            <span class="w-2 h-2 rounded-full bg-brand-600 animate-pulse"></span>
            Bangladesh's Trusted Digital Healthcare Platform
          </div>

          <h1 class="text-2xl sm:text-4xl lg:text-5xl font-extrabold text-slate-900 tracking-tight leading-tight">
            Authentic Medicines &amp; Healthcare, <br class="hidden sm:inline"/>
            <span class="text-brand-600">Delivered to Your Doorstep.</span>
          </h1>

          <p class="text-xs sm:text-base text-slate-600 max-w-xl leading-relaxed">
            Order genuine prescription medicines, OTC drugs, healthcare devices, and baby care essentials. Verified by registered pharmacists with express delivery in Dhaka and nationwide shipping.
          </p>

          <!-- Quick CTAs -->
          <div class="flex flex-col sm:flex-row items-stretch sm:items-center gap-2.5 sm:gap-3 pt-2">
            <a href="#/prescriptions/upload" class="btn-primary py-3 px-5 text-xs sm:text-sm font-bold shadow-lg shadow-brand-600/25 text-center">
              <i data-lucide="file-up" class="w-4 h-4"></i>
              <span>Upload Prescription</span>
            </a>
            <a href="#/products" class="btn-secondary py-3 px-5 text-xs sm:text-sm font-semibold text-center">
              <i data-lucide="layout-grid" class="w-4 h-4 text-brand-600"></i>
              <span>Browse Catalog</span>
            </a>
          </div>

          <!-- Feature badges -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-2 sm:gap-3 pt-4 border-t border-slate-200/60 text-slate-700">
            <div class="flex items-center gap-2 text-xs font-medium">
              <i data-lucide="check-circle-2" class="w-4 h-4 text-brand-600 flex-shrink-0"></i>
              <span>100% Genuine</span>
            </div>
            <div class="flex items-center gap-2 text-xs font-medium">
              <i data-lucide="check-circle-2" class="w-4 h-4 text-brand-600 flex-shrink-0"></i>
              <span>Pharmacist Verified</span>
            </div>
            <div class="flex items-center gap-2 text-xs font-medium">
              <i data-lucide="check-circle-2" class="w-4 h-4 text-brand-600 flex-shrink-0"></i>
              <span>Cold-Chain Storage</span>
            </div>
          </div>
        </div>

        <!-- Right: Promo Visual Card -->
        <div class="lg:col-span-5">
          <div class="bg-white rounded-3xl border border-slate-200/80 shadow-xl p-6 relative overflow-hidden">
            
            <div class="flex items-center justify-between mb-6">
              <div class="flex items-center gap-2">
                <span class="w-3 h-3 rounded-full bg-emerald-500"></span>
                <span class="text-xs font-bold uppercase tracking-wider text-slate-500">Fast Healthcare Service</span>
              </div>
              <span class="px-2.5 py-1 rounded-full bg-accent-100 text-accent-700 text-xs font-bold">Hotline 16247</span>
            </div>

            <!-- Interactive Prescription Upload Shortcut Box -->
            <div class="border-2 border-dashed border-brand-300 hover:border-brand-500 rounded-2xl p-6 bg-brand-50/40 hover:bg-brand-50 transition cursor-pointer text-center group" onclick="window.location.hash='#/prescriptions/upload'">
              <div class="w-14 h-14 mx-auto rounded-2xl bg-white border border-brand-200 flex items-center justify-center text-brand-600 shadow-sm group-hover:scale-110 transition">
                <i data-lucide="upload-cloud" class="w-7 h-7"></i>
              </div>
              <h3 class="text-sm font-bold text-slate-900 mt-3">Got a Doctor's Prescription?</h3>
              <p class="text-xs text-slate-500 mt-1 max-w-xs mx-auto">Snap a picture or upload PDF. Our pharmacists will review your medicines and prepare your order.</p>
              <button class="mt-4 btn-primary text-xs py-2 px-4 shadow-sm">
                Upload &amp; Order &rarr;
              </button>
            </div>

            <div class="mt-5 pt-4 border-t border-slate-100 flex items-center justify-between text-xs text-slate-600">
              <span class="flex items-center gap-1.5">
                <i data-lucide="shield-alert" class="w-4 h-4 text-brand-600"></i> No self-medication
              </span>
              <span class="font-semibold text-brand-700">Free Pharmacist Review</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Quick Action Cards Section -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6">
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        
        <a href="#/prescriptions/upload" class="p-4 rounded-2xl bg-white border border-slate-200/80 shadow-sm hover:shadow-card-hover transition group flex flex-col justify-between">
          <div class="w-10 h-10 rounded-xl bg-rose-50 text-rose-600 flex items-center justify-center mb-3 group-hover:scale-105 transition">
            <i data-lucide="file-plus" class="w-5 h-5"></i>
          </div>
          <div>
            <h4 class="text-xs font-bold text-slate-900 group-hover:text-brand-600 transition">Upload Prescription</h4>
            <p class="text-[11px] text-slate-500 mt-0.5">Quick order by Rx photo</p>
          </div>
        </a>

        <a href="#/products?category=prescription-medicine" class="p-4 rounded-2xl bg-white border border-slate-200/80 shadow-sm hover:shadow-card-hover transition group flex flex-col justify-between">
          <div class="w-10 h-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center mb-3 group-hover:scale-105 transition">
            <i data-lucide="pill" class="w-5 h-5"></i>
          </div>
          <div>
            <h4 class="text-xs font-bold text-slate-900 group-hover:text-brand-600 transition">Order Medicines</h4>
            <p class="text-[11px] text-slate-500 mt-0.5">Search 10,000+ brands</p>
          </div>
        </a>

        <a href="#/products?category=healthcare-devices" class="p-4 rounded-2xl bg-white border border-slate-200/80 shadow-sm hover:shadow-card-hover transition group flex flex-col justify-between">
          <div class="w-10 h-10 rounded-xl bg-teal-50 text-teal-600 flex items-center justify-center mb-3 group-hover:scale-105 transition">
            <i data-lucide="activity" class="w-5 h-5"></i>
          </div>
          <div>
            <h4 class="text-xs font-bold text-slate-900 group-hover:text-brand-600 transition">Health Devices</h4>
            <p class="text-[11px] text-slate-500 mt-0.5">BP monitors, Glucose meters</p>
          </div>
        </a>

        <a href="#/products?sort=discount" class="p-4 rounded-2xl bg-white border border-slate-200/80 shadow-sm hover:shadow-card-hover transition group flex flex-col justify-between">
          <div class="w-10 h-10 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center mb-3 group-hover:scale-105 transition">
            <i data-lucide="percent" class="w-5 h-5"></i>
          </div>
          <div>
            <h4 class="text-xs font-bold text-slate-900 group-hover:text-brand-600 transition">Special Discounts</h4>
            <p class="text-[11px] text-slate-500 mt-0.5">Up to 20% off bundles</p>
          </div>
        </a>

      </div>
    </section>

    <!-- Categories Grid (Loaded dynamically from PostgreSQL) -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6">
      <div class="flex items-center justify-between mb-5">
        <div>
          <h2 class="text-lg font-bold text-slate-900">Popular Categories</h2>
          <p class="text-xs text-slate-500">Explore authentic healthcare essentials</p>
        </div>
        <a href="#/products" class="text-xs font-bold text-brand-600 hover:text-brand-700 flex items-center gap-1">
          View All &rarr;
        </a>
      </div>

      <div id="home-categories-grid" class="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-6 gap-2 sm:gap-3">
        <!-- Loading skeletons -->
        ${Array(6).fill(0).map(() => `
          <div class="bg-white rounded-2xl border border-slate-200/60 p-2.5 sm:p-4 animate-pulse text-center">
            <div class="w-10 h-10 sm:w-12 sm:h-12 rounded-xl bg-slate-200 mx-auto mb-2 sm:mb-3"></div>
            <div class="h-3 bg-slate-200 rounded w-2/3 mx-auto"></div>
          </div>
        `).join('')}
      </div>
    </section>

    <!-- Featured Products Section -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6">
      <div class="flex items-center justify-between mb-4 sm:mb-5">
        <div>
          <h2 class="text-base sm:text-lg font-bold text-slate-900">Featured Medicines &amp; Healthcare</h2>
          <p class="text-[11px] sm:text-xs text-slate-500">Authentic medicines with verified batch &amp; expiry</p>
        </div>
        <a href="#/products?featured=true" class="text-xs font-bold text-brand-600 hover:text-brand-700 flex items-center gap-1">
          See More &rarr;
        </a>
      </div>

      <div id="home-featured-grid" class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-2.5 sm:gap-4">
        <!-- Skeletons -->
        ${Array(5).fill(0).map(() => `
          <div class="bg-white rounded-2xl border border-slate-200/60 p-4 animate-pulse space-y-3">
            <div class="aspect-square bg-slate-200 rounded-xl"></div>
            <div class="h-3 bg-slate-200 rounded w-3/4"></div>
            <div class="h-4 bg-slate-200 rounded w-1/2"></div>
          </div>
        `).join('')}
      </div>
    </section>

    <!-- Promotional Discount Section -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6">
      <div class="bg-gradient-to-r from-emerald-800 to-brand-900 rounded-3xl p-6 sm:p-8 text-white relative overflow-hidden shadow-lg">
        <div class="absolute right-0 bottom-0 opacity-10 pointer-events-none">
          <i data-lucide="cross" class="w-96 h-96 -mr-16 -mb-16"></i>
        </div>

        <div class="relative z-10 flex flex-col md:flex-row items-start md:items-center justify-between gap-6">
          <div class="space-y-2 max-w-lg">
            <span class="px-3 py-1 rounded-full bg-accent-500 text-slate-950 font-bold text-xs uppercase tracking-wider">Special Health Offers</span>
            <h3 class="text-xl sm:text-2xl font-black text-white">Save on Everyday Chronic Care Medicines</h3>
            <p class="text-xs sm:text-sm text-emerald-100 leading-relaxed">
              Use coupon code <span class="font-mono bg-white/20 px-2 py-0.5 rounded text-white font-bold">HEALTH10</span> at checkout for instant discounts on all healthcare products.
            </p>
          </div>
          <a href="#/products?sort=discount" class="btn-accent px-6 py-3 text-xs uppercase tracking-wider shadow-md">
            Shop Offers &rarr;
          </a>
        </div>
      </div>
    </section>

    <!-- Discounted Products Grid -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6">
      <div class="flex items-center justify-between mb-5">
        <div>
          <h2 class="text-lg font-bold text-slate-900">Discounted Essentials</h2>
          <p class="text-xs text-slate-500">Genuine price cuts on popular healthcare items</p>
        </div>
        <a href="#/products?sort=discount" class="text-xs font-bold text-brand-600 hover:text-brand-700 flex items-center gap-1">
          View All &rarr;
        </a>
      </div>

      <div id="home-discounted-grid" class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-2.5 sm:gap-4">
        <!-- Skeletons -->
        ${Array(5).fill(0).map(() => `
          <div class="bg-white rounded-2xl border border-slate-200/60 p-4 animate-pulse space-y-3">
            <div class="aspect-square bg-slate-200 rounded-xl"></div>
            <div class="h-3 bg-slate-200 rounded w-3/4"></div>
            <div class="h-4 bg-slate-200 rounded w-1/2"></div>
          </div>
        `).join('')}
      </div>
    </section>

  `;

  renderIcons(container);

  // Asynchronously load real catalog data from backend API
  loadHomeData(container);

  return container;
}

async function loadHomeData(container) {
  // 1. Categories
  try {
    const categories = await categoriesApi.getCategories();
    const categoriesGrid = container.querySelector('#home-categories-grid');
    if (categoriesGrid && categories && categories.length > 0) {
      // Pick top-level categories
      const topCategories = categories.slice(0, 6);
      categoriesGrid.innerHTML = topCategories.map(cat => {
        let iconName = 'pill';
        if (cat.slug.includes('device')) iconName = 'activity';
        else if (cat.slug.includes('baby')) iconName = 'baby';
        else if (cat.slug.includes('beauty') || cat.slug.includes('skin')) iconName = 'sparkles';
        else if (cat.slug.includes('vitamin')) iconName = 'shield-plus';
        else if (cat.slug.includes('personal')) iconName = 'heart';

        return `
          <a href="#/products?category=${escapeHtml(cat.slug)}" class="bg-white rounded-2xl border border-slate-200/70 p-4 hover:border-brand-500 hover:shadow-card-hover transition text-center group flex flex-col items-center justify-center">
            <div class="w-12 h-12 rounded-2xl bg-brand-50 text-brand-700 flex items-center justify-center mb-2.5 group-hover:scale-110 transition">
              <i data-lucide="${iconName}" class="w-6 h-6"></i>
            </div>
            <span class="text-xs font-bold text-slate-800 group-hover:text-brand-600 transition line-clamp-1">${escapeHtml(cat.name)}</span>
            <span class="text-[10px] text-slate-400 font-sans mt-0.5">${escapeHtml(cat.nameBn || '')}</span>
          </a>
        `;
      }).join('');
      renderIcons(categoriesGrid);
    }
  } catch (err) {
    console.warn('Failed to load categories:', err);
  }

  // 2. Featured Products
  try {
    const featured = await productsApi.getFeatured(10);
    const featuredGrid = container.querySelector('#home-featured-grid');
    if (featuredGrid && featured && featured.length > 0) {
      featuredGrid.innerHTML = '';
      featured.forEach(product => {
        featuredGrid.appendChild(renderProductCard(product));
      });
    }
  } catch (err) {
    console.warn('Failed to load featured products:', err);
  }

  // 3. Discounted Products
  try {
    const discounted = await productsApi.getDiscounted(10);
    const discountedGrid = container.querySelector('#home-discounted-grid');
    if (discountedGrid && discounted && discounted.length > 0) {
      discountedGrid.innerHTML = '';
      discounted.forEach(product => {
        discountedGrid.appendChild(renderProductCard(product));
      });
    }
  } catch (err) {
    console.warn('Failed to load discounted products:', err);
  }
}
