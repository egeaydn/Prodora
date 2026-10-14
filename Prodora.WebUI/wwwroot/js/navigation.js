/* Progressive enhancement for shared navigation; all links work without JavaScript. */
(() => {
    const header = document.querySelector('[data-site-header]');
    if (!header) return;

    let scrollFrame = 0;
    const syncScroll = () => {
        header.classList.toggle('is-scrolled', window.scrollY > 16);
        scrollFrame = 0;
    };
    window.addEventListener('scroll', () => {
        if (!scrollFrame) scrollFrame = window.requestAnimationFrame(syncScroll);
    }, { passive: true });
    syncScroll();

    // Sticky summaries and anchor targets use the real header height at every breakpoint.
    const syncHeight = () => document.documentElement.style.setProperty('--header-offset', `${Math.ceil(header.getBoundingClientRect().height)}px`);
    if ('ResizeObserver' in window) new ResizeObserver(syncHeight).observe(header);
    else window.addEventListener('resize', syncHeight, { passive: true });
    syncHeight();

    const dialog = document.querySelector('[data-mobile-menu]');
    const toggle = document.querySelector('[data-menu-toggle]');
    const closeButton = dialog?.querySelector('[data-menu-close]');
    const panel = dialog?.querySelector('.mobile-menu-panel');
    const mobileMenu = window.matchMedia('(max-width: 850px)');
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');

    if (dialog && toggle && closeButton && panel && typeof dialog.showModal === 'function') {
        let closeTimer;
        let closing = false;
        document.documentElement.classList.add('navigation-ready');

        const finishClose = () => {
            window.clearTimeout(closeTimer);
            dialog.classList.remove('is-open');
            if (dialog.open) dialog.close();
            document.body.classList.remove('menu-open');
            toggle.setAttribute('aria-expanded', 'false');
            closing = false;
        };

        const closeMenu = () => {
            if (!dialog.open || closing) return;
            closing = true;
            dialog.classList.remove('is-open');
            if (reducedMotion.matches) finishClose();
            else {
                // Safety fallback if a resize or browser interrupts transitionend.
                const duration = parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--duration-panel')) || 260;
                closeTimer = window.setTimeout(finishClose, duration + 60);
            }
        };

        toggle.addEventListener('click', () => {
            if (!mobileMenu.matches || dialog.open) return;
            closing = false;
            dialog.showModal();
            document.body.classList.add('menu-open');
            toggle.setAttribute('aria-expanded', 'true');
            window.requestAnimationFrame(() => {
                if (dialog.open && !closing) dialog.classList.add('is-open');
            });
        });
        closeButton.addEventListener('click', closeMenu);
        dialog.addEventListener('keydown', event => {
            if (event.key !== 'Tab') return;
            const controls = [...dialog.querySelectorAll('a[href], button:not([disabled]), [tabindex="0"]')]
                .filter(element => element.getClientRects().length > 0);
            const first = controls[0];
            const last = controls[controls.length - 1];
            if (event.shiftKey && (document.activeElement === first || !dialog.contains(document.activeElement))) {
                event.preventDefault();
                last?.focus();
            } else if (!event.shiftKey && (document.activeElement === last || !dialog.contains(document.activeElement))) {
                event.preventDefault();
                first?.focus();
            }
        });
        dialog.addEventListener('cancel', event => { event.preventDefault(); closeMenu(); });
        dialog.addEventListener('click', event => {
            if (event.target === dialog) closeMenu();
            if (event.target.closest('a')) finishClose();
        });
        panel.addEventListener('transitionend', event => {
            if (closing && event.target === panel && event.propertyName === 'transform') finishClose();
        });
        dialog.addEventListener('close', () => {
            document.body.classList.remove('menu-open');
            toggle.setAttribute('aria-expanded', 'false');
        });
        mobileMenu.addEventListener('change', () => { if (!mobileMenu.matches) finishClose(); });
        window.addEventListener('pagehide', finishClose);
        window.addEventListener('pageshow', () => { finishClose(); syncScroll(); syncHeight(); });
    }

    const footerGroups = [...document.querySelectorAll('[data-footer-group]')];
    const mobileFooter = window.matchMedia('(max-width: 600px)');
    const mobileState = new WeakMap(footerGroups.map(group => [group, false]));
    const footerAnimations = new WeakMap();
    const stopFooterAnimation = group => {
        footerAnimations.get(group)?.cancel();
        footerAnimations.delete(group);
        group.classList.remove('is-animating', 'is-collapsing');
    };
    const toggleFooterGroup = (group, open) => {
        const links = group.querySelector('.footer-links');
        const from = links.getBoundingClientRect().height;
        stopFooterAnimation(group);
        if (reducedMotion.matches || typeof links.animate !== 'function') {
            group.open = open;
            return;
        }
        if (open) group.open = true;
        group.classList.add('is-animating');
        group.classList.toggle('is-collapsing', !open);
        const tokens = getComputedStyle(document.documentElement);
        const animation = links.animate(
            [{ height: `${from}px`, opacity: open ? .65 : 1 }, { height: `${open ? links.scrollHeight : 0}px`, opacity: open ? 1 : 0 }],
            { duration: parseFloat(tokens.getPropertyValue('--duration-base')) || 180, easing: tokens.getPropertyValue('--ease-out').trim() }
        );
        footerAnimations.set(group, animation);
        animation.onfinish = () => {
            if (footerAnimations.get(group) !== animation) return;
            group.open = open;
            footerAnimations.delete(group);
            group.classList.remove('is-animating', 'is-collapsing');
        };
    };
    const syncFooter = () => {
        footerGroups.forEach(group => {
            stopFooterAnimation(group);
            const summary = group.querySelector('summary');
            group.open = mobileFooter.matches ? mobileState.get(group) : true;
            summary.tabIndex = mobileFooter.matches ? 0 : -1;
            if (mobileFooter.matches) summary.removeAttribute('aria-disabled');
            else summary.setAttribute('aria-disabled', 'true');
        });
    };
    footerGroups.forEach(group => {
        group.querySelector('summary').addEventListener('click', event => {
            event.preventDefault();
            if (!mobileFooter.matches) return;
            const open = !mobileState.get(group);
            mobileState.set(group, open);
            toggleFooterGroup(group, open);
        });
    });
    mobileFooter.addEventListener('change', syncFooter);
    syncFooter();
})();
