document.addEventListener('DOMContentLoaded', () => {
    // Ürün verilerini localStorage'dan al
    const productData = JSON.parse(localStorage.getItem('selectedProduct'));


    // Ürün bilgilerini sayfaya yerleştir
    document.getElementById('mainImage').src = productData.image;
    document.getElementById('productName').textContent = productData.name;
    document.getElementById('productBrand').textContent = productData.brand;
    document.getElementById('productRating').innerHTML = productData.rating;
    document.getElementById('productPrice').textContent = productData.discountedPrice;

    if (productData.originalPrice) {
        document.getElementById('originalPrice').textContent = productData.originalPrice;
    } else {
        document.getElementById('originalPrice').style.display = 'none';
    }

    if (productData.discount) {
        document.getElementById('discountBadge').textContent = productData.discount;
    } else {
        document.getElementById('discountBadge').style.display = 'none';
    }

    // Favori butonu işlemleri
    const favoriteBtn = document.getElementById('favoriteBtn');
    const heartIcon = favoriteBtn.querySelector('i');

    // Favori durumunu kontrol et
    if (favoriteManager.isFavorite(productData.name)) {
        favoriteBtn.classList.add('active');
        heartIcon.classList.remove('far');
        heartIcon.classList.add('fas');
    }

    // Favori butonuna tıklama olayı
    favoriteBtn.addEventListener('click', () => {
        favoriteBtn.classList.toggle('active');

        if (favoriteBtn.classList.contains('active')) {
            heartIcon.classList.remove('far');
            heartIcon.classList.add('fas');
            favoriteManager.addFavorite(productData);
        } else {
            heartIcon.classList.remove('fas');
            heartIcon.classList.add('far');
            favoriteManager.removeFavorite(favoriteManager.favorites.findIndex(fav => fav.name === productData.name));
        }
    });

    // Küçük resimlere tıklama olayı
    const thumbnails = document.querySelectorAll('.thumbnail');
    const mainImage = document.getElementById('mainImage');

    thumbnails.forEach(thumb => {
        thumb.addEventListener('click', () => {
            // Aktif sınıfını güncelle
            thumbnails.forEach(t => t.classList.remove('active'));
            thumb.classList.add('active');

            // Ana görseli güncelle
            mainImage.src = thumb.src;
        });
    });

    // Yorum işlevselliği
    const commentForm = document.getElementById('commentForm');
    const reviewsList = document.querySelector('.reviews-list');
    const sortReviews = document.getElementById('sortReviews');

    // Yorumları localStorage'dan al
    let reviews = JSON.parse(localStorage.getItem('productReviews')) || {};
    const productId = getProductIdFromUrl(); // URL'den ürün ID'sini al

    // Mevcut yorumları göster
    function displayReviews() {
        const productReviews = reviews[productId] || [];
        reviewsList.innerHTML = ''; // Mevcut yorumları temizle

        productReviews.forEach(review => {
            const reviewElement = createReviewElement(review);
            reviewsList.appendChild(reviewElement);
        });
    }

    // Yeni yorum elementi oluştur
    function createReviewElement(review) {
        const div = document.createElement('div');
        div.className = 'review-item';
        div.innerHTML = `
            <div class="review-header">
                <div class="reviewer-info">
                    <span class="reviewer-name">${review.name}</span>
                    <span class="review-date">${review.date}</span>
                </div>
                <div class="review-rating">${'★'.repeat(review.rating)}</div>
            </div>
            <h4 class="review-title">${review.title}</h4>
            <p class="review-content">${review.content}</p>
            <div class="review-footer">
                <button class="helpful-btn" data-helpful="${review.helpful || 0}">
                    <i class="fas fa-thumbs-up"></i>
                    <span>Faydalı (${review.helpful || 0})</span>
                </button>
            </div>
        `;

        // Faydalı butonu işlevselliği
        const helpfulBtn = div.querySelector('.helpful-btn');
        helpfulBtn.addEventListener('click', () => {
            review.helpful = (review.helpful || 0) + 1;
            helpfulBtn.querySelector('span').textContent = `Faydalı (${review.helpful})`;
            saveReviews();
        });

        return div;
    }

    // Yorumları kaydet
    function saveReviews() {
        localStorage.setItem('productReviews', JSON.stringify(reviews));
    }

    // Yorum formunu gönderme
    commentForm.addEventListener('submit', (e) => {
        e.preventDefault();

        const rating = document.querySelector('input[name="rating"]:checked').value;
        const title = document.getElementById('reviewTitle').value;
        const content = document.getElementById('reviewContent').value;

        const newReview = {
            name: 'Kullanıcı', // Gerçek uygulamada kullanıcı adı buraya gelecek
            date: new Date().toLocaleDateString('tr-TR'),
            rating: parseInt(rating),
            title: title,
            content: content,
            helpful: 0
        };

        // Ürüne ait yorumları al veya yeni array oluştur
        if (!reviews[productId]) {
            reviews[productId] = [];
        }

        // Yeni yorumu ekle
        reviews[productId].unshift(newReview);
        saveReviews();
        displayReviews();

        // Formu temizle
        commentForm.reset();

        // Başarılı mesajı göster
        showNotification('Yorumunuz başarıyla eklendi!');
    });

    // Yorumları sırala
    sortReviews.addEventListener('change', () => {
        const productReviews = reviews[productId] || [];

        switch (sortReviews.value) {
            case 'newest':
                productReviews.sort((a, b) => new Date(b.date) - new Date(a.date));
                break;
            case 'highest':
                productReviews.sort((a, b) => b.rating - a.rating);
                break;
            case 'lowest':
                productReviews.sort((a, b) => a.rating - b.rating);
                break;
        }

        displayReviews();
    });

    // Bildirim göster
    function showNotification(message) {
        const notification = document.createElement('div');
        notification.className = 'notification';
        notification.textContent = message;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.remove();
        }, 3000);
    }

    // URL'den ürün ID'sini al
    function getProductIdFromUrl() {
        // Gerçek uygulamada URL'den ürün ID'si parse edilecek
        return window.location.pathname.split('/').pop() || 'default';
    }

    // Sayfa yüklendiğinde mevcut yorumları göster
    displayReviews();
}); 