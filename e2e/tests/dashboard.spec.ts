import { test, expect, Page } from '@playwright/test';

const getDayEntriesSection = (page: Page) =>
  page.locator('div.entries-section', {
    has: page.getByRole('heading', { name: /day entries/i }),
  });

const waitForClockInOutRequest = (page: Page) =>
  page.waitForResponse(
    (response) =>
      response
        .url()
        .toLowerCase()
        .includes('/api/dashboard/clockinout') && response.ok()
  );

const waitForDashboardRequest = (page: Page) =>
  page.waitForResponse(
    (response) =>
      response
        .url()
        .toLowerCase()
        .includes('/api/dashboard/index') && response.ok()
  );

const getCurrentWeekLabel = () => {
  const today = new Date();
  const weekStart = new Date(today);
  weekStart.setHours(0, 0, 0, 0);
  weekStart.setDate(weekStart.getDate() - weekStart.getDay());
  return weekStart.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
};

const ensureCurrentWeek = async (page: Page) => {
  const weekDisplay = page.locator('.week-display');
  const expectedWeekLabel = getCurrentWeekLabel();
  let displayText = (await weekDisplay.textContent()) ?? '';

  if (displayText.includes(expectedWeekLabel)) {
    return;
  }

  const nextButton = page.getByRole('button', { name: /next week/i });
  for (let attempt = 0; attempt < 2; attempt += 1) {
    if (!(await nextButton.isVisible())) {
      break;
    }

    const dashboardReload = waitForDashboardRequest(page);
    await nextButton.click();
    await dashboardReload;
    displayText = (await weekDisplay.textContent()) ?? '';

    if (displayText.includes(expectedWeekLabel)) {
      return;
    }
  }

  await expect(weekDisplay).toContainText(expectedWeekLabel);
};

test.describe('Dashboard - Time Clock', () => {
  test.describe.configure({ mode: 'serial' });

  test.beforeEach(async ({ page }) => {
    // Login before each test
    await page.goto('/login');
    await page.waitForLoadState('networkidle');
    await page.locator('#email').fill('user1@test.com');
    await page.locator('#password').fill('Coding@1234?');
    await page.locator('button[type="submit"]').click();
    await expect(page).toHaveURL(/.*dashboard/i, { timeout: 10000 });
    await page.waitForLoadState('networkidle');
    await ensureCurrentWeek(page);
  });

  test('should display dashboard with user name', async ({ page }) => {
    await expect(page.getByText(/welcome/i)).toBeVisible();
  });

  test('should display week picker', async ({ page }) => {
    await expect(page.locator('.week-display')).toBeVisible();
    await expect(page.getByRole('button', { name: /previous week/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /next week/i })).toBeVisible();
  });

  test('should clock in successfully', async ({ page }) => {
    // Look for clock in button
    const clockInBtn = page.getByRole('button', { name: /clock in/i });
    const clockOutBtn = page.getByRole('button', { name: /clock out/i });
    
    if (await clockInBtn.isEnabled()) {
      const clockInResponse = waitForClockInOutRequest(page);
      const dashboardReload = waitForDashboardRequest(page);
      await clockInBtn.click();
      await clockInResponse;
      await dashboardReload;
    }

    // After clocking in, clock out should be enabled
    await expect(clockOutBtn).toBeEnabled({ timeout: 10000 });
  });

  test('should clock out successfully', async ({ page }) => {
    const clockInBtn = page.getByRole('button', { name: /clock in/i });
    const clockOutBtn = page.getByRole('button', { name: /clock out/i });
    
    // Ensure we are clocked in before clocking out
    if (!(await clockOutBtn.isEnabled()) && (await clockInBtn.isEnabled())) {
      const clockInResponse = waitForClockInOutRequest(page);
      const dashboardReload = waitForDashboardRequest(page);
      await clockInBtn.click();
      await clockInResponse;
      await dashboardReload;
      await expect(clockOutBtn).toBeEnabled({ timeout: 10000 });
    }

    // Now clock out
    const clockOutResponse = waitForClockInOutRequest(page);
    const dashboardReload = waitForDashboardRequest(page);
    await clockOutBtn.click();
    await clockOutResponse;
    await dashboardReload;

    // Should see clock in button enabled again
    await expect(clockInBtn).toBeEnabled({ timeout: 10000 });
  });

  test('should display day entries table', async ({ page }) => {
    const dayEntriesSection = getDayEntriesSection(page);
    await expect(
      dayEntriesSection.getByRole('heading', { name: /day entries/i })
    ).toBeVisible();
    
    // Check table headers
    await expect(dayEntriesSection.getByRole('columnheader', { name: /date/i })).toBeVisible();
    await expect(dayEntriesSection.getByRole('columnheader', { name: /start time/i })).toBeVisible();
    await expect(dayEntriesSection.getByRole('columnheader', { name: /end time/i })).toBeVisible();
    await expect(dayEntriesSection.getByRole('columnheader', { name: /duration/i })).toBeVisible();
  });

  test('should navigate to previous week', async ({ page }) => {
    const prevButton = page.getByRole('button', { name: /previous week/i });
    
    if (await prevButton.isVisible()) {
      const weekDisplay = page.locator('.week-display');
      const initialWeek = await weekDisplay.textContent();
      
      await prevButton.click();
      await page.waitForLoadState('networkidle');
      await expect(weekDisplay).not.toHaveText(initialWeek ?? '');
    }
  });

  test('should navigate to next week', async ({ page }) => {
    const nextButton = page.getByRole('button', { name: /next week/i });
    
    if (await nextButton.isVisible()) {
      const weekDisplay = page.locator('.week-display');
      const initialWeek = await weekDisplay.textContent();
      
      await nextButton.click();
      await page.waitForLoadState('networkidle');
      await expect(weekDisplay).not.toHaveText(initialWeek ?? '');
    }
  });
});
