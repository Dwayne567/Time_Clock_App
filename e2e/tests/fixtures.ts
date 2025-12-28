import { test as base, expect } from '@playwright/test';

// Test user credentials - update these for your test environment
export const TEST_USER = {
  email: 'user1@test.com',
  password: 'Coding@1234?',
};

export const ADMIN_USER = {
  email: 'admin@example.com',
  password: 'Admin123!',
};

// Extend base test with authentication fixtures
export const test = base.extend<{
  authenticatedPage: typeof base;
}>({
  // This fixture logs in before each test
  authenticatedPage: async ({ page }, use) => {
    await page.goto('/login');
    await page.getByLabel(/email|username/i).fill(TEST_USER.email);
    await page.getByLabel(/password/i).fill(TEST_USER.password);
    await page.getByRole('button', { name: /login/i }).click();
    await expect(page).toHaveURL(/.*dashboard/i, { timeout: 10000 });
    
    await use(page as any);
  },
});

export { expect };
