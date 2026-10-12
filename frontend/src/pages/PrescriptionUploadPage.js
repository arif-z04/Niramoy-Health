import { authStore } from '../state/authStore.js';
import { prescriptionsApi } from '../api/prescriptions.js';
import { toast } from '../state/toastStore.js';
import { escapeHtml } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export function renderPrescriptionUploadPage() {
  const container = document.createElement('div');
  container.className = 'max-w-3xl mx-auto px-4 sm:px-6 py-10 space-y-8';

  const authState = authStore.getState();
  if (!authState.isAuthenticated) {
    container.innerHTML = `
      <div class="bg-white rounded-3xl border border-slate-200/80 p-8 sm:p-12 text-center shadow-card space-y-5">
        <div class="w-16 h-16 rounded-2xl bg-brand-50 text-brand-600 flex items-center justify-center mx-auto">
          <i data-lucide="lock" class="w-8 h-8"></i>
        </div>
        <div>
          <h2 class="text-xl font-bold text-slate-900">Sign in to Upload Prescription</h2>
          <p class="text-xs sm:text-sm text-slate-500 max-w-md mx-auto mt-1 leading-relaxed">
            Prescriptions are sensitive medical documents linked to your personal patient account so our licensed pharmacists can review and assist you safely.
          </p>
        </div>
        <div class="flex items-center justify-center gap-3 pt-2">
          <a href="#/login?redirect=prescriptions/upload" class="btn-primary text-xs py-2.5 px-6 font-bold">
            Sign In to Continue &rarr;
          </a>
          <a href="#/register" class="btn-secondary text-xs py-2.5 px-5">
            Create Account
          </a>
        </div>
      </div>
    `;
    renderIcons(container);
    return container;
  }

  container.innerHTML = `
    <!-- Header -->
    <div class="space-y-2 text-center max-w-lg mx-auto">
      <div class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-brand-50 text-brand-700 text-xs font-semibold">
        <i data-lucide="shield-check" class="w-3.5 h-3.5"></i>
        BMDC Registered Pharmacist Review
      </div>
      <h1 class="text-2xl sm:text-3xl font-extrabold text-slate-900">Upload Your Prescription</h1>
      <p class="text-xs sm:text-sm text-slate-500 leading-relaxed">
        Upload a clear photograph or scanned PDF of your doctor's prescription. Our clinical pharmacy team will review it and prepare your order.
      </p>
    </div>

    <!-- Upload Card -->
    <div class="bg-white rounded-3xl border border-slate-200/80 p-6 sm:p-10 shadow-card">
      <form id="prescription-upload-form" class="space-y-6">
        
        <!-- File Picker Drag & Drop Zone -->
        <div class="space-y-2">
          <label class="form-label font-bold text-slate-900">Prescription File (Image or PDF) *</label>
          <div 
            id="drop-zone"
            class="border-2 border-dashed border-slate-300 hover:border-brand-500 rounded-2xl p-6 sm:p-8 text-center bg-slate-50/50 hover:bg-brand-50/30 transition cursor-pointer relative"
          >
            <input 
              type="file" 
              id="prescription-file-input" 
              name="file" 
              accept=".jpg,.jpeg,.png,.pdf" 
              class="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
            />
            
            <div id="drop-zone-prompt" class="space-y-3">
              <div class="w-14 h-14 rounded-2xl bg-white border border-slate-200 text-brand-600 flex items-center justify-center mx-auto shadow-sm">
                <i data-lucide="upload-cloud" class="w-7 h-7"></i>
              </div>
              <div>
                <p class="text-xs sm:text-sm font-semibold text-slate-800">
                  <span class="text-brand-600 underline">Browse file</span> or drag and drop here
                </p>
                <p class="text-[11px] text-slate-400 mt-1">Supports JPG, PNG, or PDF (Max file size: 10 MB)</p>
              </div>
            </div>

            <!-- Preview Container -->
            <div id="file-preview-container" class="hidden space-y-3">
              <div class="inline-flex items-center gap-2 p-2.5 px-4 rounded-xl bg-white border border-slate-200 shadow-sm text-xs font-semibold text-slate-800">
                <i data-lucide="file-check" class="w-4 h-4 text-brand-600"></i>
                <span id="preview-filename" class="truncate max-w-xs">prescription.jpg</span>
                <button type="button" id="remove-file-btn" class="ml-2 text-slate-400 hover:text-rose-600 p-0.5">
                  <i data-lucide="x" class="w-3.5 h-3.5"></i>
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Patient Demographics Form -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label for="patient-name" class="form-label font-semibold">Patient Full Name *</label>
            <input 
              type="text" 
              id="patient-name" 
              required 
              placeholder="e.g. Mohammad Rahim" 
              value="${escapeHtml(authState.user?.fullName || '')}"
              class="form-input"
            />
          </div>

          <div>
            <label for="patient-phone" class="form-label font-semibold">Patient Phone Number *</label>
            <input 
              type="tel" 
              id="patient-phone" 
              required 
              placeholder="017XXXXXXXX" 
              value="${escapeHtml(authState.user?.phoneNumber || '')}"
              class="form-input"
            />
          </div>

          <div>
            <label for="patient-age" class="form-label font-semibold">Patient Age (Years) *</label>
            <input 
              type="number" 
              id="patient-age" 
              required 
              min="1" 
              max="120" 
              placeholder="e.g. 35" 
              class="form-input"
            />
          </div>

          <div>
            <label for="patient-gender" class="form-label font-semibold">Patient Gender *</label>
            <select id="patient-gender" required class="form-input">
              <option value="male">Male (পুরুষ)</option>
              <option value="female">Female (মহিলা)</option>
              <option value="other">Other</option>
            </select>
          </div>

          <div class="sm:col-span-2">
            <label for="doctor-name" class="form-label font-semibold">Doctor or Clinic / Hospital Name (Optional)</label>
            <input 
              type="text" 
              id="doctor-name" 
              placeholder="e.g. Dr. Kabir Ahmed, Square Hospital" 
              class="form-input"
            />
          </div>

          <div class="sm:col-span-2">
            <label for="prescription-notes" class="form-label font-semibold">Notes / Special Instructions (Optional)</label>
            <textarea 
              id="prescription-notes" 
              rows="2" 
              placeholder="Any specific brands, tablet counts, or chronic dose duration instructions..." 
              class="form-input"
            ></textarea>
          </div>
        </div>

        <!-- Guidance Alert -->
        <div class="p-3.5 bg-slate-50 border border-slate-200/80 rounded-xl text-xs text-slate-600 flex items-start gap-2.5">
          <i data-lucide="info" class="w-4 h-4 text-brand-600 flex-shrink-0 mt-0.5"></i>
          <div>
            <p class="font-semibold text-slate-800">What happens after upload?</p>
            <p class="text-[11px] text-slate-500 mt-0.5 leading-relaxed">
              Your prescription will be reviewed by our licensed clinical pharmacists. You will receive real-time updates under <a href="#/prescriptions" class="text-brand-600 underline font-semibold">My Prescriptions</a>, where you can complete checkout once approved.
            </p>
          </div>
        </div>

        <!-- Submit Button -->
        <button type="submit" id="submit-upload-btn" class="btn-primary w-full py-3.5 font-bold text-sm shadow-md shadow-brand-600/20">
          <i data-lucide="send" class="w-4 h-4"></i>
          <span>Submit Prescription for Review</span>
        </button>

      </form>
    </div>
  `;

  renderIcons(container);
  setupUploadEvents(container);

  return container;
}

function setupUploadEvents(container) {
  const fileInput = container.querySelector('#prescription-file-input');
  const dropPrompt = container.querySelector('#drop-zone-prompt');
  const previewContainer = container.querySelector('#file-preview-container');
  const previewFilename = container.querySelector('#preview-filename');
  const removeBtn = container.querySelector('#remove-file-btn');
  const form = container.querySelector('#prescription-upload-form');
  const submitBtn = container.querySelector('#submit-upload-btn');

  let selectedFile = null;

  fileInput?.addEventListener('change', (e) => {
    if (e.target.files && e.target.files[0]) {
      selectedFile = e.target.files[0];
      if (selectedFile.size > 10 * 1024 * 1024) {
        toast.error('File size exceeds the 10 MB limit.');
        fileInput.value = '';
        selectedFile = null;
        return;
      }
      previewFilename.textContent = selectedFile.name;
      dropPrompt.classList.add('hidden');
      previewContainer.classList.remove('hidden');
      renderIcons(previewContainer);
    }
  });

  removeBtn?.addEventListener('click', (e) => {
    e.stopPropagation();
    fileInput.value = '';
    selectedFile = null;
    dropPrompt.classList.remove('hidden');
    previewContainer.classList.add('hidden');
  });

  form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    if (!selectedFile) {
      toast.error('Please choose a prescription file (image or PDF) to upload.');
      return;
    }

    const patientName = container.querySelector('#patient-name').value.trim();
    const patientPhone = container.querySelector('#patient-phone').value.trim();
    const patientAge = container.querySelector('#patient-age').value.trim();
    const patientGender = container.querySelector('#patient-gender').value;
    const doctorName = container.querySelector('#doctor-name').value.trim();
    const notes = container.querySelector('#prescription-notes').value.trim();

    if (!patientName || !patientPhone || !patientAge) {
      toast.error('Please fill in all required patient demographic fields.');
      return;
    }

    const formData = new FormData();
    formData.append('file', selectedFile);
    formData.append('files', selectedFile);
    formData.append('patientName', patientName);
    formData.append('patientPhone', patientPhone);
    formData.append('patientAge', patientAge);
    formData.append('patientGender', patientGender);
    if (doctorName) formData.append('doctorName', doctorName);
    if (notes) {
      formData.append('notes', notes);
      formData.append('customerNotes', notes);
    }

    submitBtn.disabled = true;
    const origHtml = submitBtn.innerHTML;
    submitBtn.innerHTML = `<i data-lucide="loader" class="w-4 h-4 animate-spin"></i> Submitting...`;
    renderIcons(submitBtn);

    try {
      const res = await prescriptionsApi.uploadPrescription(formData);
      toast.success('Prescription uploaded successfully! Status: Submitted for review.');
      window.location.hash = '#/prescriptions';
    } catch (err) {
      toast.error(err.message || 'Failed to upload prescription');
      submitBtn.disabled = false;
      submitBtn.innerHTML = origHtml;
      renderIcons(submitBtn);
    }
  });
}
