import { renderIcons } from '../utils/icons.js';

export function createFooter() {
  const footer = document.createElement('footer');
  footer.className = 'mt-auto bg-slate-900 text-slate-300 border-t border-slate-800';

  footer.innerHTML = `
    <!-- Value proposition trust bar -->
    <div class="border-b border-slate-800/80 py-6 sm:py-8 px-4 sm:px-6">
      <div class="max-w-7xl mx-auto grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6">
        <div class="flex items-start gap-3">
          <div class="w-10 h-10 rounded-xl bg-brand-900/50 border border-brand-700/30 flex items-center justify-center text-brand-400 flex-shrink-0">
            <i data-lucide="shield-check" class="w-5 h-5"></i>
          </div>
          <div>
            <h4 class="text-xs font-bold text-white uppercase tracking-wider">100% Genuine Medicines</h4>
            <p class="text-[11px] text-slate-400 mt-0.5">Sourced directly from DGDA licensed top manufacturers</p>
          </div>
        </div>

        <div class="flex items-start gap-3">
          <div class="w-10 h-10 rounded-xl bg-brand-900/50 border border-brand-700/30 flex items-center justify-center text-brand-400 flex-shrink-0">
            <i data-lucide="stethoscope" class="w-5 h-5"></i>
          </div>
          <div>
            <h4 class="text-xs font-bold text-white uppercase tracking-wider">BMDC Pharmacist Review</h4>
            <p class="text-[11px] text-slate-400 mt-0.5">Every prescription is verified by registered pharmacists</p>
          </div>
        </div>

        <div class="flex items-start gap-3">
          <div class="w-10 h-10 rounded-xl bg-brand-900/50 border border-brand-700/30 flex items-center justify-center text-brand-400 flex-shrink-0">
            <i data-lucide="truck" class="w-5 h-5"></i>
          </div>
          <div>
            <h4 class="text-xs font-bold text-white uppercase tracking-wider">Express Delivery</h4>
            <p class="text-[11px] text-slate-400 mt-0.5">Fast delivery inside Dhaka and reliable district shipping</p>
          </div>
        </div>

        <div class="flex items-start gap-3">
          <div class="w-10 h-10 rounded-xl bg-brand-900/50 border border-brand-700/30 flex items-center justify-center text-brand-400 flex-shrink-0">
            <i data-lucide="credit-card" class="w-5 h-5"></i>
          </div>
          <div>
            <h4 class="text-xs font-bold text-white uppercase tracking-wider">Flexible Payments</h4>
            <p class="text-[11px] text-slate-400 mt-0.5">bKash, Nagad, Card and Cash on Delivery supported</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Footer Links -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 py-12">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-8">
        
        <!-- Brand & Contact -->
        <div class="lg:col-span-2 space-y-4">
          <div class="flex items-center gap-2.5">
            <div class="w-8 h-8 rounded-lg bg-brand-600 flex items-center justify-center text-white">
              <i data-lucide="cross" class="w-5 h-5 stroke-[2.5]"></i>
            </div>
            <span class="text-lg font-bold text-white tracking-tight">Niramoy <span class="text-brand-400">Health</span></span>
          </div>
          <p class="text-xs text-slate-400 leading-relaxed max-w-sm">
            Niramoy Health (নিরাময় হেলথ) is a technology-enabled digital healthcare platform serving individuals and families across Bangladesh with authentic medicines, healthcare essentials, and professional pharmacist care.
          </p>
          <div class="space-y-1.5 text-xs text-slate-300">
            <div class="flex items-center gap-2">
              <i data-lucide="phone" class="w-3.5 h-3.5 text-brand-400"></i>
              <span>Hotline: 16247 (9 AM – 11 PM)</span>
            </div>
            <div class="flex items-center gap-2">
              <i data-lucide="mail" class="w-3.5 h-3.5 text-brand-400"></i>
              <span>support@niramoyhealth.com</span>
            </div>
            <div class="flex items-center gap-2">
              <i data-lucide="map-pin" class="w-3.5 h-3.5 text-brand-400"></i>
              <span>Gulshan-2, Dhaka-1212, Bangladesh</span>
            </div>
          </div>
        </div>

        <!-- Categories -->
        <div class="space-y-3">
          <h5 class="text-xs font-bold text-white uppercase tracking-wider">Categories</h5>
          <ul class="space-y-2 text-xs text-slate-400">
            <li><a href="#/products?category=prescription-medicine" class="hover:text-brand-400 transition">Prescription Drugs</a></li>
            <li><a href="#/products?category=otc-medicine" class="hover:text-brand-400 transition">OTC Medicines</a></li>
            <li><a href="#/products?category=vitamins-supplements" class="hover:text-brand-400 transition">Vitamins & Supplements</a></li>
            <li><a href="#/products?category=healthcare-devices" class="hover:text-brand-400 transition">Healthcare Devices</a></li>
            <li><a href="#/products?category=baby-mom-care" class="hover:text-brand-400 transition">Baby & Mom Care</a></li>
            <li><a href="#/products?category=beauty-skincare" class="hover:text-brand-400 transition">Personal Care</a></li>
          </ul>
        </div>

        <!-- Customer Care -->
        <div class="space-y-3">
          <h5 class="text-xs font-bold text-white uppercase tracking-wider">Customer Care</h5>
          <ul class="space-y-2 text-xs text-slate-400">
            <li><a href="#/prescriptions/upload" class="hover:text-brand-400 transition">Upload Prescription</a></li>
            <li><a href="#/account/orders" class="hover:text-brand-400 transition">Order Tracking</a></li>
            <li><a href="#/account" class="hover:text-brand-400 transition">My Account</a></li>
            <li><a href="#/support" class="hover:text-brand-400 transition">Help & Support Ticket</a></li>
            <li><a href="#/support" class="hover:text-brand-400 transition">Shipping & Delivery Info</a></li>
          </ul>
        </div>

        <!-- Policies & Compliance -->
        <div class="space-y-3">
          <h5 class="text-xs font-bold text-white uppercase tracking-wider">Policies</h5>
          <ul class="space-y-2 text-xs text-slate-400">
            <li><a href="#/privacy-policy" class="hover:text-brand-400 transition">Privacy Policy</a></li>
            <li><a href="#/terms" class="hover:text-brand-400 transition">Terms & Conditions</a></li>
            <li><a href="#/refund-policy" class="hover:text-brand-400 transition">Refund & Return Policy</a></li>
            <li><a href="#/disclaimer" class="hover:text-brand-400 transition">Medical Disclaimer</a></li>
          </ul>
          
          <div class="pt-2">
            <h6 class="text-[11px] font-semibold text-slate-400 mb-2">Payment Methods</h6>
            <div class="flex flex-wrap gap-1.5 text-[10px] font-bold text-slate-300">
              <span class="px-2 py-0.5 bg-pink-900/60 text-pink-300 rounded border border-pink-700/50">bKash</span>
              <span class="px-2 py-0.5 bg-orange-900/60 text-orange-300 rounded border border-orange-700/50">Nagad</span>
              <span class="px-2 py-0.5 bg-purple-900/60 text-purple-300 rounded border border-purple-700/50">Rocket</span>
              <span class="px-2 py-0.5 bg-emerald-900/60 text-emerald-300 rounded border border-emerald-700/50">COD</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Copyright & Disclaimer -->
      <div class="border-t border-slate-800/80 mt-10 pt-6 flex flex-col md:flex-row justify-between items-center gap-4 text-xs text-slate-500">
        <p>&copy; 2026 Niramoy Health Ltd. All rights reserved.</p>
        <p class="text-[11px] text-slate-500 text-center md:text-right max-w-xl">
          Disclaimer: Information on this site is not intended to substitute professional medical advice. Always consult a licensed physician for prescription drugs.
        </p>
      </div>
    </div>
  `;

  renderIcons(footer);
  return footer;
}
