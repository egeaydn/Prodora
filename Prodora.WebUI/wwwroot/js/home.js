// Stats Counter Animation
function animateStats() {
    const stats = document.querySelectorAll('.stat-number');

    stats.forEach(stat => {
        const target = parseInt(stat.getAttribute('data-count'));
        const increment = target / 100;
        let current = 0;

        const timer = setInterval(() => {
            current += increment;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }
            stat.textContent = Math.floor(current).toLocaleString();
        }, 20);
    });
}

// Site Entry Function
function enterSite() {
    document.body.style.transform = 'scale(1.1)';
    document.body.style.opacity = '0';
    document.body.style.transition = 'all 1s ease';

    setTimeout(() => {
        window.location.href = '/'; // Ana sayfaya yönlendir
    }, 1000);
}

// Initialize animations
document.addEventListener('DOMContentLoaded', function () {
    // Start stats animation after a delay
    setTimeout(animateStats, 2000);

    // Loading screen removed - immediate display

    // Auto-redirect after 8 seconds (optional)
   
});

// Parallax effect for mouse movement
document.addEventListener('mousemove', function (e) {
    const particles = document.querySelectorAll('.particle');
    const x = e.clientX / window.innerWidth;
    const y = e.clientY / window.innerHeight;

    particles.forEach((particle, index) => {
        const speed = (index + 1) * 0.5;
        particle.style.transform = `translate(${x * speed}px, ${y * speed}px)`;
    });
});

// Keyboard navigation
document.addEventListener('keydown', function (e) {
    if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        enterSite();
    }
});