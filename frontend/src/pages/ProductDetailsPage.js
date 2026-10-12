import { productsApi } from '../api/products.js';
import { cartStore } from '../state/cartStore.js';
import { renderProductCard } from '../components/ProductCard.js';
import { escapeHtml, formatCurrency } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export async function renderProductDetailsPage(slug) {
  const container = document.createElement('div');
  container.className = 'max-w-7xl mx-auto px-3 sm:px-6 py-4 sm:py-8 space-y-6 sm:space-y-10 pb-28 lg:pb-10 w-full max-w-full';

  // Skeleton loading state
  container.innerHTML = `
    <div class="animate-pulse space-y-6">
      <div class="h-4 bg-slate-200 rounded w-1/4"></div>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
        <div class="aspect-square bg-slate-200 rounded-3xl"></div>
        <div class="space-y-4">
          <div class="h-8 bg-slate-200 rounded w-3/4"></div>
          <div class="h-4 bg-slate-200 rounded w-1/2"></div>
          <div class="h-10 bg-slate-200 rounded w-1/3"></div>
        </div>
      </div>
    </div>
  `;

  try {
    const product = await productsApi.getProductBySlug(slug);
    if (!product) {
      renderNotFound(container);
      return container;
    }

    renderProductContent(container, product);
  } catch (err) {
    renderNotFound(container, err.message);
  }

  return container;
}

function renderNotFound(container, message) {
  container.innerHTML = `
    <div class="text-center py-16 sm:py-20 bg-white rounded-3xl border border-slate-200/80 p-6 sm:p-8 space-y-4">
      <div class="w-16 h-16 rounded-full bg-rose-50 text-rose-500 flex items-center justify-center mx-auto">
        <i data-lucide="alert-octagon" class="w-8 h-8"></i>
      </div>
      <h2 class="text-lg sm:text-xl font-bold text-slate-900">Product Not Found</h2>
      <p class="text-xs text-slate-500 max-w-sm mx-auto">${escapeHtml(message || 'The product you are looking for does not exist or may have been unlisted.')}</p>
      <a href="#/products" class="btn-primary text-xs py-2.5 px-5 inline-flex">
        Back to Catalog
      </a>
    </div>
  `;
  renderIcons(container);
}

function renderProductContent(container, product) {
  const isOutOfStock = product.stockQuantity !== undefined && product.stockQuantity <= 0;
  const hasDiscount = product.discountPrice && product.discountPrice < product.basePrice;
  const currentPrice = hasDiscount ? product.discountPrice : product.basePrice;
  const discountPct = hasDiscount 
    ? Math.round(((product.basePrice - product.discountPrice) / product.basePrice) * 100) 
    : 0;

  const images = product.images && product.images.length > 0 
    ? product.images 
    : [{ imageUrl: product.imageUrl || '/assets/medicine-placeholder.png', altText: product.name }];

  container.innerHTML = `
    <!-- Breadcrumb -->
    <div class="flex items-center gap-1.5 text-xs text-slate-500 overflow-x-auto whitespace-nowrap pb-1 no-scrollbar">
      <a href="#/" class="hover:text-brand-600 flex-shrink-0">Home</a>
      <i data-lucide="chevron-right" class="w-3.5 h-3.5 flex-shrink-0"></i>
      <a href="#/products" class="hover:text-brand-600 flex-shrink-0">Products</a>
      ${product.categoryName ? `
        <i data-lucide="chevron-right" class="w-3.5 h-3.5 flex-shrink-0"></i>
        <a href="#/products?category=${encodeURIComponent(product.categorySlug || '')}" class="hover:text-brand-600 truncate max-w-[120px] sm:max-w-xs flex-shrink-0">${escapeHtml(product.categoryName)}</a>
      ` : ''}
      <i data-lucide="chevron-right" class="w-3.5 h-3.5 flex-shrink-0"></i>
      <span class="text-slate-900 font-semibold truncate max-w-[140px] sm:max-w-xs">${escapeHtml(product.name)}</span>
    </div>

    <!-- Main Product Section -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 lg:gap-8 items-start">
      
      <!-- Gallery Column -->
      <div class="lg:col-span-5 space-y-3 sm:space-y-4">
        <div class="bg-white rounded-3xl border border-slate-200/80 p-6 sm:p-8 shadow-card flex items-center justify-center aspect-square max-h-80 sm:max-h-96 mx-auto w-full overflow-hidden relative group">
          <img 
            id="main-product-image"
            src="${escapeHtml(images[0]?.imageUrl)}" 
            alt="${escapeHtml(product.name)}" 
            class="max-h-60 sm:max-h-72 max-w-full object-contain group-hover:scale-105 transition-transform duration-300"
            onerror="this.src='data:image/svg+xml;utf8,<svg xmlns=\\'http://www.w3.org/2000/svg\\' width=\\'96\\' height=\\'96\\' viewBox=\\'0 0 24 24\\' fill=\\'none\\' stroke=\\'%2394a3b8\\' stroke-width=\\'1.5\\'><path d=\\'m10.5 20.5 10-10a4.95 4.95 0 1 0-7-7l-10 10a4.95 4.95 0 1 0 7 7Z\\'/><path d=\\'m8.5 8.5 7 7\\'/></svg>'"
          />
          ${hasDiscount ? `
            <div class="absolute top-3 left-3 sm:top-4 sm:left-4 badge-discount font-bold shadow-sm">
              -${discountPct}% OFF
            </div>
          ` : ''}
        </div>

        <!-- Thumbnails -->
        ${images.length > 1 ? `
          <div class="flex gap-2 sm:gap-3 overflow-x-auto pb-2 no-scrollbar">
            ${images.map((img, i) => `
              <button type="button" class="thumbnail-btn w-14 h-14 sm:w-16 sm:h-16 rounded-xl border-2 ${i === 0 ? 'border-brand-500' : 'border-slate-200'} bg-white p-1 flex-shrink-0 flex items-center justify-center overflow-hidden hover:border-brand-400 transition" data-img-url="${escapeHtml(img.imageUrl)}">
                <img src="${escapeHtml(img.imageUrl)}" alt="${escapeHtml(img.altText || '')}" class="max-h-full max-w-full object-contain"/>
              </button>
            `).join('')}
          </div>
        ` : ''}
      </div>

      <!-- Info Column -->
      <div class="lg:col-span-7 bg-white rounded-3xl border border-slate-200/80 p-5 sm:p-8 shadow-card space-y-5 sm:space-y-6">
        
        <!-- Header Info -->
        <div class="space-y-2 border-b border-slate-100 pb-4 sm:pb-5">
          <div class="flex flex-wrap items-center gap-2">
            <span class="text-xs font-bold text-brand-700 bg-brand-50 px-2.5 py-0.5 rounded-full border border-brand-200/60 uppercase tracking-wide">
              ${escapeHtml(product.dosageForm || 'Medicine')}
            </span>
            ${product.isPrescriptionRequired ? `
              <span class="badge-prescription">
                <i data-lucide="file-text" class="w-3.5 h-3.5"></i> Prescription Required
              </span>
            ` : `
              <span class="badge-otc">
                <i data-lucide="check" class="w-3.5 h-3.5"></i> OTC Drug
              </span>
            `}
          </div>

          <h1 class="text-xl sm:text-3xl font-extrabold text-slate-900 tracking-tight leading-snug">
            ${escapeHtml(product.name)}
          </h1>

          <div class="flex flex-wrap items-center gap-x-3 sm:gap-x-4 gap-y-1 text-xs text-slate-600 font-medium pt-1">
            <span>Generic: <strong class="text-slate-900 font-bold">${escapeHtml(product.genericName || 'N/A')}</strong></span>
            <span>•</span>
            <span>Strength: <strong class="text-slate-900 font-bold">${escapeHtml(product.strength || 'N/A')}</strong></span>
            <span>•</span>
            <span>Manufacturer: <strong class="text-brand-700 font-bold">${escapeHtml(product.manufacturerName || 'N/A')}</strong></span>
          </div>
        </div>

        <!-- Pricing Card -->
        <div class="bg-slate-50/70 rounded-2xl p-4 sm:p-5 border border-slate-200/60 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 sm:gap-4">
          <div>
            <div class="text-[10px] sm:text-[11px] font-semibold text-slate-500 uppercase tracking-wider">Unit / Pack Price</div>
            <div class="flex items-baseline gap-2 mt-0.5">
              <span class="text-2xl sm:text-3xl font-black text-slate-900">${formatCurrency(currentPrice)}</span>
              ${hasDiscount ? `
                <span class="text-xs sm:text-sm text-slate-400 line-through font-medium">${formatCurrency(product.basePrice)}</span>
                <span class="text-[11px] sm:text-xs font-bold text-emerald-700 bg-emerald-100 px-2 py-0.5 rounded">Save ${formatCurrency(product.basePrice - product.discountPrice)}</span>
              ` : ''}
            </div>
            <p class="text-[10px] sm:text-[11px] text-slate-500 mt-1">${escapeHtml(product.packSize || 'Single Unit')} • MRP inclusive of all taxes</p>
          </div>

          <!-- Stock Indicator -->
          <div class="text-left sm:text-right">
            ${isOutOfStock ? `
              <span class="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-bold bg-rose-50 text-rose-700 border border-rose-200">
                <i data-lucide="x-circle" class="w-3.5 h-3.5"></i> Out of Stock
              </span>
            ` : `
              <span class="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-bold bg-emerald-50 text-emerald-700 border border-emerald-200">
                <i data-lucide="check-circle-2" class="w-3.5 h-3.5"></i> In Stock (${product.stockQuantity ?? 'Available'})
              </span>
            `}
          </div>
        </div>

        <!-- Prescription Warning Banner -->
        ${product.isPrescriptionRequired ? `
          <div class="p-3.5 bg-rose-50/70 border border-rose-200 rounded-xl text-xs text-rose-800 flex items-start gap-2.5">
            <i data-lucide="alert-triangle" class="w-4 h-4 text-rose-600 flex-shrink-0 mt-0.5"></i>
            <div>
              <strong class="font-bold">Prescription Required for this Medicine:</strong>
              <p class="text-[11px] text-rose-700 mt-0.5">Under DGDA regulations, this medication requires a valid prescription written by a registered BMDC doctor. You can upload it during checkout.</p>
            </div>
          </div>
        ` : ''}

        <!-- Add to cart and Quantity Controls -->
        <div class="space-y-4 pt-1 sm:pt-2">
          <div class="flex flex-col sm:flex-row items-stretch sm:items-center gap-3">
            <!-- Stepper -->
            <div class="inline-flex items-center border-2 border-slate-200 rounded-xl bg-white overflow-hidden shadow-sm self-start min-h-[44px]">
              <button type="button" id="qty-decrease-btn" class="p-2.5 px-3.5 text-slate-600 hover:bg-slate-100 active:bg-slate-200 tap-target-44 flex items-center justify-center">
                <i data-lucide="minus" class="w-4 h-4"></i>
              </button>
              <input type="number" id="detail-qty-input" value="1" min="1" max="50" class="w-14 text-center text-base sm:text-sm font-bold text-slate-800 focus:outline-none"/>
              <button type="button" id="qty-increase-btn" class="p-2.5 px-3.5 text-slate-600 hover:bg-slate-100 active:bg-slate-200 tap-target-44 flex items-center justify-center">
                <i data-lucide="plus" class="w-4 h-4"></i>
              </button>
            </div>

            <!-- Add to Cart CTA -->
            <button 
              type="button" 
              id="detail-add-to-cart-btn" 
              class="flex-1 btn-primary py-3 font-bold text-xs sm:text-sm shadow-lg shadow-brand-600/25 min-h-[44px] ${isOutOfStock ? 'opacity-50 cursor-not-allowed' : ''}"
              ${isOutOfStock ? 'disabled' : ''}
            >
              <i data-lucide="shopping-bag" class="w-4 h-4"></i>
              <span>${isOutOfStock ? 'Out of Stock' : 'Add to Cart'}</span>
            </button>
          </div>
        </div>

        <!-- Medical Details Tabs/Accordion -->
        <div class="border-t border-slate-100 pt-5 sm:pt-6 space-y-4 text-xs text-slate-700">
          <div>
            <h3 class="font-bold text-sm text-slate-900 mb-1.5">Description &amp; Indicative Uses</h3>
            <p class="leading-relaxed text-slate-600">${escapeHtml(product.description || 'Verified pharmaceutical formulation registered with the Directorate General of Drug Administration (DGDA).')}</p>
          </div>

          ${product.usageInstructions ? `
            <div>
              <h3 class="font-bold text-sm text-slate-900 mb-1">Dosage &amp; Administration</h3>
              <p class="leading-relaxed text-slate-600">${escapeHtml(product.usageInstructions)}</p>
            </div>
          ` : ''}

          ${product.storageConditions ? `
            <div>
              <h3 class="font-bold text-sm text-slate-900 mb-1">Storage Conditions</h3>
              <p class="leading-relaxed text-slate-600">${escapeHtml(product.storageConditions)}</p>
            </div>
          ` : ''}

          <!-- Medical Disclaimer -->
          <div class="p-3 bg-amber-50/70 border border-amber-200/80 rounded-xl text-[11px] text-amber-900 leading-relaxed">
            <strong>Important Medical Disclaimer:</strong> The product information provided on this page is for educational and informational purposes only and does not substitute for the professional advice, diagnosis, or treatment of a qualified healthcare provider.
          </div>
        </div>

      </div>
    </div>

    <!-- Mobile Sticky Purchase Bar (< lg) -->
    <div id="mobile-sticky-purchase-bar" class="lg:hidden fixed bottom-14 left-0 right-0 z-30 bg-white/95 backdrop-blur-md border-t border-slate-200/90 shadow-[0_-4px_16px_rgba(0,0,0,0.08)] px-3 sm:px-4 py-2.5 flex items-center justify-between gap-3">
      <div class="min-w-0 flex-1">
        <div class="text-[11px] text-slate-500 truncate">${escapeHtml(product.name)}</div>
        <div class="text-base font-extrabold text-slate-900">${formatCurrency(currentPrice)}</div>
      </div>
      <button 
        type="button" 
        id="sticky-add-to-cart-btn" 
        class="btn-primary py-2.5 px-4 sm:px-5 font-bold text-xs shadow-md shadow-brand-600/20 flex-shrink-0 min-h-[40px] ${isOutOfStock ? 'opacity-50 cursor-not-allowed' : ''}"
        ${isOutOfStock ? 'disabled' : ''}
      >
        <i data-lucide="shopping-bag" class="w-4 h-4"></i>
        <span>${isOutOfStock ? 'Out of Stock' : 'Add to Cart'}</span>
      </button>
    </div>
  `;

  renderIcons(container);

  // Setup thumbnail gallery switcher
  container.querySelectorAll('.thumbnail-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      const imgUrl = btn.dataset.imgUrl;
      const mainImg = container.querySelector('#main-product-image');
      if (mainImg) mainImg.src = imgUrl;

      container.querySelectorAll('.thumbnail-btn').forEach(b => b.classList.replace('border-brand-500', 'border-slate-200'));
      btn.classList.replace('border-slate-200', 'border-brand-500');
    });
  });

  // Quantity stepper
  const qtyInput = container.querySelector('#detail-qty-input');
  container.querySelector('#qty-decrease-btn')?.addEventListener('click', () => {
    let q = parseInt(qtyInput.value, 10) || 1;
    if (q > 1) qtyInput.value = q - 1;
  });
  container.querySelector('#qty-increase-btn')?.addEventListener('click', () => {
    let q = parseInt(qtyInput.value, 10) || 1;
    if (q < 50) qtyInput.value = q + 1;
  });

  // Add to cart helper function
  const handleAddToCart = async (triggerBtn) => {
    if (isOutOfStock) return;
    const qty = parseInt(qtyInput.value, 10) || 1;
    triggerBtn.disabled = true;
    const originalText = triggerBtn.innerHTML;
    triggerBtn.innerHTML = `<i data-lucide="loader" class="w-4 h-4 animate-spin"></i> Adding...`;
    renderIcons(triggerBtn);

    try {
      await cartStore.addItem(product.id, qty);
      triggerBtn.innerHTML = `<i data-lucide="check" class="w-4 h-4"></i> Added!`;
      renderIcons(triggerBtn);
      cartStore.openDrawer();
      setTimeout(() => {
        triggerBtn.innerHTML = originalText;
        triggerBtn.disabled = false;
        renderIcons(triggerBtn);
      }, 2000);
    } catch (e) {
      triggerBtn.innerHTML = originalText;
      triggerBtn.disabled = false;
      renderIcons(triggerBtn);
    }
  };

  const addBtn = container.querySelector('#detail-add-to-cart-btn');
  addBtn?.addEventListener('click', () => handleAddToCart(addBtn));

  const stickyBtn = container.querySelector('#sticky-add-to-cart-btn');
  stickyBtn?.addEventListener('click', () => handleAddToCart(stickyBtn));
}
