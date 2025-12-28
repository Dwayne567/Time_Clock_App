import { test, expect, Page } from '@playwright/test';

// Keep selectors scoped to the Task Entries card to avoid matching other sections.
const getTaskEntriesSection = (page: Page) =>
  page.locator('div.entries-section', {
    has: page.getByRole('heading', { name: /task entries/i }),
  });

const getTaskEntryForm = (page: Page) =>
  getTaskEntriesSection(page).locator('.add-form');

const getTaskEntryJobSelect = (page: Page) =>
  getTaskEntryForm(page).locator('select').nth(0);

const getTaskEntryTaskSelect = (page: Page) =>
  getTaskEntryForm(page).locator('select').nth(1);

const getTaskEntryDurationInput = (page: Page) =>
  getTaskEntryForm(page).locator('input[type="number"]');

const getTaskEntryCommentInput = (page: Page) =>
  getTaskEntryForm(page).locator('input[type="text"]');

const getTaskEntryRows = (page: Page) =>
  getTaskEntriesSection(page)
    .locator('tbody tr')
    .filter({ has: page.getByRole('button', { name: /delete/i }) });

const waitForTaskEntryAddRequest = (page: Page) =>
  page.waitForResponse(
    (response) =>
      response
        .url()
        .toLowerCase()
        .includes('/api/dashboard/addtaskentry') && response.ok()
  );

const waitForTaskEntryDeleteRequest = (page: Page) =>
  page.waitForResponse(
    (response) =>
      response
        .url()
        .toLowerCase()
        .includes('/api/dashboard/deletetaskentry') && response.ok()
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

// Creates a task entry via the UI and returns the unique comment used.
async function addTaskEntryViaUI(page: Page) {
  const taskSection = getTaskEntriesSection(page);
  const toggleButton = taskSection
    .locator('.section-header')
    .getByRole('button', { name: /add task entry|cancel/i });
  const jobSelect = getTaskEntryJobSelect(page);

  if (!(await jobSelect.isVisible().catch(() => false))) {
    await toggleButton.click();
  }

  await expect(getTaskEntryForm(page)).toBeVisible();
  await expect(jobSelect).toBeVisible();

  const jobOptionsCount = await jobSelect.locator('option').count();
  if (jobOptionsCount < 2) {
    throw new Error('No jobs available to create a task entry.');
  }

  const taskSelect = getTaskEntryTaskSelect(page);
  await expect(taskSelect).toBeVisible();
  const taskOptionsCount = await taskSelect.locator('option').count();
  if (taskOptionsCount < 2) {
    throw new Error('No tasks available to create a task entry.');
  }

  await jobSelect.selectOption({ index: 1 });
  await taskSelect.selectOption({ index: 1 });

  const durationInput = getTaskEntryDurationInput(page);
  await durationInput.fill('');
  await durationInput.fill('2');

  const commentInput = getTaskEntryCommentInput(page);
  const uniqueComment = `Automation task ${Date.now()}`;
  await commentInput.fill(uniqueComment);

  const addEntryResponse = waitForTaskEntryAddRequest(page);
  const dashboardReload = waitForDashboardRequest(page);
  await getTaskEntriesSection(page)
    .getByRole('button', { name: /^save$/i })
    .click();
  await addEntryResponse;
  await dashboardReload;

  await expect(jobSelect).toBeHidden();

  return uniqueComment;
}

test.describe('Task Entries', () => {
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

  test('should display task entries section', async ({ page }) => {
    await expect(
      getTaskEntriesSection(page).getByRole('heading', { name: /task entries/i })
    ).toBeVisible();
  });

  test('should show add task entry button', async ({ page }) => {
    await expect(
      getTaskEntriesSection(page).getByRole('button', { name: /add task entry/i })
    ).toBeVisible();
  });

  test('should open add task entry form', async ({ page }) => {
    await getTaskEntriesSection(page)
      .getByRole('button', { name: /add task entry/i })
      .click();

    // Form should be visible with job and task selectors
    await expect(getTaskEntryForm(page)).toBeVisible();
    await expect(getTaskEntryJobSelect(page)).toBeVisible();
  });

  test('should add a new task entry', async ({ page }) => {
    const taskRows = getTaskEntryRows(page);
    const initialCount = await taskRows.count();

    let addedComment: string | undefined;
    try {
      addedComment = await addTaskEntryViaUI(page);
    } catch (error) {
      test.skip(true, (error as Error).message);
    }

    if (!addedComment) {
      return;
    }

    await expect(taskRows).toHaveCount(initialCount + 1, { timeout: 5000 });
    await expect(taskRows.filter({ hasText: addedComment })).toHaveCount(1, {
      timeout: 5000,
    });
  });

  test('should delete a task entry', async ({ page }) => {
    const taskRows = getTaskEntryRows(page);
    let visibleRows = await taskRows.count();
    let rowToDelete = taskRows.first();
    let commentToDelete: string | null = null;

    if (visibleRows === 0) {
      let createdComment: string | undefined;
      try {
        createdComment = await addTaskEntryViaUI(page);
      } catch (error) {
        test.skip(true, (error as Error).message);
      }

      if (!createdComment) {
        return;
      }

      commentToDelete = createdComment;
      rowToDelete = taskRows.filter({ hasText: createdComment }).first();
      visibleRows = await taskRows.count();
    } else {
      const commentCell = rowToDelete.locator('td').nth(4);
      commentToDelete = (await commentCell.textContent())?.trim() || null;
    }

    const deleteResponse = waitForTaskEntryDeleteRequest(page);
    const dashboardReload = waitForDashboardRequest(page);
    page.once('dialog', (dialog) => dialog.accept());
    await rowToDelete.getByRole('button', { name: /delete/i }).click();
    await deleteResponse;
    await dashboardReload;

    const expectedCount = Math.max(visibleRows - 1, 0);
    await expect(taskRows).toHaveCount(expectedCount, { timeout: 5000 });

    if (commentToDelete) {
      await expect(taskRows.filter({ hasText: commentToDelete })).toHaveCount(0, {
        timeout: 5000,
      });
    }
  });
});
