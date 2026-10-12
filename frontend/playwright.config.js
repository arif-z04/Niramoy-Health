import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  timeout: 30000,
  expect: {
    timeout: 5000,
  },
  use: {
    baseURL: 'http://127.0.0.1:5173',
    browserName: 'chromium',
    launchOptions: {
      executablePath: '/usr/bin/google-chrome-stable',
      args: ['--no-sandbox', '--disable-setuid-sandbox', '--disable-gpu'],
    },
    trace: 'on-first-retry',
  },
  webServer: [
    {
      command: 'dotnet run --project ../backend/src/NiramoyHealth.Api --urls http://127.0.0.1:5131',
      port: 5131,
      reuseExistingServer: true,
      timeout: 30000,
    },
    {
      command: 'npm run dev -- --host 127.0.0.1 --port 5173',
      port: 5173,
      reuseExistingServer: true,
      timeout: 30000,
    },
  ],
});
