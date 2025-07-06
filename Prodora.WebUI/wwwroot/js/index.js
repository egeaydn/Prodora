// =====================================
// PRODORA HOME INDEX - JAVASCRIPT
// =====================================

document.addEventListener('DOMContentLoaded', function() {
    initializeHomePage();
});

function initializeHomePage() {
    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });

    // Search functionality
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        searchInput.addEventListener('input', debounce(handleSearch, 300));
    }

    // Filter functionality
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            filterProducts(this.dataset.filter);
        });
    });

    // View toggle functionality
    document.querySelectorAll('.view-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            document.querySelectorAll('.view-btn').forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            toggleView(this.dataset.view);
        });
    });

    // Category search
    const categorySearch = document.querySelector('.category-search-input');
    if (categorySearch) {
        categorySearch.addEventListener('input', debounce(filterCategories, 200));
    }

    // Newsletter form
    const newsletterForm = document.querySelector('.newsletter-form');
    if (newsletterForm) {
        newsletterForm.addEventListener('submit', handleNewsletterSubmit);
    }

    // Initialize animations
    initializeAnimations();
    
    // Initialize stats counter
    initializeStatsCounter();
}

function handleSearch(e) {
    const query = e.target.value.toLowerCase();
    const products = document.querySelectorAll('.product-wrapper');
    
    products.forEach(product => {
        const productName = product.querySelector('.product-title')?.textContent.toLowerCase() || '';
        const productDescription = product.querySelector('.product-description')?.textContent.toLowerCase() || '';
        
        if (productName.includes(query) || productDescription.includes(query)) {
            product.style.display = 'block';
            product.classList.add('search-match');
        } else {
            product.style.display = 'none';
            product.classList.remove('search-match');
        }
    });
    
    updateProductsCount();
}

function filterProducts(filter) {
    const products = document.querySelectorAll('.product-wrapper');
    
    products.forEach(product => {
        let show = true;
        
        switch(filter) {
            case 'new':
                show = product.querySelector('.new-badge') !== null;
                break;
            case 'sale':
                show = product.querySelector('.sale-badge') !== null;
                break;
            case 'popular':
                show = product.querySelector('.popular-badge') !== null;
                break;
            case 'all':
            default:
                show = true;
                break;
        }
        
        product.style.display = show ? 'block' : 'none';
    });
    
    updateProductsCount();
}

function toggleView(view) {
    const container = document.getElementById('productsContainer');
    if (container) {
        container.className = container.className.replace(/view-\w+/g, '');
        container.classList.add(`view-${view}`);
    }
}

function filterCategories(e) {
    const query = e.target.value.toLowerCase();
    const categories = document.querySelectorAll('.category-list li');
    
    categories.forEach(category => {
        const categoryName = category.textContent.toLowerCase();
        category.style.display = categoryName.includes(query) ? 'block' : 'none';
    });
}

function toggleSidebar() {
    const sidebar = document.querySelector('.category-sidebar');
    if (sidebar) {
        sidebar.classList.toggle('collapsed');
    }
}

function loadMoreProducts() {
    // Simulate loading more products
    showNotification('Daha fazla ürün yükleniyor...', 'info');
    
    setTimeout(() => {
        showNotification('Tüm ürünler yüklendi!', 'success');
    }, 1500);
}

function handleNewsletterSubmit(e) {
    e.preventDefault();
    const email = e.target.querySelector('.newsletter-input').value;
    
    if (validateEmail(email)) {
        showNotification('Başarıyla abone oldunuz!', 'success');
        e.target.querySelector('.newsletter-input').value = '';
    } else {
        showNotification('Geçerli bir e-posta adresi girin.', 'error');
    }
}

function validateEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function updateProductsCount() {
    const visibleProducts = document.querySelectorAll('.product-wrapper[style="display: block"], .product-wrapper:not([style])').length;
    const countElement = document.querySelector('.products-count');
    if (countElement) {
        countElement.textContent = `${visibleProducts} ürün gösteriliyor`;
    }
}

function initializeAnimations() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
            }
        });
    }, observerOptions);

    document.querySelectorAll('.product-wrapper, .feature-item, .stat-item').forEach(el => {
        observer.observe(el);
    });
}

function initializeStatsCounter() {
    const stats = document.querySelectorAll('.stat-number');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateCounter(entry.target);
                observer.unobserve(entry.target);
            }
        });
    });

    stats.forEach(stat => observer.observe(stat));
}

function animateCounter(element) {
    const target = element.textContent;
    const number = parseInt(target.replace(/\D/g, ''));
    const suffix = target.replace(/[\d,]/g, '');
    let current = 0;
    const increment = number / 50;
    
    const timer = setInterval(() => {
        current += increment;
        if (current >= number) {
            element.textContent = number.toLocaleString() + suffix;
            clearInterval(timer);
        } else {
            element.textContent = Math.floor(current).toLocaleString() + suffix;
        }
    }, 30);
}

function showNotification(message, type) {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <span class="notification-icon">
                ${type === 'success' ? '<i class="fas fa-check-circle"></i>' : 
                  type === 'error' ? '<i class="fas fa-exclamation-circle"></i>' : 
                  '<i class="fas fa-info-circle"></i>'}
            </span>
            <span class="notification-text">${message}</span>
            <button class="notification-close" onclick="this.parentElement.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 4000);
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Add to cart functionality
function addToCart(productId) {
    // Show loading state
    showNotification('Ürün sepete ekleniyor...', 'info');

    // Simulate API call
    fetch('/Basket/AddToBasket', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        },
        body: JSON.stringify({ productId: productId, quantity: 1 })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification('Ürün başarıyla sepete eklendi!', 'success');
            updateCartBadge();
        } else {
            showNotification('Ürün sepete eklenirken bir hata oluştu.', 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('Ürün sepete eklenirken bir hata oluştu.', 'error');
    });
}

// Update cart badge
function updateCartBadge() {
    // This would typically fetch cart count from server
    const cartBadge = document.querySelector('.cart-badge');
    if (cartBadge) {
        const currentCount = parseInt(cartBadge.textContent) || 0;
        cartBadge.textContent = currentCount + 1;
    }
}