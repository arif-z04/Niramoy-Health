import { cartStore } from '../state/cartStore.js';
import { escapeHtml, formatCurrency } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export function renderProductCard(product) {
  const isOutOfStock = product.stockQuantity !== undefined && product.stockQuantity <= 0;
  const hasDiscount = product.discountPrice && product.discountPrice < product.basePrice;
  const currentPrice = hasDiscount ? product.discountPrice : product.basePrice;
  const discountPct = hasDiscount 
    ? Math.round(((product.basePrice - product.discountPrice) / product.basePrice) * 100) 
    : 0;

  const card = document.createElement('div');
  card.className = 'group bg-white rounded-2xl border border-slate-200/80 shadow-card hover:shadow-card-hover transition-all duration-200 flex flex-col justify-between overflow-hidden relative';

  card.innerHTML = `
    <div>
      <!-- Top badges: Discount & Rx -->
      <div class="absolute top-2.5 left-2.5 right-2.5 flex items-center justify-between z-10 pointer-events-none">
        <div class="flex flex-col gap-1">
          ${hasDiscount ? `
            <span class="badge-discount font-bold shadow-sm">
              -${discountPct}%
            </span>
          ` : ''}
          ${product.isPrescriptionRequired ? `
            <span class="badge-prescription shadow-sm">
              <i data-lucide="file-text" class="w-3 h-3"></i> Rx
            </span>
          ` : ''}
        </div>
        ${product.badge ? `
          <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-slate-900 text-white tracking-wide uppercase">
            ${escapeHtml(product.badge)}
          </span>
        ` : ''}
      </div>

      <!-- Product Image -->
      <a href="#/products/${escapeHtml(product.slug)}" class="block pt-8 pb-4 px-4 bg-slate-50/50 group-hover:bg-slate-50 transition aspect-square relative flex items-center justify-center overflow-hidden">
        <img 
          src="${escapeHtml(product.imageUrl || '/assets/medicine-placeholder.png')}" 
          alt="${escapeHtml(product.name)}" 
          loading="lazy"
          class="max-h-36 max-w-full object-contain group-hover:scale-105 transition-transform duration-200"
          onerror="this.src='data:image/svg+xml;utf8,<svg xmlns=\\'http://www.w3.org/2000/svg\\' width=\\'64\\' height=\\'64\\' viewBox=\\'0 0 24 24\\' fill=\\'none\\' stroke=\\'%2394a3b8\\' stroke-width=\\'1.5\\'><path d=\\'m10.5 20.5 10-10a4.95 4.95 0 1 0-7-7l-10 10a4.95 4.95 0 1 0 7 7Z\\'/><path d=\\'m8.5 8.5 7 7\\'/></svg>'"
        />
      </a>

      <!-- Content -->
      <div class="p-3 sm:p-4">
        <!-- Manufacturer & Category -->
        <div class="flex items-center justify-between text-[10px] sm:text-[11px] text-slate-500 mb-1">
          <span class="truncate max-w-[100px] sm:max-w-[120px] font-medium">${escapeHtml(product.manufacturerName || product.genericName || 'Medicine')}</span>
          <span class="text-slate-400 truncate">${escapeHtml(product.dosageForm || '')}</span>
        </div>

        <!-- Title -->
        <a href="#/products/${escapeHtml(product.slug)}" class="block font-bold text-xs sm:text-sm text-slate-900 group-hover:text-brand-600 transition leading-snug line-clamp-2 min-h-[2.25rem] sm:min-h-[2.5rem]">
          ${escapeHtml(product.name)}
        </a>

        <!-- Strength / Pack size -->
        <div class="text-[10px] sm:text-[11px] text-slate-500 mt-1 truncate">
          ${escapeHtml(product.strength || '')} ${product.packSize ? `• ${escapeHtml(product.packSize)}` : ''}
        </div>
      </div>
    </div>

    <!-- Pricing & Add to Cart -->
    <div class="p-3 sm:p-4 pt-0 mt-auto">
      <div class="flex items-baseline gap-1.5 sm:gap-2 mb-2.5 sm:mb-3">
        <span class="text-sm sm:text-base font-extrabold text-slate-900">${formatCurrency(currentPrice)}</span>
        ${hasDiscount ? `
          <span class="text-[11px] sm:text-xs text-slate-400 line-through">${formatCurrency(product.basePrice)}</span>
        ` : ''}
      </div>

      <button 
        type="button" 
        class="add-to-cart-btn w-full btn-primary text-xs py-2 sm:py-2.5 min-h-[38px] ${isOutOfStock ? 'opacity-50 cursor-not-allowed' : ''}" 
        data-product-id="${product.id}"
        ${isOutOfStock ? 'disabled' : ''}
      >
        <i data-lucide="${isOutOfStock ? 'slash' : 'shopping-bag'}" class="w-3.5 h-3.5"></i>
        <span>${isOutOfStock ? 'Out of Stock' : 'Add to Cart'}</span>
      </button>
    </div>
  `;

  // Prevent multiple fast clicks
  const btn = card.querySelector('.add-to-cart-btn');
  btn?.addEventListener('click', async (e) => {
    e.stopPropagation();
    if (isOutOfStock) return;
    
    btn.disabled = true;
    const originalText = btn.innerHTML;
    btn.innerHTML = `<i data-lucide="loader" class="w-3.5 h-3.5 animate-spin"></i> Adding...`;
    renderIcons(btn);

    try {
      await cartStore.addItem(product.id, 1);
      btn.innerHTML = `<i data-lucide="check" class="w-3.5 h-3.5"></i> Added!`;
      renderIcons(btn);
      setTimeout(() => {
        btn.innerHTML = originalText;
        btn.disabled = false;
        renderIcons(btn);
      }, 1500);
    } catch (err) {
      btn.innerHTML = originalText;
      btn.disabled = false;
      renderIcons(btn);
    }
  });

  renderIcons(card);
  return card;
}
