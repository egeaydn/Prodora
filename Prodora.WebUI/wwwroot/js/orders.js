document.addEventListener('DOMContentLoaded', function () {
    // Enhanced collapse animation
    const collapseElements = document.querySelectorAll('.collapse');

    collapseElements.forEach(element => {
        element.addEventListener('show.bs.collapse', function () {
            this.style.transition = 'height 0.35s ease';
        });

        element.addEventListener('hide.bs.collapse', function () {
            this.style.transition = 'height 0.35s ease';
        });
    });

    // Card hover effects
    const orderCards = document.querySelectorAll('.order-card');

    orderCards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-3px)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });

    // Status color enhancement
    const statusElements = document.querySelectorAll('.order-status');

    statusElements.forEach(status => {
        const statusText = status.textContent.toLowerCase().trim();

        switch (statusText) {
            case 'pending':
            case 'beklemede':
                status.classList.add('status-pending');
                break;
            case 'processing':
            case 'işleniyor':
                status.classList.add('status-processing');
                break;
            case 'completed':
            case 'tamamlandı':
                status.classList.add('status-completed');
                break;
            case 'cancelled':
            case 'iptal edildi':
                status.classList.add('status-cancelled');
                break;
        }
    });
});