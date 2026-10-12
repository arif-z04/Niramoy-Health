import { chromium } from '@playwright/test';

const VIEWPORTS = [
  { name: 'Small mobile', width: 320, height: 568 },
  { name: 'Standard mobile', width: 360, height: 640 },
  { name: 'Mobile', width: 375, height: 667 },
  { name: 'Large mobile', width: 430, height: 932 },
  { name: 'Small tablet', width: 600, height: 960 },
  { name: 'Tablet', width: 768, height: 1024 },
  { name: 'Small laptop', width: 1024, height: 768 },
  { name: 'Laptop', width: 1280, height: 800 },
  { name: 'Desktop', width: 1440, height: 900 },
  { name: 'Large desktop', width: 1920, height: 1080 },
];

const ROUTES = [
  { name: 'Home', path: '#/' },
  { name: 'Catalog', path: '#/products' },
  { name: 'Product Details', path: '#/products/napa-extra-tablet' },
  { name: 'Login', path: '#/login' },
  { name: 'Register', path: '#/register' },
  { name: 'Prescription Upload', path: '#/prescriptions/upload' },
  { name: 'Support', path: '#/support' },
];

async function runAudit() {
  const browser = await chromium.launch({
    executablePath: '/usr/bin/google-chrome-stable',
    args: ['--no-sandbox', '--disable-setuid-sandbox', '--disable-gpu'],
  });

  console.log('--- STARTING AUDIT ACROSS 10 VIEWPORTS ---');

  for (const vp of VIEWPORTS) {
    const page = await browser.newPage({ viewport: { width: vp.width, height: vp.height } });

    for (const route of ROUTES) {
      await page.goto(`http://127.0.0.1:5173/${route.path}`);
      await page.waitForTimeout(600);

      const auditData = await page.evaluate(() => {
        const docScrollWidth = document.documentElement.scrollWidth;
        const winWidth = window.innerWidth;
        const bodyScrollWidth = document.body.scrollWidth;
        const hasHorizontalOverflow = docScrollWidth > winWidth || bodyScrollWidth > winWidth;

        // Find overflowing elements
        const overflowingElements = [];
        const allElements = document.querySelectorAll('*');
        for (const el of allElements) {
          const rect = el.getBoundingClientRect();
          if (rect.right > winWidth + 1 && rect.width > 0 && rect.height > 0) {
            overflowingElements.push({
              tag: el.tagName.toLowerCase(),
              id: el.id,
              className: (el.className || '').toString().slice(0, 60),
              right: Math.round(rect.right),
              width: Math.round(rect.width),
            });
          }
        }

        // Check input font sizes (< 16px on mobile viewports < 768px causes iOS auto-zoom)
        const smallInputs = [];
        if (winWidth < 768) {
          const inputs = document.querySelectorAll('input:not([type="checkbox"]):not([type="radio"]):not([type="hidden"]), select, textarea');
          for (const input of inputs) {
            const style = window.getComputedStyle(input);
            const fontSize = parseFloat(style.fontSize);
            if (fontSize < 16) {
              smallInputs.push({
                tag: input.tagName.toLowerCase(),
                id: input.id,
                fontSize: `${fontSize}px`,
              });
            }
          }
        }

        return {
          winWidth,
          docScrollWidth,
          hasHorizontalOverflow,
          overflowCount: overflowingElements.length,
          overflowSample: overflowingElements.slice(0, 4),
          smallInputsCount: smallInputs.length,
          smallInputsSample: smallInputs.slice(0, 3),
        };
      });

      if (auditData.hasHorizontalOverflow) {
        console.log(`[OVERFLOW] ${vp.name} (${vp.width}px) on ${route.name}: doc=${auditData.docScrollWidth}px > win=${auditData.winWidth}px`);
        if (auditData.overflowSample.length > 0) {
          console.log('   Elements:', JSON.stringify(auditData.overflowSample));
        }
      }

      if (auditData.smallInputsCount > 0 && vp.width < 768) {
        console.log(`[IOS ZOOM RISK] ${vp.name} (${vp.width}px) on ${route.name}: ${auditData.smallInputsCount} inputs < 16px`);
      }
    }

    await page.close();
  }

  await browser.close();
  console.log('--- AUDIT COMPLETED ---');
}

runAudit().catch(console.error);
