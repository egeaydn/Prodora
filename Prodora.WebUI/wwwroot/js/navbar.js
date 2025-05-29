/**
 * PRODORA Navbar JavaScript
 * Handles mobile navigation, dropdowns, search functionality, and animations
 */

class ProdoraNavbar {
    constructor() {
        this.init();
        this.bindEvents();
        this.handleScroll();
    }

    init() {
        // Get DOM elements
        this.navbar = document.getElementById('mainNavbar');
        this.mobileToggle = document.getElementById('mobileMenuToggle');
        this.navbarMenu = document.getElementById('navbarMenu');
        this.mobileOverlay = document.getElementById('mobileOverlay');
        this.searchInput = document.getElementById('searchInput');
        this.cartBadge = document.getElementById('cartBadge');

        // Get all dropdown elements
        this.dropdownToggles = document.querySelectorAll('[data-dropdown]');
        this.dropdownMenus = document.querySelectorAll('.dropdown-menu');

        // State management
        this.isMobileMenuOpen = false;
        this.activeDropdown = null;
        this.scrollPosition = 0;

        // Initialize cart badge
        this.updateCartBadge();

        // Set initial scroll state
        this.updateScrollState();
    }

    bindEvents() {
        // Mobile menu toggle
        if (this.mobileToggle) {
            this.mobileToggle.addEventListener('click', (e) => {
                e.preventDefault();
                this.toggleMobileMenu();
            });
        }

        // Mobile overlay click
        if (this.mobileOverlay) {
            this.mobileOverlay.addEventListener('click', () => {
                this.closeMobileMenu();
            });
        }

        // Dropdown toggles
        this.dropdownToggles.forEach(toggle => {
            toggle.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.toggleDropdown(toggle.getAttribute('data-dropdown'));
            });
        });

        // Search functionality
        if (this.searchInput) {
            this.searchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    this.handleSearch();
                }
            });

            // Search suggestions (if needed)
            this.searchInput.addEventListener('input', (e) => {
                this.handleSearchInput(e.target.value);
            });
        }

        // Close dropdowns when clicking outside
        document.addEventListener('click', (e) => {
            if (!e.target.closest('.nav-dropdown') && !e.target.closest('.user-dropdown')) {
                this.closeAllDropdowns();
            }
        });

        // Handle escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.closeAllDropdowns();
                if (this.isMobileMenuOpen) {
                    this.closeMobileMenu();
                }
            }
        });

        // Handle window resize
        window.addEventListener('resize', () => {
            this.handleResize();
        });

        // Handle scroll
        window.addEventListener('scroll', () => {
            this.handleScroll();
        });

        // Handle navigation link clicks
        document.querySelectorAll('.nav-link-item').forEach(link => {
            link.addEventListener('click', (e) => {
                if (!link.classList.contains('dropdown-toggle')) {
                    this.handleNavLinkClick(link);
                }
            });
        });
    }

    toggleMobileMenu() {
        this.isMobileMenuOpen = !this.isMobileMenuOpen;

        if (this.isMobileMenuOpen) {
            this.openMobileMenu();
        } else {
            this.closeMobileMenu();
        }
    }

    openMobileMenu() {
        this.mobileToggle.classList.add('active');
        this.navbarMenu.classList.add('show');
        this.mobileOverlay.classList.add('show');
        document.body.style.overflow = 'hidden';

        // Add animation delay for menu items
        const menuItems = this.navbarMenu.querySelectorAll('.nav-link-item');
        menuItems.forEach((item, index) => {
            setTimeout(() => {
                item.classList.add('fade-in');
            }, index * 50);
        });
    }

    closeMobileMenu() {
        this.isMobileMenuOpen = false;
        this.mobileToggle.classList.remove('active');
        this.navbarMenu.classList.remove('show');
        this.mobileOverlay.classList.remove('show');
        document.body.style.overflow = '';
        this.closeAllDropdowns();

        // Remove animation classes
        const menuItems = this.navbarMenu.querySelectorAll('.nav-link-item');
        menuItems.forEach(item => {
            item.classList.remove('fade-in');
        });
    }

    toggleDropdown(dropdownId) {
        const dropdown = document.getElementById(dropdownId);
        const toggle = document.querySelector(`[data-dropdown="${dropdownId}"]`);

        if (!dropdown || !toggle) return;

        const isActive = dropdown.classList.contains('show');

        // Close all other dropdowns
        this.closeAllDropdowns();

        if (!isActive) {
            // Open this dropdown
            dropdown.classList.add('show', 'slide-down');
            toggle.classList.add('active');
            this.activeDropdown = dropdownId;

            // Focus management for accessibility
            const firstItem = dropdown.querySelector('.dropdown-item');
            if (firstItem) {
                setTimeout(() => firstItem.focus(), 100);
            }
        }
    }

    closeAllDropdowns() {
        this.dropdownMenus.forEach(menu => {
            menu.classList.remove('show', 'slide-down');
        });

        this.dropdownToggles.forEach(toggle => {
            toggle.classList.remove('active');
        });

        this.activeDropdown = null;
    }

    handleSearch() {
        const query = this.searchInput.value.trim();
        if (query) {
            // Show loading state
            this.searchInput.classList.add('loading');

            // Redirect to search results or trigger search
            // You can customize this based on your search implementation
            window.location.href = `/products/search?q=${encodeURIComponent(query)}`;
        }
    }

    handleSearchInput(value) {
        // Implement search suggestions here if needed
        // This is where you could add autocomplete functionality
        if (value.length > 2) {
            // Debounce the search suggestions
            clearTimeout(this.searchTimeout);
            this.searchTimeout = setTimeout(() => {
                this.fetchSearchSuggestions(value);
            }, 300);
        }
    }

    fetchSearchSuggestions(query) {
        // Implement search suggestions API call here
        // This would typically call your backend search endpoint
        console.log('Fetching suggestions for:', query);
    }

    handleNavLinkClick(link) {
        // Add visual feedback
        link.classList.add('loading');

        // Remove active state from other links
        document.querySelectorAll('.nav-link-item').forEach(item => {
            item.classList.remove('active');
        });

        // Add active state to clicked link
        link.classList.add('active');

        // Close mobile menu if open
        if (this.isMobileMenuOpen) {
            this.closeMobileMenu();
        }

        // Remove loading state after navigation
        setTimeout(() => {
            link.classList.remove('loading');
        }, 500);
    }

    handleResize() {
        // Close mobile menu on desktop
        if (window.innerWidth > 768 && this.isMobileMenuOpen) {
            this.closeMobileMenu();
        }

        // Close dropdowns on mobile
        if (window.innerWidth <= 768) {
            this.closeAllDropdowns();
        }
    }

    handleScroll() {
        const currentScroll = window.pageYOffset;

        // Update navbar scroll state
        this.updateScrollState();

        // Hide/show navbar on scroll (optional)
        if (currentScroll > this.scrollPosition && currentScroll > 100) {
            // Scrolling down
            this.navbar.style.transform = 'translateY(-100%)';
        } else {
            // Scrolling up
            this.navbar.style.transform = 'translateY(0)';
        }

        this.scrollPosition = currentScroll;
    }

    updateScrollState() {
        if (window.pageYOffset > 50) {
            this.navbar.classList.add('scrolled');
        } else {
            this.navbar.classList.remove('scrolled');
        }
    }

    updateCartBadge() {
        // This would typically fetch the cart count from your backend
        // For now, we'll use localStorage or a default value
        const cartCount = this.getCartCount();
        if (this.cartBadge) {
            this.cartBadge.textContent = cartCount;
            this.cartBadge.style.display = cartCount > 0 ? 'flex' : 'none';
        }
    }

    getCartCount() {
        // Implement your cart count logic here
        // This could come from localStorage, session, or an API call
        try {
            const cart = JSON.parse(localStorage.getItem('cart')) || [];
            return cart.reduce((total, item) => total + (item.quantity || 1), 0);
        } catch {
            return 0;
        }
    }

    // Public method to update cart badge from other parts of the application
    refreshCartBadge() {
        this.updateCartBadge();
    }

    // Public method to set active navigation item
    setActiveNavItem(path) {
        document.querySelectorAll('.nav-link-item').forEach(item => {
            item.classList.remove('active');
            if (item.getAttribute('href') === path) {
                item.classList.add('active');
            }
        });
    }

    // Public method to show notifications
    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `navbar-notification ${type}`;
        notification.textContent = message;

        // Add to navbar
        this.navbar.appendChild(notification);

        // Animate in
        setTimeout(() => notification.classList.add('show'), 100);

        // Remove after delay
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }
}

// Initialize navbar when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.prodoraNavbar = new ProdoraNavbar();
});

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ProdoraNavbar;
}

// Global utility functions for navbar
window.updateCartBadge = () => {
    if (window.prodoraNavbar) {
        window.prodoraNavbar.refreshCartBadge();
    }
};

window.setActiveNavItem = (path) => {
    if (window.prodoraNavbar) {
        window.prodoraNavbar.setActiveNavItem(path);
    }
};

window.showNavNotification = (message, type) => {
    if (window.prodoraNavbar) {
        window.prodoraNavbar.showNotification(message, type);
    }
};
