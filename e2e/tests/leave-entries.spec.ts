import { test, expect, Page } from '@playwright/test';

const getLeaveEntriesSection = (page: Page) =>
  page.locator('div.entries-section', {
    has: page.getByRole('heading', { name: /leave entries/i }),
  });

const getLeaveEntryForm = (page: Page) =>
  getLeaveEntriesSection(page).locator('.add-form');

const getLeaveTypeSelect = (page: Page) =>
  getLeaveEntryForm(page).locator('select');

const getLeaveDurationInput = (page: Page) =>
  getLeaveEntryForm(page).locator('input[type="number"]');

const waitForLeaveAddRequest = (page: Page) =>
  page.waitForResponse(
    (response) =>
      response
        .url()
        .toLowerCase()
        .includes('/api/dashboard/addleave') && response.ok()
  );

test.describe('Leave Entries', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('/login');
    await page.waitForLoadState('networkidle');
    await page.locator('#email').fill('user1@test.com');
    await page.locator('#password').fill('Coding@1234?');
    await page.locator('button[type="submit"]').click();
    await expect(page).toHaveURL(/.*dashboard/i, { timeout: 10000 });
    await page.waitForLoadState('networkidle');
  });

  test('should display leave entries section', async ({ page }) => {
    await expect(
      getLeaveEntriesSection(page).getByRole('heading', { name: /leave entries/i })
    ).toBeVisible();
  });

  test('should show add leave entry button', async ({ page }) => {
    await expect(
      getLeaveEntriesSection(page).getByRole('button', { name: /add leave entry|add leave/i })
    ).toBeVisible();
  });

  test('should open add leave entry form', async ({ page }) => {
    await getLeaveEntriesSection(page)
      .getByRole('button', { name: /add leave entry|add leave/i })
      .click();
    
    // Form should be visible
    await expect(getLeaveEntryForm(page)).toBeVisible();
    await expect(getLeaveTypeSelect(page)).toBeVisible();
  });

  test('should submit a leave request', async ({ page }) => {
    await getLeaveEntriesSection(page)
      .getByRole('button', { name: /add leave entry|add leave/i })
      .click();
    await expect(getLeaveEntryForm(page)).toBeVisible();
    
    // Fill in the form
    await getLeaveTypeSelect(page).selectOption({ index: 1 });
    await getLeaveDurationInput(page).fill('8');
    
    // Submit
    const addLeaveResponse = waitForLeaveAddRequest(page);
    await getLeaveEntriesSection(page)
      .getByRole('button', { name: /^save$/i })
      .click();
    await addLeaveResponse;
  });
});
