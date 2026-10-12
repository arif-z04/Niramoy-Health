import { authStore } from '../state/authStore.js';
import { supportApi } from '../api/admin.js';
import { toast } from '../state/toastStore.js';
import { escapeHtml, formatDate, getStatusBadgeClass } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export async function renderSupportPage() {
  const container = document.createElement('div');
  container.className = 'max-w-4xl mx-auto px-4 sm:px-6 py-8 space-y-8';

  const authState = authStore.getState();

  container.innerHTML = `
    <!-- Header -->
    <div class="border-b border-slate-200/80 pb-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-900">Help &amp; Customer Support</h1>
        <p class="text-xs text-slate-500">Reach our pharmacy support desk or open an assistance ticket</p>
      </div>
      <div class="flex items-center gap-3 text-xs">
        <span class="px-3 py-1.5 rounded-xl bg-brand-50 text-brand-700 font-bold border border-brand-200">
          Hotline: 16247
        </span>
      </div>
    </div>

    <!-- Contact Info Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="bg-white rounded-2xl border border-slate-200/80 p-4 shadow-sm flex items-start gap-3">
        <div class="w-10 h-10 rounded-xl bg-brand-50 text-brand-600 flex items-center justify-center flex-shrink-0">
          <i data-lucide="phone" class="w-5 h-5"></i>
        </div>
        <div>
          <h4 class="text-xs font-bold text-slate-900">Emergency Phone</h4>
          <p class="text-[11px] text-slate-500 mt-0.5">16247 or 09612-000000</p>
          <p class="text-[10px] text-slate-400">9 AM – 11 PM Everyday</p>
        </div>
      </div>

      <div class="bg-white rounded-2xl border border-slate-200/80 p-4 shadow-sm flex items-start gap-3">
        <div class="w-10 h-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center flex-shrink-0">
          <i data-lucide="mail" class="w-5 h-5"></i>
        </div>
        <div>
          <h4 class="text-xs font-bold text-slate-900">Email Inquiries</h4>
          <p class="text-[11px] text-slate-500 mt-0.5">support@niramoyhealth.com</p>
          <p class="text-[10px] text-slate-400">Response within 2 hours</p>
        </div>
      </div>

      <div class="bg-white rounded-2xl border border-slate-200/80 p-4 shadow-sm flex items-start gap-3">
        <div class="w-10 h-10 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center flex-shrink-0">
          <i data-lucide="map-pin" class="w-5 h-5"></i>
        </div>
        <div>
          <h4 class="text-xs font-bold text-slate-900">Pharmacy Hub</h4>
          <p class="text-[11px] text-slate-500 mt-0.5">Gulshan-2, Dhaka-1212</p>
          <p class="text-[10px] text-slate-400">Central Distribution Facility</p>
        </div>
      </div>
    </div>

    <!-- Create Ticket Form -->
    <div class="bg-white rounded-3xl border border-slate-200/80 p-6 sm:p-8 shadow-card space-y-4">
      <h3 class="text-sm font-bold text-slate-900 flex items-center gap-2 border-b border-slate-100 pb-3">
        <i data-lucide="message-square-plus" class="w-4 h-4 text-brand-600"></i>
        Open a Support Ticket
      </h3>

      ${authState.isAuthenticated ? `
        <form id="new-ticket-form" class="space-y-4">
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label class="form-label font-semibold">Subject *</label>
              <input type="text" id="ticket-subject" required placeholder="e.g. Question regarding Napa Extra batch" class="form-input"/>
            </div>

            <div>
              <label class="form-label font-semibold">Priority</label>
              <select id="ticket-priority" class="form-input">
                <option value="low">Low - General inquiry</option>
                <option value="medium" selected>Medium - Order assistance</option>
                <option value="high">High - Urgent medicine query</option>
                <option value="urgent">Urgent - Adverse effect</option>
              </select>
            </div>
          </div>

          <div>
            <label class="form-label font-semibold">Message / Explanation *</label>
            <textarea id="ticket-message" required rows="4" placeholder="Please describe how our clinical or support team can help you..." class="form-input"></textarea>
          </div>

          <button type="submit" id="submit-ticket-btn" class="btn-primary text-xs py-2.5 px-6 font-bold">
            Submit Support Request &rarr;
          </button>
        </form>
      ` : `
        <div class="p-6 text-center bg-slate-50 rounded-2xl border border-slate-200 space-y-3">
          <p class="text-xs text-slate-600">Please sign in to track and submit support tickets directly from your account.</p>
          <a href="#/login?redirect=support" class="btn-primary text-xs py-2 px-5 inline-flex">Sign In</a>
        </div>
      `}
    </div>

    <!-- My Tickets List (if authenticated) -->
    ${authState.isAuthenticated ? `
      <div class="bg-white rounded-3xl border border-slate-200/80 p-6 sm:p-8 shadow-card space-y-4">
        <h3 class="text-sm font-bold text-slate-900 border-b border-slate-100 pb-3">Your Support Tickets</h3>
        <div id="support-tickets-list" class="space-y-3">
          <p class="text-xs text-slate-400 py-3 text-center">Loading tickets...</p>
        </div>
      </div>
    ` : ''}
  `;

  renderIcons(container);

  if (authState.isAuthenticated) {
    setupSupportEvents(container);
  }

  return container;
}

async function setupSupportEvents(container) {
  const form = container.querySelector('#new-ticket-form');
  const submitBtn = container.querySelector('#submit-ticket-btn');

  form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const subject = container.querySelector('#ticket-subject').value.trim();
    const priority = container.querySelector('#ticket-priority').value;
    const message = container.querySelector('#ticket-message').value.trim();

    submitBtn.disabled = true;
    submitBtn.innerHTML = `Submitting...`;

    try {
      await supportApi.createTicket(subject, message, priority);
      toast.success('Support ticket opened! Our team will respond shortly.');
      form.reset();
      loadUserTickets(container);
    } catch (err) {
      toast.error(err.message || 'Failed to submit ticket');
    } finally {
      submitBtn.disabled = false;
      submitBtn.innerHTML = `Submit Support Request &rarr;`;
    }
  });

  loadUserTickets(container);
}

async function loadUserTickets(container) {
  const listEl = container.querySelector('#support-tickets-list');
  if (!listEl) return;

  try {
    const data = await supportApi.getTickets();
    const tickets = data?.items || [];

    if (tickets.length === 0) {
      listEl.innerHTML = `<p class="text-xs text-slate-400 text-center py-4">No active support tickets found.</p>`;
    } else {
      listEl.innerHTML = tickets.map(t => {
        const badge = getStatusBadgeClass(t.status);
        return `
          <div class="p-4 rounded-2xl border border-slate-200 bg-slate-50/40 space-y-2 text-xs">
            <div class="flex items-center justify-between">
              <span class="font-bold text-slate-900">${escapeHtml(t.subject)}</span>
              <span class="px-2 py-0.5 rounded-full text-[10px] font-bold border ${badge} uppercase">
                ${escapeHtml(t.status)}
              </span>
            </div>
            <div class="text-slate-500 text-[11px]">Ticket #${t.id} • Opened ${formatDate(t.createdAt)} • Priority: ${escapeHtml(t.priority)}</div>
            ${t.lastMessage ? `
              <p class="text-slate-700 bg-white p-2.5 rounded-xl border border-slate-200/60 mt-1">${escapeHtml(t.lastMessage)}</p>
            ` : ''}
          </div>
        `;
      }).join('');
    }

    renderIcons(listEl);
  } catch (err) {
    listEl.innerHTML = `<p class="text-xs text-rose-600">Failed to load tickets: ${err.message}</p>`;
  }
}
