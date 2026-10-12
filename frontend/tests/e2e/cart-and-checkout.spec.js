import { test, expect } from '@playwright/test';

test.describe('Cart & Checkout Workflow', () => {

  test('Guest adds item to cart, logs in, checks out with COD, and tracks order', async ({ page }) => {
    // 1. Visit catalog
    await page.goto('/#/products');

    // Wait for product cards
    const grid = page.locator('#catalog-products-grid');
    await expect(grid).toBeVisible();

    // Find first add-to-cart button and click it
    const addBtn = grid.locator('.add-to-cart-btn').first();
    await expect(addBtn).toBeVisible({ timeout: 10000 });
    await addBtn.click();

    // Verify cart badge updates
    const cartBadge = page.locator('#header-cart-count');
    await expect(cartBadge).toHaveText('1');

    // 2. Open cart drawer
    await page.locator('#header-cart-btn').click();
    const cartPanel = page.locator('#cart-drawer-panel');
    await expect(cartPanel).toBeVisible();

    // Click increment (+)
    const plusBtn = cartPanel.locator('.cart-qty-btn[data-action="increase"]').first();
    await expect(plusBtn).toBeVisible();
    await plusBtn.click();
    await expect(cartBadge).toHaveText('2');

    // Close drawer
    await page.locator('#cart-drawer-close-btn').click();

    // 3. Login
    await page.goto('/#/login?redirect=checkout');
    await page.locator('#login-identifier').fill('01712345678');
    await page.locator('#login-password').fill('Password123!');
    await page.locator('#login-submit-btn').click();
    await expect(page).toHaveURL(/#\/checkout/, { timeout: 10000 });

    // Check delivery address selector is visible
    const addrRadio = page.locator('input[name="checkout-address"]').first();
    await expect(addrRadio).toBeVisible({ timeout: 10000 });

    // Check delivery methods are visible
    const delivRadio = page.locator('input[name="checkout-delivery"]').first();
    await expect(delivRadio).toBeVisible({ timeout: 10000 });

    // Select COD if not already selected
    const codRadio = page.locator('input[name="checkout-payment"][data-code="cod"]');
    await expect(codRadio).toBeVisible({ timeout: 10000 });
    await codRadio.check();

    // Submit order
    const placeOrderBtn = page.locator('#place-order-submit-btn');
    await expect(placeOrderBtn).toBeVisible();
    await placeOrderBtn.click();

    // 5. Order confirmation
    await expect(page).toHaveURL(/#\/order-confirmed\/\d+/, { timeout: 10000 });
    await expect(page.locator('h1')).toContainText(/Thank You For Your Order/i);
    await expect(page.locator('body')).toContainText(/ORD-/i);

    // 6. View order tracking
    const trackingBtn = page.locator('a:has-text("View Order Tracking")');
    await trackingBtn.click();
    await expect(page).toHaveURL(/#\/account\/orders\/\d+/);
    await expect(page.locator('h1')).toContainText(/ORD-/i);
    await expect(page.locator('body')).toContainText(/Pending/i);
  });

});
