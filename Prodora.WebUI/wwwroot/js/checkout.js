document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('checkoutForm');
    const submitBtn = document.getElementById('submitBtn');
    const cardNumberInput = document.querySelector('input[name="CardNumber"]');
    const paymentBox = document.getElementById('payment-box');

    // Card number formatting
    if (cardNumberInput) {
        cardNumberInput.addEventListener('input', function (e) {
            let value = e.target.value.replace(/\s/g, '').replace(/[^0-9]/gi, '');
            let formattedInputValue = value.match(/.{1,4}/g)?.join(' ') || value;
            if (formattedInputValue.length <= 19) {
                this.value = formattedInputValue;
            }
        });
    }

    // Form validation and submission
    form.addEventListener('submit', function (e) {
        e.preventDefault();

        // Show loading state
        submitBtn.classList.add('loading-state');
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> İşleniyor...';
        submitBtn.disabled = true;

        // Simulate processing time
        setTimeout(() => {
            this.submit();
        }, 1500);
    });

    // Payment method toggle
    window.PaymentMethodChangeEvent = function (method) {
        if (method === 'credit') {
            paymentBox.style.display = 'block';
            paymentBox.classList.add('active');
        } else {
            paymentBox.style.display = 'none';
            paymentBox.classList.remove('active');
        }
    };

    // Enhanced form validation
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(input => {
        input.addEventListener('blur', function () {
            if (this.hasAttribute('required') && !this.value.trim()) {
                this.style.borderColor = 'var(--danger-color)';
            } else {
                this.style.borderColor = 'var(--border-color)';
            }
        });

        input.addEventListener('focus', function () {
            this.style.borderColor = 'var(--primary-color)';
        });
    });

    // Card CVV input restriction
    const cvvInput = document.querySelector('input[name="CVV"]');
    if (cvvInput) {
        cvvInput.addEventListener('input', function (e) {
            this.value = this.value.replace(/[^0-9]/g, '');
        });
    }

    // Month/Year validation
    const monthInput = document.querySelector('input[name="ExpirationMonth"]');
    const yearInput = document.querySelector('input[name="ExpirationYear"]');

    if (monthInput) {
        monthInput.addEventListener('input', function (e) {
            let value = this.value.replace(/[^0-9]/g, '');
            if (value > 12) value = '12';
            if (value.length === 1 && value > 1) value = '0' + value;
            this.value = value;
        });
    }

    if (yearInput) {
        yearInput.addEventListener('input', function (e) {
            this.value = this.value.replace(/[^0-9]/g, '');
        });
    }

    // Phone number formatting
    const phoneInput = document.querySelector('input[name="Phone"]');
    if (phoneInput) {
        phoneInput.addEventListener('input', function (e) {
            this.value = this.value.replace(/[^0-9+()-\s]/g, '');
        });
    }

    // Entrance animation for form sections
    const sections = document.querySelectorAll('.col-md-8, .col-md-4');
    sections.forEach((section, index) => {
        section.style.opacity = '0';
        section.style.transform = 'translateY(30px)';

        setTimeout(() => {
            section.style.transition = 'all 0.6s ease';
            section.style.opacity = '1';
            section.style.transform = 'translateY(0)';
        }, index * 200 + 300);
    });
});