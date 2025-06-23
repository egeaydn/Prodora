// Image gallery functionality
function changeMainImage(imageSrc, thumbnail) {
    const mainImage = document.getElementById('main-image');

    if (!mainImage) {
        console.error('Ana resim elementi bulunamadı');
        return;
    }

    // Remove active class from all thumbnails
    document.querySelectorAll('.thumbnail-item').forEach(item => {
        item.classList.remove('active');
    });

    // Add active class to clicked thumbnail
    if (thumbnail) {
        thumbnail.classList.add('active');
    }

    // Change main image with smooth transition
    mainImage.style.opacity = '0.7';
    mainImage.style.transition = 'opacity 0.3s ease';

    setTimeout(() => {
        mainImage.src = imageSrc;
        mainImage.style.opacity = '1';

        // Error handling for broken images
        mainImage.onerror = function () {
            console.error('Resim yüklenemedi:', imageSrc);
            this.src = '/img/placeholder.jpg';
            this.style.opacity = '1';
        };
    }, 150);
}

// Tab functionality
function showTab(tabName, clickedButton) {
    // Hide all tab panes
    document.querySelectorAll('.tab-pane').forEach(pane => {
        pane.classList.remove('active');
    });

    // Remove active class from all tab buttons
    document.querySelectorAll('.tab-button').forEach(button => {
        button.classList.remove('active');
    });

    // Show selected tab pane
    document.getElementById(tabName + '-tab').classList.add('active');

    // Add active class to clicked button
    clickedButton.classList.add('active');
}

// Enhanced form submission
document.addEventListener('DOMContentLoaded', function () {
    const forms = document.querySelectorAll('form[asp-controller="Basket"]');

    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const button = e.target.querySelector('button[type="submit"]');
            if (button) {
                const originalText = button.innerHTML;
                button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> İşlem yapılıyor...';
                button.disabled = true;

                // Reset after 3 seconds (adjust based on actual response)
                setTimeout(() => {
                    button.innerHTML = originalText;
                    button.disabled = false;
                }, 3000);
            }
        });
    });

    // Smooth scrolling for related products
    const productCards = document.querySelectorAll('.product-card');
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.animation = 'fadeIn 0.6s ease-out';
            }
        });
    }, observerOptions);

    productCards.forEach(card => {
        observer.observe(card);
    });
});