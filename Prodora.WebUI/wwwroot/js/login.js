document.addEventListener('DOMContentLoaded', function () {
    // Input alanlarına odaklanınca label animasyonu
    const inputs = document.querySelectorAll('.form-group input');
    inputs.forEach(input => {
        input.addEventListener('focus', function () {
            this.parentNode.querySelector('label').style.color = 'var(--primary-color)';
        });

        input.addEventListener('blur', function () {
            if (!this.value) {
                this.parentNode.querySelector('label').style.color = '';
            }
        });
    });

    // Buton üzerinde mouse hareketi efekti
    const loginButton = document.querySelector('.login-button');
    if (loginButton) {
        loginButton.addEventListener('mousemove', function (e) {
            const rect = this.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            this.style.setProperty('--mouse-x', `${x}px`);
            this.style.setProperty('--mouse-y', `${y}px`);
        });

        loginButton.addEventListener('mouseleave', function () {
            this.style.removeProperty('--mouse-x');
            this.style.removeProperty('--mouse-y');
        });
    }
});