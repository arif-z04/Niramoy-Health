import { authStore } from '../state/authStore.js';
import { toast } from '../state/toastStore.js';
import { renderIcons } from '../utils/icons.js';

export function renderRegisterPage() {
  const container = document.createElement('div');
  container.className = 'max-w-md mx-auto px-4 py-12 space-y-6';

  container.innerHTML = `
    <div class="bg-white rounded-3xl border border-slate-200/80 p-6 sm:p-8 shadow-card space-y-6">
      
      <!-- Brand & Title -->
      <div class="text-center space-y-2">
        <div class="w-12 h-12 rounded-2xl bg-gradient-to-tr from-brand-700 to-brand-500 text-white flex items-center justify-center mx-auto shadow-md shadow-brand-500/20">
          <i data-lucide="cross" class="w-7 h-7 stroke-[2.5]"></i>
        </div>
        <h1 class="text-2xl font-extrabold text-slate-900 tracking-tight">Create Your Account</h1>
        <p class="text-xs text-slate-500">Join Niramoy Health for genuine healthcare delivery</p>
      </div>

      <!-- Error Alert Container -->
      <div id="register-error-alert" class="hidden p-3.5 bg-rose-50 border border-rose-200 rounded-xl text-xs text-rose-800 flex items-start gap-2">
        <i data-lucide="alert-circle" class="w-4 h-4 text-rose-600 flex-shrink-0 mt-0.5"></i>
        <div id="register-error-text">Registration failed.</div>
      </div>

      <!-- Form -->
      <form id="register-form" class="space-y-4">
        <div>
          <label for="reg-name" class="form-label font-semibold">Full Name *</label>
          <input 
            type="text" 
            id="reg-name" 
            required 
            placeholder="Mohammad Rahim" 
            class="form-input"
          />
        </div>

        <div>
          <label for="reg-phone" class="form-label font-semibold">Bangladeshi Mobile Number *</label>
          <div class="relative">
            <span class="absolute left-3.5 top-2.5 text-xs font-bold text-slate-400 select-none">+88</span>
            <input 
              type="tel" 
              id="reg-phone" 
              required 
              placeholder="017XXXXXXXX" 
              class="form-input pl-12"
            />
          </div>
          <p class="text-[10px] text-slate-400 mt-1">Must be an 11-digit active Bangladeshi number</p>
        </div>

        <div>
          <label for="reg-email" class="form-label font-semibold">Email Address (Optional)</label>
          <input 
            type="email" 
            id="reg-email" 
            placeholder="rahim@example.com" 
            class="form-input"
          />
        </div>

        <div>
          <label for="reg-password" class="form-label font-semibold">Password *</label>
          <input 
            type="password" 
            id="reg-password" 
            required 
            placeholder="At least 8 chars with uppercase & symbol" 
            class="form-input"
          />
          <p class="text-[10px] text-slate-400 mt-1">Min 8 characters, 1 uppercase, 1 lowercase, 1 number, 1 symbol</p>
        </div>

        <div class="flex items-start gap-2 pt-1 text-xs text-slate-600">
          <input type="checkbox" id="reg-terms" required class="rounded text-brand-600 focus:ring-brand-500 mt-0.5"/>
          <label for="reg-terms">
            I agree to the <a href="#/terms" class="text-brand-600 underline">Terms of Service</a> and <a href="#/privacy-policy" class="text-brand-600 underline">Privacy Policy</a>.
          </label>
        </div>

        <button type="submit" id="register-submit-btn" class="btn-primary w-full py-3 font-bold text-sm shadow-md shadow-brand-600/20">
          <span>Create Account &rarr;</span>
        </button>
      </form>

      <!-- Footer Links -->
      <div class="text-center text-xs text-slate-500 pt-2 border-t border-slate-100">
        Already have an account? 
        <a href="#/login" class="font-bold text-brand-600 hover:text-brand-700 underline ml-1">
          Sign In
        </a>
      </div>

    </div>
  `;

  renderIcons(container);

  const form = container.querySelector('#register-form');
  const submitBtn = container.querySelector('#register-submit-btn');
  const errorAlert = container.querySelector('#register-error-alert');
  const errorText = container.querySelector('#register-error-text');

  form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    errorAlert.classList.add('hidden');

    const fullName = container.querySelector('#reg-name').value.trim();
    let phoneNumber = container.querySelector('#reg-phone').value.trim();
    const email = container.querySelector('#reg-email').value.trim();
    const password = container.querySelector('#reg-password').value;

    if (!phoneNumber.startsWith('+880') && phoneNumber.startsWith('01')) {
      phoneNumber = '+88' + phoneNumber;
    }

    submitBtn.disabled = true;
    submitBtn.innerHTML = `<i data-lucide="loader" class="w-4 h-4 animate-spin"></i> Creating account...`;
    renderIcons(submitBtn);

    try {
      await authStore.register({
        fullName,
        phoneNumber,
        email: email || undefined,
        password,
      });

      toast.success('Account created successfully! Welcome to Niramoy Health.');
      window.location.hash = '#/';
    } catch (err) {
      let msg = err.message || 'Registration failed.';
      if (err.errors) {
        msg = Object.values(err.errors).flat().join('<br/>');
      }
      errorText.innerHTML = msg;
      errorAlert.classList.remove('hidden');
      renderIcons(errorAlert);

      submitBtn.disabled = false;
      submitBtn.innerHTML = `<span>Create Account &rarr;</span>`;
      renderIcons(submitBtn);
    }
  });

  return container;
}
