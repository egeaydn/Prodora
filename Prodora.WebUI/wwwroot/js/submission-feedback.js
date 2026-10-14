/* Opt-in feedback for ordinary POST forms. AJAX and checkout keep their own handlers. */
(() => {
    const pending = new Map();
    document.addEventListener('submit', event => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || !form.matches('[data-submit-feedback]')) return;
        if (pending.has(form)) { event.preventDefault(); return; }
        if (event.defaultPrevented || !form.checkValidity()) return;
        const button = event.submitter ?? form.querySelector('button.btn[type="submit"]');
        if (!button?.matches('.btn')) return;
        // Allow every validation listener to cancel the submission first.
        queueMicrotask(() => {
            if (event.defaultPrevented || pending.has(form)) return;
            const status = document.createElement('span');
            status.className = 'visually-hidden';
            status.setAttribute('role', 'status');
            status.textContent = 'İşlemin yapılıyor…';
            pending.set(form, { button, status, busy: form.getAttribute('aria-busy'), disabled: button.getAttribute('aria-disabled') });
            form.append(status);
            form.setAttribute('aria-busy', 'true');
            button.setAttribute('aria-disabled', 'true');
            button.classList.add('is-loading');
        });
    });
    window.addEventListener('pageshow', () => {
        pending.forEach(({ button, status, busy, disabled }, form) => {
            button.classList.remove('is-loading');
            if (disabled == null) button.removeAttribute('aria-disabled'); else button.setAttribute('aria-disabled', disabled);
            if (busy == null) form.removeAttribute('aria-busy'); else form.setAttribute('aria-busy', busy);
            status.remove();
        });
        pending.clear();
    });
})();
