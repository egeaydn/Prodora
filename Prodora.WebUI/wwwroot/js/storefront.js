document.documentElement.classList.add('js');
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('img').forEach(image => {
        const fallback = () => {
            if (image.dataset.fallbackApplied) return;
            image.dataset.fallbackApplied = 'true';
            image.src = '/img/product-placeholder.svg';
        };
        image.addEventListener('error', fallback);
        if (image.complete && image.naturalWidth === 0) fallback();
    });
    document.querySelectorAll('[data-auto-submit]').forEach(select => {
        select.addEventListener('change', () => select.form.requestSubmit());
    });
    document.querySelectorAll('[data-password-toggle]').forEach(button => {
        button.addEventListener('click', () => {
            const input = button.parentElement.querySelector('input');
            const reveal = input.type === 'password';
            input.type = reveal ? 'text' : 'password';
            button.textContent = reveal ? 'Gizle' : 'Göster';
            button.setAttribute('aria-label', reveal ? 'Şifreyi gizle' : 'Şifreyi göster');
        });
    });
    document.querySelectorAll('[data-gallery-image]').forEach(button => {
        button.addEventListener('click', () => {
            const main = document.getElementById('main-image');
            main.removeAttribute('data-fallback-applied');
            main.src = button.dataset.galleryImage;
            document.querySelectorAll('[data-gallery-image]').forEach(other => {
                other.classList.toggle('active', other === button);
                other.setAttribute('aria-pressed', String(other === button));
            });
        });
    });
    const tabs = [...document.querySelectorAll('[data-tab]')];
    const activateTab = tab => {
        tabs.forEach(other => {
            const active = tab === other;
            other.classList.toggle('active', active);
            other.setAttribute('aria-selected', String(active));
            other.tabIndex = active ? 0 : -1;
            document.getElementById(other.dataset.tab)?.classList.toggle('active', active);
        });
    };
    tabs.forEach((tab, index) => {
        tab.addEventListener('click', () => activateTab(tab));
        tab.addEventListener('keydown', event => {
            let next;
            if (event.key === 'ArrowRight') next = tabs[(index + 1) % tabs.length];
            if (event.key === 'ArrowLeft') next = tabs[(index + tabs.length - 1) % tabs.length];
            if (event.key === 'Home') next = tabs[0];
            if (event.key === 'End') next = tabs[tabs.length - 1];
            if (next) { event.preventDefault(); activateTab(next); next.focus(); }
        });
    });
    document.querySelectorAll('form[data-confirm]').forEach(form => {
        form.addEventListener('submit', event => {
            if (!window.confirm(form.dataset.confirm)) { event.preventDefault(); event.stopImmediatePropagation(); }
        });
    });
    document.querySelectorAll('form[data-comment-form]').forEach(form => {
        form.addEventListener('submit', async event => {
            if (event.defaultPrevented) return;
            event.preventDefault();
            const button = form.querySelector('button[type=submit]');
            const status = form.querySelector('[data-comment-status]');
            button.disabled = true;
            status.textContent = 'İşlemin yapılıyor…';
            try {
                const response = await fetch(form.action, { method: 'POST', body: new FormData(form), headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                if (!response.ok || !response.headers.get('content-type')?.includes('application/json')) throw new Error('İşlem tamamlanamadı. Giriş durumunu kontrol ederek tekrar dene.');
                const result = await response.json();
                if (!result.result) throw new Error(result.message || 'Yorum kaydedilemedi. Alanları kontrol et.');
                window.location.reload();
            } catch (error) { status.textContent = error.message; button.disabled = false; }
        });
    });
    const checkout = document.getElementById('checkout-form');
    if (checkout) {
        const syncPayment = () => {
            const credit = checkout.querySelector('[name=paymentMethod]:checked')?.value === 'credit';
            document.getElementById('payment-box').hidden = !credit;
            document.getElementById('eft-note').hidden = credit;
            checkout.querySelectorAll('[data-card-field]').forEach(input => { input.disabled = !credit; input.required = credit; });
        };
        checkout.querySelectorAll('[name=paymentMethod]').forEach(input => input.addEventListener('change', syncPayment));
        syncPayment();
        checkout.querySelector('[name=CardNumber]')?.addEventListener('input', event => {
            const digits = event.target.value.replace(/\D/g, '').slice(0, 16);
            event.target.value = digits.match(/.{1,4}/g)?.join(' ') || '';
        });
        checkout.addEventListener('submit', () => {
            const button = document.getElementById('checkout-submit');
            button.disabled = true;
            button.textContent = 'Sipariş işleniyor…';
        });
        window.addEventListener('pageshow', () => {
            const button = document.getElementById('checkout-submit');
            button.disabled = false;
            button.textContent = 'Test siparişini tamamla';
        });
    }
    document.querySelectorAll('[data-image-preview]').forEach(input => {
        let urls = [];
        input.addEventListener('change', () => {
            const preview = input.closest('aside')?.querySelector('[data-image-previews]');
            if (!preview) return;
            urls.forEach(URL.revokeObjectURL); urls = [];
            preview.replaceChildren();
            [...input.files].slice(0, 12).filter(file => file.type.startsWith('image/')).forEach(file => {
                const image = document.createElement('img');
                image.src = URL.createObjectURL(file); urls.push(image.src);
                image.alt = file.name;
                preview.append(image);
            });
        });
        window.addEventListener('pagehide', () => urls.forEach(URL.revokeObjectURL));
    });
    const faqSearch = document.querySelector('[data-faq-search]');
    faqSearch?.addEventListener('input', () => {
        const query = faqSearch.value.trim().toLocaleLowerCase('tr-TR');
        const questions = [...document.querySelectorAll('.faq-list details')];
        questions.forEach(item => item.hidden = !item.textContent.toLocaleLowerCase('tr-TR').includes(query));
        document.querySelector('[data-faq-empty]').hidden = questions.some(item => !item.hidden);
    });
});
