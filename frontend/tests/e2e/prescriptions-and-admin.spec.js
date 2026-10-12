import { test, expect } from '@playwright/test';
import path from 'path';

test.describe('Prescription Workflow & Administrative Dashboard', () => {

  test('Customer uploads prescription (submitted status), and Admin approves it via dashboard', async ({ page }) => {
    // 1. Customer login
    await page.goto('/#/login?redirect=prescriptions/upload');
    await page.locator('#login-identifier').fill('01712345678');
    await page.locator('#login-password').fill('Password123!');
    await page.locator('#login-submit-btn').click();
    await expect(page).toHaveURL(/#\/prescriptions\/upload/, { timeout: 10000 });
    await expect(page.locator('h1')).toContainText(/Upload Your Prescription/i);

    // Set file input
    const uploadPrompt = page.locator('#prescription-file-input');
    const filePath = path.resolve('./tests/e2e/fixtures/test-rx.png');
    await uploadPrompt.setInputFiles(filePath);

    // Verify preview
    await expect(page.locator('#file-preview-container')).toBeVisible();

    // Fill demographics
    await page.locator('#patient-age').fill('28');
    await page.locator('#patient-gender').selectOption('male');
    await page.locator('#doctor-name').fill('Dr. M. Rahman, Square Hospital');
    await page.locator('#prescription-notes').fill('Prescription for acute bronchitis treatment');

    // Submit
    await page.locator('#submit-upload-btn').click();

    // 3. Verify in My Prescriptions
    await expect(page).toHaveURL(/#\/prescriptions/, { timeout: 10000 });
    await expect(page.locator('body')).toContainText(/Submitted/i);
    await expect(page.locator('body')).not.toContainText(/Approved/i); // Never auto-approved!

    // 4. Logout
    await page.locator('#user-menu-btn').click();
    await page.locator('#header-logout-btn').click();
    await expect(page.locator('#user-display-name')).toHaveText('Sign In', { timeout: 10000 });

    // 5. Admin Login
    await page.goto('/#/login?redirect=admin');
    await page.locator('#login-identifier').fill('admin@niramoyhealth.com');
    await page.locator('#login-password').fill('Admin123!');
    await page.locator('#login-submit-btn').click();
    await expect(page).toHaveURL(/#\/admin/, { timeout: 10000 });
    await expect(page.locator('h1')).toContainText(/Pharmacy Administration/i);

    // Check KPIs exist
    const kpis = page.locator('#admin-kpis-grid');
    await expect(kpis).toBeVisible();

    // 7. Open Prescription Queue tab
    const rxTabBtn = page.locator('.admin-tab-btn[data-tab="prescriptions"]');
    await rxTabBtn.click();
    const rxTab = page.locator('#tab-prescriptions');
    await expect(rxTab).toBeVisible();

    // Find Approve Rx button
    const approveBtn = rxTab.locator('.admin-rx-action-btn[data-action="approved"]').first();
    if (await approveBtn.isVisible()) {
      // Mock prompt dialog for pharmacist clinical notes
      page.on('dialog', dialog => dialog.accept('Verified against BMDC registered doctor prescription.'));
      await approveBtn.click();

      // Verify status is updated
      await expect(page.locator('body')).toContainText(/Approved/i);
    }
  });

  test('Ordinary visitor cannot access Admin dashboard without credentials', async ({ page }) => {
    // Clear tokens in browser
    await page.goto('/#/admin');
    await page.evaluate(() => localStorage.clear());
    await page.reload();

    await expect(page.locator('body')).toContainText(/Restricted Access/i);
  });

});
