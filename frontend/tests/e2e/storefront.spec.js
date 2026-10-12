import { test, expect } from '@playwright/test';

test.describe('Storefront & Catalog Browsing', () => {

  test('Homepage renders brand, categories, and featured products from PostgreSQL', async ({ page }) => {
    await page.goto('/');

    // Check title and brand
    await expect(page).toHaveTitle(/Niramoy Health/i);
    const brandLogo = page.locator('#header-brand-logo');
    await expect(brandLogo).toBeVisible();
    await expect(brandLogo).toContainText('Niramoy');
    await expect(brandLogo).toContainText('Health');

    // Wait for categories grid to load
    const categoryGrid = page.locator('#home-categories-grid');
    await expect(categoryGrid).toBeVisible();
    await expect(categoryGrid.locator('a')).toHaveCount(6);

    // Wait for featured products to load
    const featuredGrid = page.locator('#home-featured-grid');
    await expect(featuredGrid).toBeVisible();
    const productCards = featuredGrid.locator('.group');
    await expect(productCards.first()).toBeVisible();
  });

  test('Live search finds Napa and navigates to product details', async ({ page }) => {
    await page.goto('/');

    // Type in search bar
    const searchInput = page.locator('#header-search-input');
    await searchInput.fill('Napa');

    // Wait for autocomplete suggestions
    const dropdown = page.locator('#search-suggestions-dropdown');
    await expect(dropdown).toBeVisible({ timeout: 5000 });
    await expect(dropdown).toContainText('Napa');

    // Press enter or click search result
    const firstResult = dropdown.locator('a').first();
    await firstResult.click();

    // Check URL and product details
    await expect(page).toHaveURL(/#\/products\//);
    const heading = page.locator('h1');
    await expect(heading).toBeVisible();
    await expect(heading).toContainText('Napa');

    // Check dosage form, generic, and manufacturer
    await expect(page.locator('body')).toContainText(/Generic:|Manufacturer:/i);
  });

  test('Responsive layouts at mobile (375px), tablet (768px), and desktop (1280px)', async ({ page }) => {
    // 1. Mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await expect(page.locator('#mobile-menu-btn')).toBeVisible();

    // 2. Tablet viewport
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/#/products');
    await expect(page.locator('#catalog-products-grid')).toBeVisible();

    // 3. Desktop viewport
    await page.setViewportSize({ width: 1280, height: 800 });
    await page.goto('/#/products');
    await expect(page.locator('aside')).toBeVisible();
  });

});
