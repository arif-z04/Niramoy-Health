import { authStore } from '../state/authStore.js';
import { toast } from '../state/toastStore.js';
import { escapeHtml } from '../utils/formatters.js';
import { renderIcons } from '../utils/icons.js';

export function renderLoginPage(redirectTarget = null) {
  const container = document.createElement('div');
  container.className = 'max-w-md mx-auto px-4 py-12 space-y-6';

  container.innerHTML = `
    <div class="bg-white rounded-3xl border border-slate-200/80 p-6 sm:p-8 shadow-card space-y-6">
      
      <!-- Brand & Title -->
      <div class="text-center space-y-2">
        <div class="w-12 h-12 rounded-2xl bg-gradient-to-tr from-brand-700 to-brand-500 text-white flex items-center justify-center mx-auto shadow-md shadow-brand-500/20">
          <i data-lucide="cross" class="w-7 h-7 stroke-[2.5]"></i>
        </div>
        <h1 class="text-2xl font-extrabold text-slate-900 tracking-tight">Welcome Back</h1>
        <p class="text-xs text-slate-500">Sign in to manage your healthcare orders and prescriptions</p>
      </div>

      <!-- Error Alert Container -->
      <div id="login-error-alert" class="hidden p-3.5 bg-rose-50 border border-rose-200 rounded-xl text-xs text-rose-800 flex items-start gap-2">
        <i data-lucide="alert-circle" class="w-4 h-4 text-rose-600 flex-shrink-0 mt-0.5"></i>
        <span id="login-error-text">Invalid login credentials.</span>
      </div>

      <!-- Form -->
      <form id="login-form" class="space-y-4">
        <div>
          <label for="login-identifier" class="form-label font-semibold">Mobile Number or Email</label>
          <div class="relative">
            <input 
              type="text" 
              id="login-identifier" 
              required 
              autocomplete="username"
              placeholder="017XXXXXXXX or user@example.com" 
              class="form-input pl-10"
            />
            <i data-lucide="user" class="w-4 h-4 text-slate-400 absolute left-3.5 top-3 pointer-events-none"></i>
          </div>
        </div>

        <div>
          <div class="flex items-center justify-between mb-1.5">
            <label for="login-password" class="form-label font-semibold mb-0">Password</label>
          </div>
          <div class="relative">
            <input 
              type="password" 
              id="login-password" 
              required 
              autocomplete="current-password"
              placeholder="••••••••" 
              class="form-input pl-10 pr-10"
            />
            <i data-lucide="lock" class="w-4 h-4 text-slate-400 absolute left-3.5 top-3 pointer-events-none"></i>
            <button type="button" id="toggle-password-btn" class="absolute right-3.5 top-3 text-slate-400 hover:text-slate-600">
              <i data-lucide="eye" class="w-4 h-4"></i>
            </button>
          </div>
        </div>

        <button type="submit" id="login-submit-btn" class="btn-primary w-full py-3 font-bold text-sm shadow-md shadow-brand-600/20">
          <span>Sign In &rarr;</span>
        </button>
      </form>

      <!-- Demo Accounts Info Box -->
      <div class="p-3 bg-slate-50 border border-slate-200/80 rounded-xl text-[11px] text-slate-600 space-y-1">
        <div class="font-bold text-slate-800 flex items-center gap-1">
          <i data-lucide="key" class="w-3.5 h-3.5 text-brand-600"></i> Demo Accounts Available:
        </div>
        <p>• Customer: <span class="font-mono text-slate-900">01712345678</span> / <span class="font-mono text-slate-900">Password123!</span></p>
        <p>• Admin: <span class="font-mono text-slate-900">admin@niramoyhealth.com</span> / <span class="font-mono text-slate-900">Admin123!</span></p>
      </div>

      <!-- Footer Links -->
      <div class="text-center text-xs text-slate-500 pt-2 border-t border-slate-100">
        Don't have an account yet? 
        <a href="#/register" class="font-bold text-brand-600 hover:text-brand-700 underline ml-1">
          Create Account
        </a>
      </div>

    </div>
  `;

  renderIcons(container);

  // Toggle password visibility
  const pwdInput = container.querySelector('#login-password');
  const toggleBtn = container.querySelector('#toggle-password-btn');
  toggleBtn?.addEventListener('click', () => {
    if (pwdInput.type === 'password') {
      pwdInput.type = 'text';
      toggleBtn.innerHTML = `<i data-lucide="eye-off" class="w-4 h-4"></i>`;
    } else {
      pwdInput.type = 'password';
      toggleBtn.innerHTML = `<i data-lucide="eye" class="w-4 h-4"></i>`;
    }
    renderIcons(toggleBtn);
  });

  // Handle submit
  const form = container.querySelector('#login-form');
  const submitBtn = container.querySelector('#login-submit-btn');
  const errorAlert = container.querySelector('#login-error-alert');
  const errorText = container.querySelector('#login-error-text');

  form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    errorAlert.classList.add('hidden');

    const identifier = container.querySelector('#login-identifier').value.trim();
    const password = pwdInput.value;

    submitBtn.disabled = true;
    submitBtn.innerHTML = `<i data-lucide="loader" class="w-4 h-4 animate-spin"></i> Signing in...`;
    renderIcons(submitBtn);

    try {
      await authStore.login(identifier, password);
      toast.success('Signed in successfully!');

      if (redirectTarget) {
        window.location.hash = `#/${redirectTarget.replace(/^#\/?/, '')}`;
      } else {
        window.location.hash = '#/';
      }
    } catch (err) {
      errorText.textContent = err.message || 'Invalid credentials. Please try again.';
      errorAlert.classList.remove('hidden');
      renderIcons(errorAlert);
      submitBtn.disabled = false;
      submitBtn.innerHTML = `<span>Sign In &rarr;</span>`;
      renderIcons(submitBtn);
    }
  });

  return container;
}
