import { productsApi } from '../api/products.js';
import { categoriesApi } from '../api/categories.js';
import { lookupsApi } from '../api/lookups.js';
import { renderProductCard } from '../components/ProductCard.js';
import { escapeHtml } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export async function renderCatalogPage(queryParams = {}) {
  const container = document.createElement('div');
  container.className = 'max-w-7xl mx-auto px-3 sm:px-6 py-4 sm:py-6 space-y-4 sm:space-y-6 w-full max-w-full';

  const query = queryParams.query || '';
  const categorySlug = queryParams.category || '';
  const dosageFormId = queryParams.dosage || '';
  const manufacturerId = queryParams.manufacturer || '';
  const isPrescriptionRequired = queryParams.rx !== undefined ? queryParams.rx : '';
  const sortBy = queryParams.sort || 'popular';
  const page = parseInt(queryParams.page || '1', 10);
  const pageSize = 16;

  // Calculate active filter count
  let activeFilterCount = 0;
  if (categorySlug) activeFilterCount++;
  if (dosageFormId) activeFilterCount++;
  if (manufacturerId) activeFilterCount++;
  if (isPrescriptionRequired !== '') activeFilterCount++;

  container.innerHTML = `
    <!-- Breadcrumb & Title -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-slate-200/80 pb-3 sm:pb-4">
      <div>
        <div class="flex items-center gap-1.5 text-xs text-slate-500 mb-1">
          <a href="#/" class="hover:text-brand-600">Home</a>
          <i data-lucide="chevron-right" class="w-3.5 h-3.5"></i>
          <span class="text-slate-800 font-semibold">Products</span>
          ${categorySlug ? `
            <i data-lucide="chevron-right" class="w-3.5 h-3.5"></i>
            <span class="text-brand-600 font-semibold truncate max-w-[150px]">${escapeHtml(categorySlug)}</span>
          ` : ''}
        </div>
        <h1 class="text-lg sm:text-2xl font-bold text-slate-900">
          ${query ? `Search results for "${escapeHtml(query)}"` : (categorySlug ? `Category: ${escapeHtml(categorySlug)}` : 'All Products & Medicines')}
        </h1>
      </div>

      <!-- Desktop Sort control -->
      <div class="hidden lg:flex items-center gap-2">
        <label for="catalog-sort" class="text-xs font-medium text-slate-600 whitespace-nowrap">Sort by:</label>
        <select id="catalog-sort" class="form-input text-base sm:text-xs py-2 w-auto bg-white">
          <option value="popular" ${sortBy === 'popular' ? 'selected' : ''}>Popularity</option>
          <option value="price_asc" ${sortBy === 'price_asc' ? 'selected' : ''}>Price: Low to High</option>
          <option value="price_desc" ${sortBy === 'price_desc' ? 'selected' : ''}>Price: High to Low</option>
          <option value="newest" ${sortBy === 'newest' ? 'selected' : ''}>Newest Arrivals</option>
          <option value="discount" ${sortBy === 'discount' ? 'selected' : ''}>Biggest Discount</option>
        </select>
      </div>
    </div>

    <!-- Mobile Filter & Sort Bar (< lg) -->
    <div class="lg:hidden flex items-center justify-between gap-2.5 bg-white rounded-xl border border-slate-200 p-2 shadow-sm">
      <button type="button" id="mobile-filter-open-btn" class="flex-1 btn-secondary text-xs py-2 px-3 font-bold flex items-center justify-center gap-1.5 min-h-[40px]">
        <i data-lucide="sliders-horizontal" class="w-4 h-4 text-brand-600"></i>
        <span>Filters</span>
        ${activeFilterCount > 0 ? `
          <span class="w-5 h-5 rounded-full bg-brand-600 text-white text-[10px] font-bold flex items-center justify-center">
            ${activeFilterCount}
          </span>
        ` : ''}
      </button>

      <div class="flex-1 flex items-center gap-1 min-w-0">
        <select id="catalog-sort-mobile" class="form-input text-base sm:text-xs py-2 px-2.5 bg-white font-medium min-h-[40px]">
          <option value="popular" ${sortBy === 'popular' ? 'selected' : ''}>Sort: Popular</option>
          <option value="price_asc" ${sortBy === 'price_asc' ? 'selected' : ''}>Price: Low to High</option>
          <option value="price_desc" ${sortBy === 'price_desc' ? 'selected' : ''}>Price: High to Low</option>
          <option value="newest" ${sortBy === 'newest' ? 'selected' : ''}>Newest</option>
          <option value="discount" ${sortBy === 'discount' ? 'selected' : ''}>Discount</option>
        </select>
      </div>
    </div>

    <!-- Layout: Filter Sidebar + Products Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-4 gap-6 items-start">
      
      <!-- Filter Sidebar (Responsive Modal on mobile, Sticky Sidebar on desktop) -->
      <div id="filter-backdrop" class="fixed inset-0 bg-slate-950/50 backdrop-blur-sm z-50 opacity-0 pointer-events-none transition-opacity duration-300 lg:hidden"></div>

      <aside id="catalog-filter-aside" class="fixed inset-y-0 right-0 z-50 w-80 max-w-[85vw] bg-white shadow-2xl flex flex-col transform translate-x-full transition-transform duration-300 ease-in-out lg:static lg:transform-none lg:z-auto lg:w-auto lg:max-w-none lg:shadow-sm lg:rounded-2xl lg:border lg:border-slate-200/80 lg:p-5 overflow-hidden">
        
        <!-- Sidebar Header -->
        <div class="p-4 lg:p-0 flex items-center justify-between border-b border-slate-100 lg:pb-3 bg-slate-50/50 lg:bg-transparent">
          <span class="text-sm font-bold text-slate-900 flex items-center gap-1.5">
            <i data-lucide="sliders-horizontal" class="w-4 h-4 text-brand-600"></i> Filter Medicines
          </span>
          <div class="flex items-center gap-3">
            <button type="button" id="clear-filters-btn" class="text-xs font-semibold text-brand-600 hover:text-brand-700">
              Clear All
            </button>
            <button type="button" id="mobile-filter-close-btn" class="lg:hidden p-1 text-slate-400 hover:text-slate-700" aria-label="Close filters">
              <i data-lucide="x" class="w-5 h-5"></i>
            </button>
          </div>
        </div>

        <!-- Filter Body (Scrollable on mobile) -->
        <div class="flex-1 overflow-y-auto p-4 lg:p-0 space-y-5 lg:space-y-6 lg:mt-4">
          <!-- Category Filter -->
          <div class="space-y-1.5">
            <label for="filter-category" class="text-xs font-bold text-slate-800 uppercase tracking-wider block">Category</label>
            <select id="filter-category" class="form-input text-base sm:text-xs py-2">
              <option value="">All Categories</option>
            </select>
          </div>

          <!-- Dosage Form Filter -->
          <div class="space-y-1.5">
            <label for="filter-dosage" class="text-xs font-bold text-slate-800 uppercase tracking-wider block">Dosage Form</label>
            <select id="filter-dosage" class="form-input text-base sm:text-xs py-2">
              <option value="">All Dosage Forms</option>
            </select>
          </div>

          <!-- Manufacturer Filter -->
          <div class="space-y-1.5">
            <label for="filter-manufacturer" class="text-xs font-bold text-slate-800 uppercase tracking-wider block">Manufacturer</label>
            <select id="filter-manufacturer" class="form-input text-base sm:text-xs py-2">
              <option value="">All Pharmaceutical Brands</option>
            </select>
          </div>

          <!-- Prescription Requirement -->
          <div class="space-y-2 pt-2 border-t border-slate-100">
            <h4 class="text-xs font-bold text-slate-800 uppercase tracking-wider">Prescription Status</h4>
            <div class="space-y-2 text-xs text-slate-700">
              <label class="flex items-center gap-2.5 cursor-pointer py-1">
                <input type="radio" name="rx-filter" value="" ${!isPrescriptionRequired ? 'checked' : ''} class="w-4 h-4 text-brand-600 focus:ring-brand-500"/>
                <span>All Products</span>
              </label>
              <label class="flex items-center gap-2.5 cursor-pointer py-1">
                <input type="radio" name="rx-filter" value="true" ${isPrescriptionRequired === 'true' ? 'checked' : ''} class="w-4 h-4 text-brand-600 focus:ring-brand-500"/>
                <span>Prescription Only (Rx)</span>
              </label>
              <label class="flex items-center gap-2.5 cursor-pointer py-1">
                <input type="radio" name="rx-filter" value="false" ${isPrescriptionRequired === 'false' ? 'checked' : ''} class="w-4 h-4 text-brand-600 focus:ring-brand-500"/>
                <span>Over The Counter (OTC)</span>
              </label>
            </div>
          </div>
        </div>

        <!-- Sidebar Footer Action -->
        <div class="p-4 lg:p-0 border-t border-slate-100 lg:border-t-0 bg-slate-50/50 lg:bg-transparent lg:mt-5">
          <button type="button" id="apply-filters-btn" class="btn-primary w-full text-xs py-2.5 font-bold shadow-md shadow-brand-600/20">
            Apply Filters
          </button>
        </div>
      </aside>

      <!-- Main Grid Area -->
      <main class="lg:col-span-3 space-y-4 sm:space-y-6 w-full min-w-0">
        
        <!-- Active Filter Badges & Counter -->
        <div class="flex items-center justify-between text-xs text-slate-500">
          <span id="catalog-results-count">Loading products...</span>
        </div>

        <!-- Products Grid Container -->
        <div id="catalog-products-grid" class="grid grid-cols-2 sm:grid-cols-3 xl:grid-cols-4 gap-2.5 sm:gap-4">
          <!-- Loading skeletons -->
          ${Array(8).fill(0).map(() => `
            <div class="bg-white rounded-2xl border border-slate-200/70 p-3 sm:p-4 animate-pulse space-y-3">
              <div class="aspect-square bg-slate-200 rounded-xl"></div>
              <div class="h-3 bg-slate-200 rounded w-3/4"></div>
              <div class="h-4 bg-slate-200 rounded w-1/2"></div>
            </div>
          `).join('')}
        </div>

        <!-- Pagination Controls -->
        <div id="catalog-pagination" class="flex items-center justify-center gap-2 pt-4 sm:pt-6 border-t border-slate-200/80 hidden">
        </div>
      </main>
    </div>
  `;

  renderIcons(container);

  // Setup data fetching & events
  setupCatalogEvents(container, {
    query,
    categorySlug,
    dosageFormId,
    manufacturerId,
    isPrescriptionRequired,
    sortBy,
    page,
    pageSize,
  });

  return container;
}

async function setupCatalogEvents(container, filterState) {
  // Mobile filter drawer controls
  const filterBackdrop = container.querySelector('#filter-backdrop');
  const filterAside = container.querySelector('#catalog-filter-aside');
  const openFilterBtn = container.querySelector('#mobile-filter-open-btn');
  const closeFilterBtn = container.querySelector('#mobile-filter-close-btn');

  const openDrawer = () => {
    filterBackdrop?.classList.remove('opacity-0', 'pointer-events-none');
    filterBackdrop?.classList.add('opacity-100');
    filterAside?.classList.remove('translate-x-full');
    document.body.classList.add('overflow-hidden');
  };

  const closeDrawer = () => {
    filterBackdrop?.classList.add('opacity-0', 'pointer-events-none');
    filterBackdrop?.classList.remove('opacity-100');
    filterAside?.classList.add('translate-x-full');
    document.body.classList.remove('overflow-hidden');
  };

  openFilterBtn?.addEventListener('click', openDrawer);
  filterBackdrop?.addEventListener('click', closeDrawer);
  closeFilterBtn?.addEventListener('click', closeDrawer);

  // Populate Filter Selects
  try {
    const [categories, dosageForms, manufacturers] = await Promise.all([
      categoriesApi.getCategories(),
      lookupsApi.getDosageForms(),
      lookupsApi.getManufacturers(),
    ]);

    const catSelect = container.querySelector('#filter-category');
    if (catSelect && categories) {
      categories.forEach(c => {
        const opt = document.createElement('option');
        opt.value = c.slug;
        opt.textContent = `${c.name} (${c.productCount ?? 0})`;
        if (c.slug === filterState.categorySlug) opt.selected = true;
        catSelect.appendChild(opt);
      });
    }

    const dosageSelect = container.querySelector('#filter-dosage');
    if (dosageSelect && dosageForms) {
      dosageForms.forEach(d => {
        const opt = document.createElement('option');
        opt.value = d.id;
        opt.textContent = d.name;
        if (String(d.id) === String(filterState.dosageFormId)) opt.selected = true;
        dosageSelect.appendChild(opt);
      });
    }

    const mfgSelect = container.querySelector('#filter-manufacturer');
    if (mfgSelect && manufacturers) {
      manufacturers.forEach(m => {
        const opt = document.createElement('option');
        opt.value = m.id;
        opt.textContent = m.name;
        if (String(m.id) === String(filterState.manufacturerId)) opt.selected = true;
        mfgSelect.appendChild(opt);
      });
    }
  } catch (e) {
    console.warn('Filter options failed to load:', e);
  }

  // Setup Sort Handlers (Desktop + Mobile)
  const sortSelect = container.querySelector('#catalog-sort');
  const sortSelectMobile = container.querySelector('#catalog-sort-mobile');

  sortSelect?.addEventListener('change', () => {
    updateUrlAndReload({ ...filterState, sortBy: sortSelect.value, page: 1 });
  });

  sortSelectMobile?.addEventListener('change', () => {
    updateUrlAndReload({ ...filterState, sortBy: sortSelectMobile.value, page: 1 });
  });

  // Apply Filters
  container.querySelector('#apply-filters-btn')?.addEventListener('click', () => {
    closeDrawer();
    const cat = container.querySelector('#filter-category')?.value || '';
    const dosage = container.querySelector('#filter-dosage')?.value || '';
    const mfg = container.querySelector('#filter-manufacturer')?.value || '';
    const rx = container.querySelector('input[name="rx-filter"]:checked')?.value || '';

    updateUrlAndReload({
      ...filterState,
      categorySlug: cat,
      dosageFormId: dosage,
      manufacturerId: mfg,
      isPrescriptionRequired: rx,
      page: 1,
    });
  });

  container.querySelector('#clear-filters-btn')?.addEventListener('click', () => {
    closeDrawer();
    window.location.hash = '#/products';
  });

  // Fetch real products
  await fetchProducts(container, filterState);
}

function updateUrlAndReload(state) {
  const params = new URLSearchParams();
  if (state.query) params.set('query', state.query);
  if (state.categorySlug) params.set('category', state.categorySlug);
  if (state.dosageFormId) params.set('dosage', state.dosageFormId);
  if (state.manufacturerId) params.set('manufacturer', state.manufacturerId);
  if (state.isPrescriptionRequired) params.set('rx', state.isPrescriptionRequired);
  if (state.sortBy && state.sortBy !== 'popular') params.set('sort', state.sortBy);
  if (state.page && state.page > 1) params.set('page', state.page);

  window.location.hash = `#/products?${params.toString()}`;
}

async function fetchProducts(container, filterState) {
  const grid = container.querySelector('#catalog-products-grid');
  const countEl = container.querySelector('#catalog-results-count');
  const paginationEl = container.querySelector('#catalog-pagination');

  try {
    const apiParams = {
      query: filterState.query || undefined,
      categorySlug: filterState.categorySlug || undefined,
      dosageFormId: filterState.dosageFormId ? parseInt(filterState.dosageFormId, 10) : undefined,
      manufacturerId: filterState.manufacturerId ? parseInt(filterState.manufacturerId, 10) : undefined,
      isPrescriptionRequired: filterState.isPrescriptionRequired !== '' ? filterState.isPrescriptionRequired === 'true' : undefined,
      sortBy: filterState.sortBy,
      page: filterState.page,
      pageSize: filterState.pageSize,
    };

    const result = await productsApi.getProducts(apiParams);
    const items = result.items || [];
    const totalCount = result.totalCount || 0;
    const totalPages = result.totalPages || 1;

    countEl.textContent = `Showing ${items.length} of ${totalCount} products`;

    if (items.length === 0) {
      grid.innerHTML = `
        <div class="col-span-full py-16 text-center space-y-4 bg-white rounded-2xl border border-slate-200/80 p-6 sm:p-8">
          <div class="w-16 h-16 rounded-full bg-slate-100 text-slate-400 flex items-center justify-center mx-auto">
            <i data-lucide="package-search" class="w-8 h-8"></i>
          </div>
          <div>
            <h3 class="text-base font-bold text-slate-800">No products found</h3>
            <p class="text-xs text-slate-500 mt-1 max-w-sm mx-auto">Try clearing your filters or searching with different keywords.</p>
          </div>
          <a href="#/products" class="btn-secondary text-xs py-2 px-4 inline-flex">
            Reset Filters
          </a>
        </div>
      `;
      paginationEl.classList.add('hidden');
    } else {
      grid.innerHTML = '';
      items.forEach(product => {
        grid.appendChild(renderProductCard(product));
      });

      // Render pagination
      if (totalPages > 1) {
        paginationEl.classList.remove('hidden');
        paginationEl.innerHTML = `
          <button type="button" id="prev-page-btn" class="btn-secondary text-xs py-2 px-3 tap-target-44 flex items-center justify-center" ${filterState.page <= 1 ? 'disabled' : ''}>
            &larr; Prev
          </button>
          <span class="text-xs text-slate-600 px-3 font-medium">Page ${filterState.page} of ${totalPages}</span>
          <button type="button" id="next-page-btn" class="btn-secondary text-xs py-2 px-3 tap-target-44 flex items-center justify-center" ${filterState.page >= totalPages ? 'disabled' : ''}>
            Next &rarr;
          </button>
        `;

        paginationEl.querySelector('#prev-page-btn')?.addEventListener('click', () => {
          if (filterState.page > 1) {
            updateUrlAndReload({ ...filterState, page: filterState.page - 1 });
          }
        });

        paginationEl.querySelector('#next-page-btn')?.addEventListener('click', () => {
          if (filterState.page < totalPages) {
            updateUrlAndReload({ ...filterState, page: filterState.page + 1 });
          }
        });
      } else {
        paginationEl.classList.add('hidden');
      }
    }

    renderIcons(container);
  } catch (err) {
    countEl.textContent = 'Failed to load products';
    grid.innerHTML = `
      <div class="col-span-full p-8 text-center bg-rose-50 border border-rose-200 rounded-2xl text-rose-800 text-xs">
        <p class="font-bold">Error loading catalog</p>
        <p class="mt-1">${escapeHtml(err.message || 'Please check your connection and try again.')}</p>
      </div>
    `;
  }
}
