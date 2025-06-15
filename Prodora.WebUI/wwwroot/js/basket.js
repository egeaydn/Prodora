document.addEventListener('DOMContentLoaded', function () {
    // Enhanced cart functionality
    const cartContainer = document.querySelector('.cart-section');
    const loadingOverlay = document.getElementById('cartLoading');
    const toast = document.getElementById('toast');

    // Delete button functionality with enhanced UX
    const deleteForms = document.querySelectorAll('.delete-form');
    deleteForms.forEach(form => {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            const button = this.querySelector('.delete-btn');
            const row = this.closest('tr');
            const productName = row.querySelector('.product-name').textContent;

            // Show confirmation
            if (confirm(`"${productName}" ürününü sepetten çıkarmak istediğinizden emin misiniz?`)) {
                // Show loading state
                button.classList.add('btn-loading');
                button.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
                button.disabled = true;

                if (loadingOverlay) {
                    loadingOverlay.classList.add('active');
                }

                // Animate row removal
                row.style.opacity = '0.5';
                row.style.transform = 'scale(0.95)';

                // Submit form after animation
                setTimeout(() => {
                    this.submit();
                }, 300);
            }
        });
    });

    // Image hover effects
    const productImages = document.querySelectorAll('.table img');
    productImages.forEach(img => {
        img.addEventListener('click', function () {
            // Simple image preview effect
            this.style.transform = 'scale(2)';
            this.style.zIndex = '1000';
            this.style.position = 'relative';

            setTimeout(() => {
                this.style.transform = '';
                this.style.zIndex = '';
                this.style.position = '';
            }, 1500);
        });
    });

    // Enhanced hover effects for table rows
    const tableRows = document.querySelectorAll('.table tbody tr');
    tableRows.forEach(row => {
        row.addEventListener('mouseenter', function () {
            this.style.background = 'linear-gradient(135deg, #f8f9fa, #e9ecef)';
            this.style.transform = 'translateY(-2px)';
            this.style.boxShadow = '0 4px 8px rgba(0, 0, 0, 0.15)';
        });

        row.addEventListener('mouseleave', function () {
            this.style.background = '';
            this.style.transform = '';
            this.style.boxShadow = '';
        });
    });

    // Toast notification function
    function showToast(message, type = 'success') {
        toast.textContent = message;
        toast.className = `toast ${type} show`;

        setTimeout(() => {
            toast.classList.remove('show');
        }, 3000);
    }

    // Enhanced button effects
    const buttons = document.querySelectorAll('.btn-primary');
    buttons.forEach(button => {
        button.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-2px) scale(1.02)';
        });

        button.addEventListener('mouseleave', function () {
            this.style.transform = '';
        });

        button.addEventListener('click', function () {
            this.style.transform = 'scale(0.98)';
            setTimeout(() => {
                this.style.transform = '';
            }, 100);
        });
    });

    // Quantity badge animations
    const quantityBadges = document.querySelectorAll('.quantity-badge');
    quantityBadges.forEach(badge => {
        badge.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.2) rotate(5deg)';
        });

        badge.addEventListener('mouseleave', function () {
            this.style.transform = '';
        });
    });

    // Price highlighting effect
    const prices = document.querySelectorAll('.total-price');
    prices.forEach(price => {
        price.addEventListener('mouseenter', function () {
            this.style.color = '#e74c3c';
            this.style.fontWeight = '800';
            this.style.fontSize = '20px';
        });

        price.addEventListener('mouseleave', function () {
            this.style.color = '';
            this.style.fontWeight = '';
            this.style.fontSize = '';
        });
    });

    // Auto-refresh cart badge if exists
    function updateCartBadge() {
        const cartBadge = document.querySelector('.cart-badge');
        if (cartBadge) {
            const itemCount = document.querySelectorAll('.table tbody tr').length;
            cartBadge.textContent = itemCount;

            if (itemCount === 0) {
                cartBadge.style.display = 'none';
            }
        }
    }

    // Initialize cart badge
    updateCartBadge();

    // Smooth scroll for action buttons
    const actionButtons = document.querySelectorAll('.action-buttons a');
    actionButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            if (this.getAttribute('href') === '/') {
                e.preventDefault();
                showToast('Alışverişe devam ediliyor...', 'info');
                setTimeout(() => {
                    window.location.href = '/';
                }, 500);
            } else if (this.getAttribute('href') === '/checkout') {
                showToast('Ödeme sayfasına yönlendiriliyor...', 'info');
            }
        });
    });

    // Add entrance animations
    const sections = document.querySelectorAll('.cart-section, .summary-section');
    sections.forEach((section, index) => {
        section.style.opacity = '0';
        section.style.transform = 'translateY(30px)';

        setTimeout(() => {
            section.style.transition = 'all 0.6s ease';
            section.style.opacity = '1';
            section.style.transform = 'translateY(0)';
        }, index * 200);
    });
});