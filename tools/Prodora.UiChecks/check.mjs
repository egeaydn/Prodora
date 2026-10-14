import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { resolve } from 'node:path';

const root = fileURLToPath(new URL('../../', import.meta.url));
const output = resolve(root, 'artifacts/ui-previews');
process.env.PLAYWRIGHT_BROWSERS_PATH ??= resolve(root, 'artifacts/browser-cache');
const base = process.env.PRODORA_UI_BASE_URL ?? 'http://localhost:5097';
assert.ok(['localhost', '127.0.0.1', '[::1]'].includes(new URL(base).hostname), 'UI checks run only against a local development server.');
const { chromium } = await import('playwright');
const browser = await chromium.launch();
const errors = [];
let checks = 0;

async function check(label, action) {
    await action();
    checks++;
    console.log(`PASS ${label}`);
}

try {
    await mkdir(output, { recursive: true });
    const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
    context.setDefaultTimeout(15000);
    context.setDefaultNavigationTimeout(15000);
    const page = await context.newPage();
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => {
        if (message.type() === 'error' && /content.security.policy|violates.*directive/i.test(message.text())) errors.push(message.text());
    });

    await check('desktop header, footer and local resources render', async () => {
        const response = await page.goto(base);
        assert.equal(response.status(), 200);
        assert.equal(await page.locator('[data-site-header]').count(), 1);
        assert.equal(await page.locator('.footer-group').count(), 3);
        assert.equal(await page.locator('[data-menu-toggle]').isVisible(), false);
        assert.equal(await page.locator('.header-nav').isVisible(), true);
        const heights = await page.locator('.header-action').evaluateAll(links => links.map(link => link.getBoundingClientRect().height));
        assert.ok(heights.every(height => height >= 44));
        assert.equal(await page.locator('.footer-group').evaluateAll(groups => groups.every(group => group.open)), true);
        await page.screenshot({ path: resolve(output, 'navbar-footer-desktop.png'), fullPage: true });
    });

    await check('scroll changes the header surface without resizing it', async () => {
        const height = await page.locator('.site-header').evaluate(el => el.getBoundingClientRect().height);
        await page.evaluate(() => window.scrollTo({ top: 450, behavior: 'instant' }));
        await page.waitForFunction(() => document.querySelector('.site-header').classList.contains('is-scrolled'));
        const scrolled = await page.locator('.site-header').evaluate(el => ({ height: el.getBoundingClientRect().height, top: el.getBoundingClientRect().top, blur: getComputedStyle(el).backdropFilter }));
        assert.equal(scrolled.height, height);
        assert.equal(scrolled.top, 0);
        assert.equal(scrolled.blur, 'blur(12px)');
        await page.screenshot({ path: resolve(output, 'navbar-scrolled.png') });
    });

    for (const width of [320, 375, 600, 768, 850, 851, 1024, 1440]) {
        await check(`no horizontal overflow or missing navigation at ${width}px`, async () => {
            await page.setViewportSize({ width, height: 900 });
            await page.goto(base);
            await page.waitForFunction(() => document.documentElement.classList.contains('navigation-ready'));
            const overflow = await page.evaluate(() => document.documentElement.scrollWidth > window.innerWidth);
            assert.equal(overflow, false);
            assert.equal(await page.locator('[data-menu-toggle]').isVisible(), width <= 850);
            assert.equal(await page.locator('.header-nav').isVisible(), width > 850);
        });
    }

    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto(base);
    const toggle = page.locator('[data-menu-toggle]');
    const dialog = page.locator('[data-mobile-menu]');

    await check('mobile dialog opens, locks scrolling and contains keyboard focus', async () => {
        await toggle.click();
        await page.waitForFunction(() => document.querySelector('[data-mobile-menu]').classList.contains('is-open'));
        assert.equal(await toggle.getAttribute('aria-expanded'), 'true');
        assert.equal(await page.locator('body').evaluate(el => getComputedStyle(el).overflow), 'hidden');
        for (let i = 0; i < 14; i++) {
            await page.keyboard.press('Tab');
            assert.equal(await dialog.evaluate(el => el.contains(document.activeElement)), true);
        }
        await page.screenshot({ path: resolve(output, 'mobile-menu.png') });
    });

    await check('Escape closes the dialog and returns focus to its opener', async () => {
        await page.keyboard.press('Escape');
        await page.waitForFunction(() => !document.querySelector('[data-mobile-menu]').open);
        assert.equal(await toggle.getAttribute('aria-expanded'), 'false');
        assert.equal(await toggle.evaluate(el => document.activeElement === el), true);
        assert.equal(await page.locator('body').evaluate(el => el.classList.contains('menu-open')), false);
    });

    await check('backdrop click closes the mobile menu', async () => {
        await toggle.click();
        await page.waitForFunction(() => document.querySelector('[data-mobile-menu]').classList.contains('is-open'));
        await page.mouse.click(8, 200);
        await page.waitForFunction(() => !document.querySelector('[data-mobile-menu]').open);
    });

    await check('desktop resize closes a previously open mobile menu', async () => {
        await toggle.click();
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.waitForFunction(() => !document.querySelector('[data-mobile-menu]').open);
        assert.equal(await page.locator('body').evaluate(el => el.classList.contains('menu-open')), false);
    });

    await check('footer groups toggle on mobile and expand on desktop', async () => {
        await page.setViewportSize({ width: 375, height: 812 });
        await page.goto(base);
        assert.equal(await page.locator('.footer-group').evaluateAll(groups => groups.every(group => !group.open)), true);
        const group = page.locator('.footer-group').first();
        await group.locator('summary').click();
        await page.waitForFunction(() => document.querySelector('.footer-group').open && !document.querySelector('.footer-group').classList.contains('is-animating'));
        assert.equal(await group.locator('.footer-links').isVisible(), true);
        await page.screenshot({ path: resolve(output, 'footer-mobile.png') });
        await group.locator('summary').focus();
        await page.keyboard.press('Enter');
        await page.waitForFunction(() => !document.querySelector('.footer-group').open);
        assert.equal(await group.locator('.footer-links').isVisible(), false);
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.waitForFunction(() => [...document.querySelectorAll('.footer-group')].every(el => el.open));
    });

    await check('reduced motion disables menu transitions', async () => {
        await page.emulateMedia({ reducedMotion: 'reduce' });
        await page.setViewportSize({ width: 375, height: 812 });
        await page.goto(base);
        await toggle.click();
        assert.equal(await page.locator('.mobile-menu-panel').evaluate(el => getComputedStyle(el).transitionDuration), '0s');
        await page.locator('[data-menu-close]').click();
        assert.equal(await dialog.evaluate(el => el.open), false);
        await page.screenshot({ path: resolve(output, 'navbar-footer-mobile.png'), fullPage: true });
    });

    await check('search still submits a GET query and marks the shop as current', async () => {
        await page.locator('#store-search').fill('Test arama');
        await page.locator('.header-search button').click();
        await page.waitForURL(url => url.pathname === '/products' && url.searchParams.get('q') === 'Test arama');
        assert.equal(await page.locator('.main-menu-link[aria-current="page"]').first().getAttribute('href'), '/products');
    });

    await check('shared chrome renders on account, product and information pages', async () => {
        for (const path of ['/Account/Login', '/products', '/Tools/About']) {
            const response = await page.goto(new URL(path, base).href);
            assert.equal(response.status(), 200);
            assert.equal(await page.locator('[data-site-header]').count(), 1);
            assert.equal(await page.locator('footer.site-footer').count(), 1);
        }
    });

    await check('navigation remains accessible without JavaScript', async () => {
        const fallback = await browser.newContext({ javaScriptEnabled: false, viewport: { width: 375, height: 812 } });
        const noJs = await fallback.newPage();
        await noJs.goto(base);
        assert.equal(await noJs.locator('.header-nav').isVisible(), true);
        assert.equal(await noJs.locator('[data-menu-toggle]').isVisible(), false);
        assert.equal(await noJs.locator('.footer-group').evaluateAll(groups => groups.every(group => group.open)), true);
        assert.equal(await noJs.evaluate(() => document.documentElement.scrollWidth > window.innerWidth), false);
        await fallback.close();
    });

    await check('shared layouts stay within the viewport across account, catalog and help pages', async () => {
        for (const width of [320, 375, 768, 1440]) {
            await page.setViewportSize({ width, height: 900 });
            for (const path of ['/Account/Login', '/Account/Register', '/Account/ForgotPassword', '/products', '/Tools/About', '/Tools/Help', '/Tools/Faq']) {
                await page.goto(new URL(path, base).href);
                assert.equal(await page.evaluate(() => document.documentElement.scrollWidth > window.innerWidth), false, `${path} overflows at ${width}px`);
            }
        }
    });

    await check('product cards align their prices and respond to pointer and keyboard interaction', async () => {
        await page.emulateMedia({ reducedMotion: 'no-preference' });
        await page.setViewportSize({ width: 1440, height: 900 });
        await page.goto(base);
        const cards = page.locator('.product-card');
        assert.ok(await cards.count() >= 3, 'Card alignment checks need at least three products on the home page.');
        const prices = await cards.evaluateAll(elements => elements.slice(0, 3).map(el => el.querySelector('.product-price').getBoundingClientRect().top));
        assert.ok(Math.max(...prices) - Math.min(...prices) < 1);
        assert.equal(await cards.first().evaluate(el => getComputedStyle(el).borderRadius), '14px');
        await cards.first().hover();
        await page.waitForFunction(() => getComputedStyle(document.querySelector('.product-card')).transform === 'matrix(1, 0, 0, 1, 0, -2)');
        assert.notEqual(await cards.first().evaluate(el => getComputedStyle(el).boxShadow), 'none');
        await page.screenshot({ path: resolve(output, 'product-card-hover.png') });
        await page.mouse.move(0, 0);
        await cards.first().locator('h3 a').focus();
        assert.equal(await cards.first().evaluate(el => el.matches(':focus-within')), true);
        await page.emulateMedia({ reducedMotion: 'reduce' });
        await cards.first().hover();
        assert.equal(await cards.first().evaluate(el => getComputedStyle(el).transform), 'none');
        assert.equal(await cards.first().locator('img').evaluate(el => getComputedStyle(el).transform), 'none');
    });

    await check('information panels share card corners without lifting', async () => {
        await page.goto(new URL('/Tools/Help', base).href);
        const panel = page.locator('.info-card').first();
        assert.equal(await panel.evaluate(el => getComputedStyle(el).borderRadius), '14px');
        await panel.hover();
        assert.equal(await panel.evaluate(el => getComputedStyle(el).transform), 'none');
    });

    await check('buttons share touch dimensions, radius and a visible keyboard focus', async () => {
        await page.goto(new URL('/Account/Login', base).href);
        const button = page.locator('button.btn[type="submit"]');
        const state = await button.evaluate(el => ({ height: el.getBoundingClientRect().height, radius: getComputedStyle(el).borderRadius }));
        assert.ok(state.height >= 44);
        assert.equal(state.radius, '10px');
        await page.keyboard.press('Tab');
        await button.focus();
        assert.equal(await button.evaluate(el => getComputedStyle(el).outlineStyle), 'solid');
        await button.evaluate(el => { el.disabled = true; });
        assert.equal(await button.evaluate(el => getComputedStyle(el).opacity), '0.5');
        await button.evaluate(el => { el.disabled = false; });
        await page.screenshot({ path: resolve(output, 'account-controls.png'), fullPage: true });
    });

    await check('POST feedback keeps button width and the submitter payload', async () => {
        await page.emulateMedia({ reducedMotion: 'reduce' });
        let release;
        const held = new Promise(done => { release = done; });
        let payload;
        await page.route('**/Account/Login', async route => {
            if (route.request().method() !== 'POST') return route.continue();
            payload = new URLSearchParams(route.request().postData());
            await held;
            await route.fulfill({ status: 200, contentType: 'text/html', body: '<p>Synthetic UI response</p>' });
        });
        await page.goto(new URL('/Account/Login', base).href);
        const button = page.locator('button.btn[type="submit"]');
        await button.evaluate(el => { el.name = 'intent'; el.value = 'login'; });
        await page.locator('#Email').fill('ui-check@example.test');
        await page.locator('#Password').fill('SyntheticUi123!');
        const width = await button.evaluate(el => el.getBoundingClientRect().width);
        const posted = page.waitForRequest(request => request.method() === 'POST' && new URL(request.url()).pathname === '/Account/Login');
        try {
            await button.click({ noWaitAfter: true });
            // Timer polling also works while Chromium is holding a document navigation.
            await page.waitForFunction(() => document.querySelector('button.btn').classList.contains('is-loading'), null, { polling: 50 });
            const busy = await page.evaluate(() => {
                const el = document.querySelector('button.btn[type="submit"]');
                return { width: el.getBoundingClientRect().width, ariaDisabled: el.getAttribute('aria-disabled'), disabled: el.disabled, forms: document.querySelectorAll('form[aria-busy="true"]').length };
            });
            assert.equal(busy.width, width);
            assert.equal(busy.ariaDisabled, 'true');
            assert.equal(busy.disabled, false);
            assert.equal(busy.forms, 1);
            assert.equal(await page.evaluate(() => document.querySelector('form [role="status"]')?.textContent), 'İşlemin yapılıyor…');
            assert.equal(await page.evaluate(() => getComputedStyle(document.querySelector('button.btn')).backgroundColor), 'rgb(32, 42, 45)');
            await page.screenshot({ path: resolve(output, 'button-loading.png'), timeout: 5000 });
            await posted;
            assert.equal(payload.get('intent'), 'login');
            assert.equal(payload.get('Email'), 'ui-check@example.test');
            assert.ok(payload.get('__RequestVerificationToken'));
        } finally {
            release();
        }
        await page.getByText('Synthetic UI response').waitFor();
        await page.unroute('**/Account/Login');
    });

    await check('invalid forms do not become busy', async () => {
        await page.goto(new URL('/Account/Login', base).href);
        await page.locator('button.btn[type="submit"]').click();
        assert.equal(await page.locator('.is-loading').count(), 0);
        assert.equal(await page.locator('form[aria-busy="true"]').count(), 0);
    });

    await check('no script errors or CSP violations', async () => assert.deepEqual(errors, []));
    console.log(`SUCCESS ${checks} browser checks. Screenshots: ${output}`);
} finally {
    await browser.close();
}
