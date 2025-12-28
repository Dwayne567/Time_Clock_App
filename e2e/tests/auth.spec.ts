import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    // Wait for Angular to load
    await page.waitForLoadState('networkidle');
  });

  test('should display login page', async ({ page }) => {
    await expect(page.locator('h1')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('#email')).toBeVisible();
  });

  test('should show validation errors for empty login', async ({ page }) => {
    // Touch fields to trigger validation
    await page.locator('#email').click();
    await page.locator('#password').click();
    await page.locator('#email').click(); // blur password field
    
    // Check for validation messages
    await expect(page.locator('.error-message').first()).toBeVisible();
  });

  test('should login with valid credentials', async ({ page }) => {
    await page.locator('#email').fill('user1@test.com');
    await page.locator('#password').fill('Coding@1234?');
    await page.locator('button[type="submit"]').click();

    // Should redirect to dashboard after successful login
    await expect(page).toHaveURL(/.*dashboard/i, { timeout: 10000 });
  });

  test('should show error for invalid credentials', async ({ page }) => {
    await page.locator('#email').fill('invalid@example.com');
    await page.locator('#password').fill('wrongpassword');
    await page.locator('button[type="submit"]').click();

    // Should show error message
    await expect(page.locator('.login-error')).toBeVisible({ timeout: 5000 });
  });

  test('should navigate to register page', async ({ page }) => {
    await page.getByRole('link', { name: /sign up/i }).click();
    await expect(page).toHaveURL(/.*register/i);
  });

  test('should logout successfully', async ({ page }) => {
    // First login
    await page.locator('#email').fill('user1@test.com');
    await page.locator('#password').fill('Coding@1234?');
    await page.locator('button[type="submit"]').click();
    await expect(page).toHaveURL(/.*dashboard/i, { timeout: 10000 });

    // Then logout
    await page.getByRole('button', { name: /logout/i }).click();
    await expect(page).toHaveURL(/.*login/i);
  });
});
