import { toast } from '../state/toastStore.js';
import { escapeHtml } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export function createToastContainer() {
  const container = document.createElement('div');
  container.id = 'toast-container';
  container.className = 'fixed top-4 right-4 z-50 flex flex-col gap-2 max-w-sm w-full pointer-events-none px-4 sm:px-0';

  toast.subscribe(toasts => {
    container.innerHTML = toasts.map(t => {
      let icon = 'info';
      let borderClass = 'border-slate-200 bg-white text-slate-800';
      if (t.type === 'success') {
        icon = 'check-circle-2';
        borderClass = 'border-emerald-200 bg-emerald-50 text-emerald-900';
      } else if (t.type === 'error') {
        icon = 'alert-triangle';
        borderClass = 'border-rose-200 bg-rose-50 text-rose-900';
      } else if (t.type === 'warning') {
        icon = 'alert-circle';
        borderClass = 'border-amber-200 bg-amber-50 text-amber-900';
      }

      return `
        <div class="pointer-events-auto p-3.5 rounded-xl border shadow-lg flex items-start gap-3 animate-slide-down ${borderClass}" data-toast-id="${t.id}">
          <i data-lucide="${icon}" class="w-5 h-5 flex-shrink-0 mt-0.5"></i>
          <div class="flex-1 text-xs font-medium leading-relaxed">${escapeHtml(t.message)}</div>
          <button type="button" class="toast-close-btn p-0.5 text-slate-400 hover:text-slate-700 transition" data-toast-id="${t.id}">
            <i data-lucide="x" class="w-4 h-4"></i>
          </button>
        </div>
      `;
    }).join('');

    container.querySelectorAll('.toast-close-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        toast.remove(btn.dataset.toastId);
      });
    });

    renderIcons(container);
  });

  return container;
}
