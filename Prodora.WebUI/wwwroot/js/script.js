document.addEventListener('DOMContentLoaded', () => {
    // Filtreleme paneli için gerekli değişkenler
    const filterSections = document.querySelectorAll('.filter-section');
    const clearFiltersBtn = document.querySelector('.clear-filters');
    const priceRange = document.getElementById('priceRange');
    const minPriceInput = document.getElementById('minPrice');
    const maxPriceInput = document.getElementById('maxPrice');
    const sortSelect = document.getElementById('sortSelect');
    const productCards = document.querySelectorAll('.product-card');
    const filteredCount = document.getElementById('filteredCount');
    const categoryMenuItems = document.querySelectorAll('.category-menu li a');
    const searchInput = document.querySelector('.nav-search input');
    const searchButton = document.querySelector('.nav-search button');

    // Filtreleme durumunu tutacak nesne
    let filterState = {
        categories: [],
        subCategories: [],
        priceRange: { min: 0, max: 5000 },
        brands: [],
        sizes: [],
        colors: [],
        discounts: [],
        shipping: [],
        sortBy: 'popular'
    };

    // Navbar kategori menüsü için olay dinleyicileri
    categoryMenuItems.forEach(item => {
        item.addEventListener('click', (e) => {
            e.preventDefault();
            const category = e.target.textContent.toLowerCase();

            // Diğer tüm kategorileri devre dışı bırak
            categoryMenuItems.forEach(menuItem => {
                menuItem.classList.remove('active');
            });

            // Seçilen kategoriyi aktif et
            e.target.classList.add('active');

            // Filtre durumunu güncelle
            filterState.categories = [category];

            // Filtre panelindeki kategori checkbox'ını güncelle
            const categoryCheckbox = document.querySelector(`.filter-section:first-child input[value="${category}"]`);
            if (categoryCheckbox) {
                categoryCheckbox.checked = true;
            }

            // Kategoriye göre ürünleri filtrele
            productCards.forEach(card => {
                const cardCategory = card.getAttribute('data-category');
                if (cardCategory === category) {
                    card.style.display = 'block';
                } else {
                    card.style.display = 'none';
                }
            });

            // Görünen ürün sayısını güncelle
            const visibleCount = document.querySelectorAll('.product-card[style*="display: block"]').length;
            filteredCount.textContent = visibleCount;
        });
    });

    // Filtre bölümlerini aç/kapat
    filterSections.forEach(section => {
        const title = section.querySelector('.filter-title');
        title.addEventListener('click', () => {
            section.classList.toggle('active');
        });
    });

    // Fiyat aralığı slider'ını güncelle
    priceRange.addEventListener('input', (e) => {
        const value = e.target.value;
        maxPriceInput.value = value;
        filterState.priceRange.max = parseInt(value);
        applyFilters();
    });

    // Minimum fiyat input'unu güncelle
    minPriceInput.addEventListener('change', (e) => {
        const value = parseInt(e.target.value) || 0;
        filterState.priceRange.min = value;
        applyFilters();
    });

    // Maksimum fiyat input'unu güncelle
    maxPriceInput.addEventListener('change', (e) => {
        const value = parseInt(e.target.value) || 5000;
        filterState.priceRange.max = value;
        priceRange.value = value;
        applyFilters();
    });

    // Checkbox'ları dinle
    document.querySelectorAll('.checkbox-container input, .color-option input').forEach(checkbox => {
        checkbox.addEventListener('change', (e) => {
            const value = e.target.value;
            const type = e.target.closest('.filter-section').querySelector('h4').textContent.toLowerCase();

            if (e.target.checked) {
                addToFilterState(type, value);
            } else {
                removeFromFilterState(type, value);
            }

            applyFilters();
        });
    });

    // Sıralama seçeneğini dinle
    sortSelect.addEventListener('change', (e) => {
        filterState.sortBy = e.target.value;
        applyFilters();
    });

    // Filtreleri temizle
    clearFiltersBtn.addEventListener('click', () => {
        // Tüm checkbox'ları temizle
        document.querySelectorAll('.checkbox-container input, .color-option input').forEach(checkbox => {
            checkbox.checked = false;
        });

        // Fiyat aralığını sıfırla
        priceRange.value = 5000;
        minPriceInput.value = '';
        maxPriceInput.value = '';

        // Sıralamayı varsayılana döndür
        sortSelect.value = 'popular';

        // Navbar'daki aktif kategoriyi temizle
        categoryMenuItems.forEach(item => {
            item.classList.remove('active');
        });

        // Filtre durumunu sıfırla
        filterState = {
            categories: [],
            subCategories: [],
            priceRange: { min: 0, max: 5000 },
            brands: [],
            sizes: [],
            colors: [],
            discounts: [],
            shipping: [],
            sortBy: 'popular'
        };

        applyFilters();
    });

    // Filtre durumuna değer ekle
    function addToFilterState(type, value) {
        switch (type) {
            case 'kategoriler':
                filterState.categories.push(value);
                break;
            case 'alt kategoriler':
                filterState.subCategories.push(value);
                break;
            case 'markalar':
                filterState.brands.push(value);
                break;
            case 'bedenler':
                filterState.sizes.push(value);
                break;
            case 'renkler':
                filterState.colors.push(value);
                break;
            case 'indirim oranı':
                filterState.discounts.push(value);
                break;
            case 'kargo':
                filterState.shipping.push(value);
                break;
        }
    }

    // Filtre durumundan değer çıkar
    function removeFromFilterState(type, value) {
        switch (type) {
            case 'kategoriler':
                filterState.categories = filterState.categories.filter(v => v !== value);
                break;
            case 'alt kategoriler':
                filterState.subCategories = filterState.subCategories.filter(v => v !== value);
                break;
            case 'markalar':
                filterState.brands = filterState.brands.filter(v => v !== value);
                break;
            case 'bedenler':
                filterState.sizes = filterState.sizes.filter(v => v !== value);
                break;
            case 'renkler':
                filterState.colors = filterState.colors.filter(v => v !== value);
                break;
            case 'indirim oranı':
                filterState.discounts = filterState.discounts.filter(v => v !== value);
                break;
            case 'kargo':
                filterState.shipping = filterState.shipping.filter(v => v !== value);
                break;
        }
    }

    // Filtreleri uygula
    function applyFilters() {
        let visibleCount = 0;

        productCards.forEach(card => {
            const price = parseFloat(card.querySelector('.discounted').textContent.replace('TL', '').replace('.', '').replace(',', '.'));
            const brand = card.querySelector('.brand').textContent.toLowerCase();
            const discount = card.querySelector('.discount')?.textContent.replace('%', '');
            const hasFreeShipping = card.querySelector('.cargo-free') !== null;

            // Fiyat filtresi
            if (price < filterState.priceRange.min || price > filterState.priceRange.max) {
                card.style.display = 'none';
                return;
            }

            // Marka filtresi
            if (filterState.brands.length > 0 && !filterState.brands.includes(brand)) {
                card.style.display = 'none';
                return;
            }

            // İndirim filtresi
            if (filterState.discounts.length > 0) {
                const hasMatchingDiscount = filterState.discounts.some(d => {
                    return discount && parseInt(discount) >= parseInt(d);
                });
                if (!hasMatchingDiscount) {
                    card.style.display = 'none';
                    return;
                }
            }

            // Kargo filtresi
            if (filterState.shipping.includes('free-shipping') && !hasFreeShipping) {
                card.style.display = 'none';
                return;
            }

            // Diğer filtreler buraya eklenebilir...

            card.style.display = 'block';
            visibleCount++;
        });

        // Görünen ürün sayısını güncelle
        filteredCount.textContent = visibleCount;

        // Sıralama işlemi
        const container = document.querySelector('.container');
        const cards = Array.from(productCards).filter(card => card.style.display !== 'none');

        cards.sort((a, b) => {
            const priceA = parseFloat(a.querySelector('.discounted').textContent.replace('TL', '').replace('.', '').replace(',', '.'));
            const priceB = parseFloat(b.querySelector('.discounted').textContent.replace('TL', '').replace('.', '').replace(',', '.'));
            const ratingA = parseInt(a.querySelector('.rating-count').textContent.replace(/[^0-9]/g, ''));
            const ratingB = parseInt(b.querySelector('.rating-count').textContent.replace(/[^0-9]/g, ''));

            switch (filterState.sortBy) {
                case 'price-asc':
                    return priceA - priceB;
                case 'price-desc':
                    return priceB - priceA;
                case 'rating':
                    return ratingB - ratingA;
                case 'newest':
                    // Burada ürün ID'si veya tarih bilgisi kullanılabilir
                    return 0;
                default:
                    return 0;
            }
        });

        // Sıralanmış kartları DOM'a ekle
        cards.forEach(card => container.appendChild(card));
    }

    // Favori butonunu yönetme
    const favoriteButtons = document.querySelectorAll('.product-favorite');
    favoriteButtons.forEach(btn => {
        const card = btn.closest('.product-card');
        const productData = {
            brand: card.querySelector('.brand').textContent,
            name: card.querySelector('.name').textContent,
            image: card.querySelector('.product-image img').src,
            rating: card.querySelector('.rating').innerHTML,
            originalPrice: card.querySelector('.original')?.textContent || '',
            discountedPrice: card.querySelector('.discounted').textContent,
            discount: card.querySelector('.discount')?.textContent || '',
            cargoFree: card.querySelector('.cargo-free') ? true : false
        };

        // Favori durumunu kontrol et
        if (favoriteManager.isFavorite(productData.name)) {
            btn.classList.add('active');
            const heartIcon = btn.querySelector('i');
            heartIcon.classList.remove('far');
            heartIcon.classList.add('fas');
        }

        btn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();

            btn.classList.toggle('active');
            const heartIcon = btn.querySelector('i');

            if (btn.classList.contains('active')) {
                heartIcon.classList.remove('far');
                heartIcon.classList.add('fas');
                favoriteManager.addFavorite(productData);
            } else {
                heartIcon.classList.remove('fas');
                heartIcon.classList.add('far');
                favoriteManager.removeFavorite(favoriteManager.favorites.findIndex(fav => fav.name === productData.name));
            }
        });
    });

    // Ürün kartlarına tıklama olayı
    productCards.forEach(card => {
        card.addEventListener('click', (e) => {
            // Favori butonuna tıklanmadıysa detay sayfasına git
            if (!e.target.closest('.product-favorite')) {
                const productData = {
                    brand: card.querySelector('.brand').textContent,
                    name: card.querySelector('.name').textContent,
                    image: card.querySelector('.product-image img').src,
                    rating: card.querySelector('.rating').innerHTML,
                    originalPrice: card.querySelector('.original')?.textContent || '',
                    discountedPrice: card.querySelector('.discounted').textContent,
                    discount: card.querySelector('.discount')?.textContent || '',
                    cargoFree: card.querySelector('.cargo-free') ? true : false
                };

                localStorage.setItem('selectedProduct', JSON.stringify(productData));
                window.location.href = 'product-detail.html';
            }
        });
    });

    // Hızlı görünüm butonunu yönetme
    const quickViewBtn = document.querySelector('.quick-view');
    quickViewBtn.addEventListener('click', () => {
        alert('Hızlı görünüm özelliği açıldı!');
        // Burada modal veya popup açılabilir
    });

    // Görsel yükleme hatası durumunda yedek görsel
    const productImage = document.querySelector('.product-image img');
    productImage.addEventListener('error', () => {
        productImage.src = 'https://via.placeholder.com/400x500?text=Ürün+Görseli';
    });

    // Arama fonksiyonu
    function searchProducts(query) {
        query = query.toLowerCase().trim();
        let visibleCount = 0;

        // Sonuç bulunamadı mesajını temizle
        const existingMessage = document.querySelector('.no-results-message');
        if (existingMessage) {
            existingMessage.remove();
        }

        productCards.forEach(card => {
            const brand = card.querySelector('.brand').textContent.toLowerCase();
            const name = card.querySelector('.name').textContent.toLowerCase();
            const isVisible = brand.includes(query) || name.includes(query);

            card.style.display = isVisible ? 'block' : 'none';
            if (isVisible) visibleCount++;
        });

        // Görünen ürün sayısını güncelle
        if (filteredCount) {
            filteredCount.textContent = visibleCount;
        }

        // Sonuç bulunamadı mesajı
        if (visibleCount === 0) {
            const message = document.createElement('div');
            message.className = 'no-results-message';
            message.innerHTML = `
                <i class="fas fa-search"></i>
                <p>Aramanızla eşleşen ürün bulunamadı.</p>
                <p>Lütfen farklı bir arama yapın.</p>
            `;
            document.querySelector('.container').appendChild(message);
        }
    }

    // Arama butonu tıklama olayı
    if (searchButton) {
        searchButton.addEventListener('click', () => {
            searchProducts(searchInput.value);
        });
    }

    // Enter tuşu ile arama
    if (searchInput) {
        searchInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                searchProducts(searchInput.value);
            }
        });

        // Gerçek zamanlı arama
        let searchTimeout;
        searchInput.addEventListener('input', (e) => {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                searchProducts(e.target.value);
            }, 300);
        });
    }

    // Slider işlevselliği
    const slider = document.querySelector('.slider');
    const slides = document.querySelectorAll('.slide');
    const dots = document.querySelectorAll('.dot');
    const prevBtn = document.querySelector('.slider-btn.prev');
    const nextBtn = document.querySelector('.slider-btn.next');
    let currentSlide = 0;
    let slideInterval;

    function showSlide(index) {
        slides.forEach(slide => slide.classList.remove('active'));
        dots.forEach(dot => dot.classList.remove('active'));

        slides[index].classList.add('active');
        dots[index].classList.add('active');
    }

    function nextSlide() {
        currentSlide = (currentSlide + 1) % slides.length;
        showSlide(currentSlide);
    }

    function prevSlide() {
        currentSlide = (currentSlide - 1 + slides.length) % slides.length;
        showSlide(currentSlide);
    }

    function startSlideShow() {
        slideInterval = setInterval(nextSlide, 5000);
    }

    function stopSlideShow() {
        clearInterval(slideInterval);
    }

    // Slider olayları
    prevBtn.addEventListener('click', () => {
        prevSlide();
        stopSlideShow();
        startSlideShow();
    });

    nextBtn.addEventListener('click', () => {
        nextSlide();
        stopSlideShow();
        startSlideShow();
    });

    dots.forEach((dot, index) => {
        dot.addEventListener('click', () => {
            currentSlide = index;
            showSlide(currentSlide);
            stopSlideShow();
            startSlideShow();
        });
    });

    // Mouse hover durumunda slider'ı durdur
    slider.addEventListener('mouseenter', stopSlideShow);
    slider.addEventListener('mouseleave', startSlideShow);

    // Otomatik slider başlat
    startSlideShow();
}); 