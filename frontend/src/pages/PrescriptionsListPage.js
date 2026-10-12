import { authStore } from '../state/authStore.js';
import { prescriptionsApi } from '../api/prescriptions.js';
import { escapeHtml, formatDate, getStatusBadgeClass } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export async function renderPrescriptionsListPage() {
  const container = document.createElement('div');
  container.className = 'max-w-5xl mx-auto px-4 sm:px-6 py-8 space-y-6';

  const authState = authStore.getState();
  if (!authState.isAuthenticated) {
    container.innerHTML = `
      <div class="bg-white rounded-3xl border border-slate-200/80 p-8 text-center shadow-card space-y-4">
        <h2 class="text-xl font-bold text-slate-900">Sign in to view your prescriptions</h2>
        <a href="#/login?redirect=prescriptions" class="btn-primary text-xs py-2 px-5 inline-flex">Sign In</a>
      </div>
    `;
    return container;
  }

  container.innerHTML = `
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-200/80 pb-4">
      <div>
        <h1 class="text-xl sm:text-2xl font-bold text-slate-900">My Prescriptions</h1>
        <p class="text-xs text-slate-500">Track verification status and clinical notes from licensed pharmacists</p>
      </div>
      <a href="#/prescriptions/upload" class="btn-primary text-xs py-2.5 px-4 font-bold shadow-sm">
        <i data-lucide="file-up" class="w-4 h-4"></i>
        <span>Upload New Prescription</span>
      </a>
    </div>

    <!-- Prescriptions List Container -->
    <div id="prescriptions-list-container" class="space-y-4">
      <div class="p-8 text-center text-xs text-slate-400">Loading prescriptions...</div>
    </div>
  `;

  renderIcons(container);

  loadPrescriptions(container);

  return container;
}

async function loadPrescriptions(container) {
  const listEl = container.querySelector('#prescriptions-list-container');
  try {
    const data = await prescriptionsApi.getMyPrescriptions();
    const prescriptions = data?.items || [];

    if (prescriptions.length === 0) {
      listEl.innerHTML = `
        <div class="p-12 text-center bg-white rounded-3xl border border-slate-200/80 shadow-sm space-y-4">
          <div class="w-14 h-14 rounded-2xl bg-brand-50 text-brand-600 flex items-center justify-center mx-auto">
            <i data-lucide="file-text" class="w-7 h-7"></i>
          </div>
          <div>
            <h3 class="text-sm font-bold text-slate-800">No Prescriptions Uploaded Yet</h3>
            <p class="text-xs text-slate-500 mt-1 max-w-sm mx-auto">Upload a prescription to order prescription medicines and receive clinical pharmacist advice.</p>
          </div>
          <a href="#/prescriptions/upload" class="btn-primary text-xs py-2 px-5 inline-flex font-bold">
            Upload Prescription &rarr;
          </a>
        </div>
      `;
    } else {
      listEl.innerHTML = prescriptions.map(rx => {
        const statusBadge = getStatusBadgeClass(rx.status);
        let statusTitle = rx.status?.toUpperCase();
        if (rx.status === 'submitted') statusTitle = 'Submitted (Pending Review)';
        else if (rx.status === 'under_review') statusTitle = 'Under Pharmacist Review';
        else if (rx.status === 'approved') statusTitle = 'Approved by Pharmacist';
        else if (rx.status === 'rejected') statusTitle = 'Rejected';

        return `
          <div class="bg-white rounded-2xl border border-slate-200/80 p-5 shadow-sm space-y-3">
            <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2 border-b border-slate-100 pb-3">
              <div class="flex items-center gap-2">
                <span class="w-2 h-2 rounded-full ${rx.status === 'approved' ? 'bg-emerald-500' : (rx.status === 'rejected' ? 'bg-rose-500' : 'bg-amber-500')}"></span>
                <span class="text-xs font-bold text-slate-900">Rx #${rx.id}</span>
                <span class="text-[11px] text-slate-400">• Uploaded on ${formatDate(rx.createdAt)}</span>
              </div>
              <span class="px-2.5 py-0.5 rounded-full text-xs font-bold border ${statusBadge} self-start sm:self-auto">
                ${escapeHtml(statusTitle)}
              </span>
            </div>

            <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 text-xs text-slate-600">
              <div>
                <span class="text-[10px] text-slate-400 block uppercase">Patient</span>
                <strong class="text-slate-900">${escapeHtml(rx.patientName || 'N/A')} (${escapeHtml(rx.patientGender || '')}, ${rx.patientAge || 'N/A'}y)</strong>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block uppercase">Doctor / Clinic</span>
                <span class="text-slate-800">${escapeHtml(rx.doctorName || 'Not specified')}</span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block uppercase">Phone</span>
                <span class="text-slate-800">${escapeHtml(rx.patientPhone || 'N/A')}</span>
              </div>
              <div>
                <span class="text-[10px] text-slate-400 block uppercase">Reviewed By</span>
                <span class="text-slate-800">${rx.reviewedBy ? `Pharmacist #${rx.reviewedBy}` : 'Pending assignment'}</span>
              </div>
            </div>

            ${rx.clinicalNotes ? `
              <div class="p-3 bg-slate-50 border border-slate-200/70 rounded-xl text-xs text-slate-700">
                <strong class="font-bold text-slate-900">Pharmacist Notes:</strong> ${escapeHtml(rx.clinicalNotes)}
              </div>
            ` : ''}

            ${rx.status === 'approved' ? `
              <div class="flex items-center justify-between pt-1">
                <span class="text-xs text-emerald-700 font-semibold flex items-center gap-1">
                  <i data-lucide="check" class="w-3.5 h-3.5"></i> Ready to attach during checkout!
                </span>
                <a href="#/products" class="btn-primary text-xs py-1.5 px-3">
                  Shop Medicines &rarr;
                </a>
              </div>
            ` : ''}
          </div>
        `;
      }).join('');
    }

    renderIcons(listEl);
  } catch (err) {
    listEl.innerHTML = `
      <div class="p-6 bg-rose-50 border border-rose-200 rounded-2xl text-rose-800 text-xs">
        Failed to load prescriptions: ${escapeHtml(err.message)}
      </div>
    `;
  }
}
