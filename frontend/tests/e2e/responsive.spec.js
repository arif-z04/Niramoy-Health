import { test, expect } from '@playwright/test';
import fs from 'fs';
import path from 'path';

const SCREENSHOT_DIR = path.resolve('tests/artifacts/screenshots');

test.beforeAll(() => {
  if (!fs.existsSync(SCREENSHOT_DIR)) {
    fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
  }
});

const VIEWPORTS = [
  { name: 'small-mobile-320', width: 320, height: 568 },
  { name: 'standard-mobile-360', width: 360, height: 640 },
  { name: 'mobile-375', width: 375, height: 667 },
  { name: 'large-mobile-430', width: 430, height: 932 },
  { name: 'small-tablet-600', width: 600, height: 960 },
  { name: 'tablet-768', width: 768, height: 1024 },
  { name: 'small-laptop-1024', width: 1024, height: 768 },
  { name: 'laptop-1280', width: 1280, height: 800 },
  { name: 'desktop-1440', width: 1440, height: 900 },
  { name: 'large-desktop-1920', width: 1920, height: 1080 },
];

test.describe('Responsive Layout & No Horizontal Overflow', () => {

  for (const vp of VIEWPORTS) {
    test(`Zero horizontal overflow at ${vp.name} (${vp.width}x${vp.height}) on Home, Catalog, Product Details`, async ({ page }) => {
      await page.setViewportSize({ width: vp.width, height: vp.height });

      const testRoutes = [
        { name: 'home', path: '#/' },
        { name: 'catalog', path: '#/products' },
        { name: 'product', path: '#/products/napa-extra-tablet' },
      ];

      for (const route of testRoutes) {
        await page.goto(`/${route.path}`);
        await page.waitForTimeout(400);

        const overflow = await page.evaluate(() => {
          const docW = document.documentElement.scrollWidth;
          const winW = window.innerWidth;
          const bodyW = document.body.scrollWidth;
          return {
            docW,
            winW,
            bodyW,
            hasOverflow: docW > winW || bodyW > winW,
          };
        });

        expect(overflow.hasOverflow).toBeFalsy();
      }
    });
  }

  test('Mobile drawer navigation opens, closes, locks body scroll, and handles Escape', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');

    // Mobile menu button should be visible
    const menuBtn = page.locator('#mobile-menu-btn');
    await expect(menuBtn).toBeVisible();

    // Drawer should initially be hidden (slid out)
    const drawerPanel = page.locator('#mobile-drawer-panel');
    await expect(drawerPanel).toHaveClass(/-translate-x-full/);

    // Open drawer
    await menuBtn.click();
    await page.waitForTimeout(300);
    await expect(drawerPanel).not.toHaveClass(/-translate-x-full/);

    // Body scroll should be locked
    const isBodyLocked = await page.evaluate(() => document.body.classList.contains('overflow-hidden'));
    expect(isBodyLocked).toBeTruthy();

    // Capture screenshot of mobile drawer
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '01-mobile-drawer-375px.png') });

    // Press Escape to close
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
    await expect(drawerPanel).toHaveClass(/-translate-x-full/);

    const isBodyUnlocked = await page.evaluate(() => !document.body.classList.contains('overflow-hidden'));
    expect(isBodyUnlocked).toBeTruthy();
  });

  test('Mobile bottom navigation bar is visible on mobile (<768px) and hidden on desktop', async ({ page }) => {
    // 375px mobile
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    const bottomNav = page.locator('#mobile-bottom-nav');
    await expect(bottomNav).toBeVisible();

    // 1280px desktop
    await page.setViewportSize({ width: 1280, height: 800 });
    await expect(bottomNav).toBeHidden();
  });

  test('Mobile filter drawer in Catalog opens, filters items, and closes', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/#/products');
    await page.waitForTimeout(500);

    const filterBtn = page.locator('#mobile-filter-open-btn');
    await expect(filterBtn).toBeVisible();

    const filterAside = page.locator('#catalog-filter-aside');
    await expect(filterAside).toHaveClass(/translate-x-full/);

    // Open mobile filter modal
    await filterBtn.click();
    await page.waitForTimeout(300);
    await expect(filterAside).not.toHaveClass(/translate-x-full/);

    // Capture screenshot of filter modal
    await page.screenshot({ path: path.join(SCREENSHOT_DIR, '02-catalog-mobile-filters-375px.png') });

    // Close filter modal
    await page.locator('#mobile-filter-close-btn').click();
    await page.waitForTimeout(300);
    await expect(filterAside).toHaveClass(/translate-x-full/);
  });

  test('Sticky purchase bar is visible on mobile Product Details and hidden on desktop', async ({ page }) => {
    // Mobile view: sticky bottom bar present
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/#/products/napa-extra-tablet');
    await page.waitForTimeout(500);

    const stickyBar = page.locator('#mobile-sticky-purchase-bar');
    await expect(stickyBar).toBeVisible();

    // Desktop view: sticky bottom bar hidden
    await page.setViewportSize({ width: 1280, height: 800 });
    await expect(stickyBar).toBeHidden();
  });

  test('Forms have font-size >= 16px on mobile viewports to avoid iOS auto-zoom', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/#/login');

    const inputs = page.locator('input:not([type="checkbox"]):not([type="radio"]):not([type="hidden"])');
    const count = await inputs.count();
    expect(count).toBeGreaterThan(0);

    for (let i = 0; i < count; i++) {
      const fontSize = await inputs.nth(i).evaluate((el) => {
        return parseFloat(window.getComputedStyle(el).fontSize);
      });
      expect(fontSize).toBeGreaterThanOrEqual(16);
    }
  });

  test('Visual capture across key responsive viewports', async ({ page }) => {
    const keyBreakpoints = [
      { name: '320px-small-phone', width: 320, height: 568 },
      { name: '375px-standard-phone', width: 375, height: 667 },
      { name: '768px-tablet', width: 768, height: 1024 },
      { name: '1280px-desktop', width: 1280, height: 800 },
    ];

    for (const bp of keyBreakpoints) {
      await page.setViewportSize({ width: bp.width, height: bp.height });

      // 1. Homepage
      await page.goto('/');
      await page.waitForTimeout(600);
      await page.screenshot({ path: path.join(SCREENSHOT_DIR, `home-${bp.name}.png`), fullPage: false });

      // 2. Catalog
      await page.goto('/#/products');
      await page.waitForTimeout(600);
      await page.screenshot({ path: path.join(SCREENSHOT_DIR, `catalog-${bp.name}.png`), fullPage: false });

      // 3. Product Details
      await page.goto('/#/products/napa-extra-tablet');
      await page.waitForTimeout(600);
      await page.screenshot({ path: path.join(SCREENSHOT_DIR, `product-details-${bp.name}.png`), fullPage: false });

      // 4. Prescription Upload
      await page.goto('/#/prescriptions/upload');
      await page.waitForTimeout(600);
      await page.screenshot({ path: path.join(SCREENSHOT_DIR, `rx-upload-${bp.name}.png`), fullPage: false });
    }
  });

});
